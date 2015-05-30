/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) 2015 Keno Schwalb
 */

using UnityEngine;
using System.Collections;
using System.IO;
using System;
using UnityImageProcessing;
using System.Threading;

public class CameraView : MonoBehaviour {
	// display
	public static DisplayMode displayMode = DisplayMode.Normal;
	private static bool displayModeChanged = false;
	public static bool scaleImage = true;

	// calibration
	public static bool startCalibration = false;
	private bool calibrating = false;
	private float calibrationStartTime;

	public static void setDisplayMode(DisplayMode displayMode) {
		CameraView.displayMode = displayMode;
		// update camera texture
		displayModeChanged = true;
	}

	public enum DisplayMode {
		Tracking,
		Edge,
		Binary,
		Euclidean,
		Normal
	}

	private WebCamTexture webcamTexture;
	private Renderer noMarkerRenderer = null;
	private GameObject noMarker = null;
	private string noMarkerText = null;
	private Color markerModelColor = new Color();
	private bool initialised = false;
	
	public bool Processing {
		get;
		set;
	}

	public Image ProcessedImage {
		get;
		set;
	}

	public Rectangle MarkerRect {
		get;
		set;
	}

	void Start () {
		// initialise the front camera and map it to the plane
		webcamTexture = new WebCamTexture ();
		GetComponent<Renderer> ().material.mainTexture = webcamTexture;
		webcamTexture.Play ();
		
		Processing = false;
		noMarker = GameObject.Find ("NoMarker");
		noMarkerRenderer = noMarker.GetComponent<Renderer> ();
		markerModelColor = noMarkerRenderer.material.color;
		noMarkerText = noMarker.GetComponent<TextMesh> ().text;
		noMarker.GetComponent<TextMesh> ().text = "Model not initialised";
	}

	// Use this for initialization
	void init () {
		noMarker.GetComponent<TextMesh> ().text = noMarkerText;
		initialised = true;
	}

	// Update is called once per frame
	void Update () {
		if (Startup.config == null || Startup.config.currentObject == null) {
			return;
		}

		if (!initialised) {
			init ();
		}

		// calibration
		if (startCalibration || calibrating) {
			if(startCalibration) {
				startCalibration = false;

				// reset to normal view
				GetComponent<Renderer> ().material.mainTexture = webcamTexture;

				// show object and text
				noMarkerRenderer.material.color = markerModelColor;
				Startup.config.currentObject.SetActive (true);

				// place object in the middle of the screen as pointer
				Vector3 position = new Vector3 (0, 0, Startup.config.currentObject.transform.localPosition.z);
				Startup.config.currentObject.transform.localPosition = position;

				// start countdown
				calibrationStartTime = Time.time;
				noMarker.GetComponent<TextMesh> ().text = "3";
				calibrating = true;
			}

			if(calibrating) {
				// check time, update timer, get middle pixel and find colour
				float seconds = Time.time - calibrationStartTime;

				if(seconds >= 4) {
					calibrating = false;
					noMarker.GetComponent<TextMesh> ().text = noMarkerText;
				} else if(seconds >= 3) {
					// get mid point of webcam image
					int x = (int) (webcamTexture.width * 0.5f);
					int y = (int) (webcamTexture.height * 0.5f);

					Image image = new Image(webcamTexture.GetPixels32 (), webcamTexture.width, webcamTexture.height);
					Startup.config.analysis.markerColor = image.getPixel(x, y);
				} else if(seconds >= 2) {
					noMarker.GetComponent<TextMesh> ().text = "1";
				} else if(seconds >= 1) {
					noMarker.GetComponent<TextMesh> ().text = "2";
				}
			}

			return;
		}

		// restart the image processing thread if it's not running
		if (!Processing) {
			StartProcessing ();
		}

		// if we have image data available
		if (ProcessedImage != null) {
			Texture2D tex2d = ProcessedImage.GetTexture2D ();

			// if object was found
			if (MarkerRect != null ) {
				// update the objects position
				Vector2 markerCoordinate = getRelativeCoordinateFromTextureCoordinate (MarkerRect.MidPointX, MarkerRect.MidPointY, tex2d); // (320, 240);
				Vector3 position = new Vector3 (markerCoordinate.x, markerCoordinate.y, Startup.config.currentObject.transform.localPosition.z);
				Startup.config.currentObject.transform.localPosition = position;

				// hide text and display the object
				noMarkerRenderer.material.color = new Color32 (1, 1, 1, 0);
				Startup.config.currentObject.SetActive (true);
			}

			// check whether or not we are supposed to display the processed image
			if(displayMode != DisplayMode.Normal) {
				GetComponent<Renderer> ().material.mainTexture = tex2d;
			}
		}

		// check whether or not we have to reset to display the normal image
		if (displayModeChanged && displayMode == DisplayMode.Normal) {
			GetComponent<Renderer> ().material.mainTexture = webcamTexture;
		}

		if(displayModeChanged) {
			displayModeChanged = false;
		}

		// if object was not found
		if (MarkerRect == null || ProcessedImage == null) {
			// show text and hide the object
			noMarkerRenderer.material.color = markerModelColor;
			Startup.config.currentObject.SetActive (false);
		}
	}

	void StartProcessing() {
		Processing = true;

		Image image = Image.FromWebCamTexture (webcamTexture);
		ImageProcessor processor = new ImageProcessor (image, this);
		Thread processorThread = new Thread (new ThreadStart (processor.ThreadRun));

		try {
			processorThread.Start();
		} catch(Exception e) {
			Debug.Log ("Could not start thread: " + e.ToString ());
		}
	}

	Vector2 getRelativeCoordinateFromTextureCoordinate(int x, int y, Texture2D texture) {
		Vector2 relativeCoordinate = new Vector2 ();

		// convert x & y from top-left based to center-based coordinates
		float xCenterBased = x - (texture.width / 2.0f);
		float yCenterBased = y - (texture.height / 2.0f);
		float xScaled = xCenterBased / (1.5f * texture.width) * (-1);
		float yScaled = yCenterBased / (1.5f * texture.height) * (-1);

		relativeCoordinate.x = xScaled;
		relativeCoordinate.y = yScaled;

		return relativeCoordinate;
	}
}

class ImageProcessor {
	Config config;
	Image image;
	CameraView cameraView;
	EuclideanFilter euclideanFilter;
	BinaryFilter binaryFilter;
	BlobFinder blobFinder;
	ImageScaler imageScaler;

	public ImageProcessor(Image image, CameraView cameraview) {
		this.config = Startup.config;
		this.image = image;
		this.cameraView = cameraview;

		euclideanFilter = new EuclideanFilter (config.analysis.markerColor, config.analysis.deviation);
		binaryFilter = new BinaryFilter (config.analysis.binaryThreshold);
		blobFinder = new BlobFinder ();
		blobFinder.MinWidth = config.analysis.markerMinWidth;
		blobFinder.MinHeight = config.analysis.markerMinHeight;
		imageScaler = new ImageScaler (config.analysis.scaleFactor);
	}

	public void ThreadRun() {
		try {
			Image processedImage = null;

			// scale image down to make processing faster
			Image processed  = image;

			if(CameraView.scaleImage) {
				processed = imageScaler.Process(image);
			}

			// make a copy of the scaled image for drawing the tracker
			if(CameraView.displayMode == CameraView.DisplayMode.Tracking) {
				processedImage = copyImage(processed);
			}

			// apply euclidean filter
			euclideanFilter.ApplyInPlace (processed);
			if(CameraView.displayMode == CameraView.DisplayMode.Euclidean) {
				processedImage = copyImage(processed);
			}

			// apply grayscale filter
			GrayscaleFilter.ApplyInPlace (processed);

			// apply binaryFilter
			binaryFilter.ApplyInPlace (processed);
			if(CameraView.displayMode == CameraView.DisplayMode.Binary) {
				processedImage = copyImage(processed);
			}

			// turn to a BinaryImage and apply edge detection
			BinaryImage bin = BinaryImage.FromImage (processed);
			bin = EdgeDetection.Apply (bin);
			bin = new ImageObjectScaler(config.analysis.edgeScale).Apply(bin);
			processed = bin.GetImage ();
			if(CameraView.displayMode == CameraView.DisplayMode.Edge) {
				processedImage = copyImage(processed);
			}

			// analyse the processed image for blobs
			Rectangle[] rectangles = blobFinder.Process (processed);

			if(CameraView.displayMode == CameraView.DisplayMode.Normal) {
				processedImage = processed;
			}

			if(rectangles.Length > 0) {
				Rectangle biggest = null;
				foreach(Rectangle rect in rectangles) {
					if(biggest == null || biggest.SurfaceArea < rect.SurfaceArea) {
						biggest = rect;
					}
				}

				// check if we are supposed to display the tracking
				if(CameraView.displayMode != CameraView.DisplayMode.Normal) {
					biggest.StrokeWidth = 5;
					biggest.drawInPlace(processedImage);
				}

				// update marker in the CameraView
				cameraView.MarkerRect = biggest;
			} else {
				cameraView.MarkerRect = null;
			}

			// update processed image in the camera view
			cameraView.ProcessedImage = processedImage;
		} catch(Exception e) {
			Debug.Log (e.ToString ());
		}

		cameraView.Processing = false;
	}

	private Image copyImage(Image image) {
		Color32[] newPixels = new Color32[image.Pixels.Length];
		image.Pixels.CopyTo (newPixels, 0);

		return new Image (newPixels, image.Width, image.Height);
	}
}
