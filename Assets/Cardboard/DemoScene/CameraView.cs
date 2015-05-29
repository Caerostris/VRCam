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
	public static DisplayMode displayMode = DisplayMode.Normal;

	public enum DisplayMode {
		Tracking,
		Edge,
		Binary,
		Euclidean,
		Normal
	}

	private WebCamTexture webcamTexture;
	private Renderer noMarker = null;
	private Color markerColor = new Color();
	private GameObject obj = null;
	
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

	// Use this for initialization
	void Start () {
		// initialise the front camera and map it to the plane
		webcamTexture = new WebCamTexture ();
		GetComponent<Renderer> ().material.mainTexture = webcamTexture;
		webcamTexture.Play ();

		StartProcessing ();
		noMarker = GameObject.Find ("NoMarker").GetComponent<Renderer> ();
		markerColor = noMarker.material.color;
		obj = GameObject.Find ("Object");
	}

	// Update is called once per frame
	void Update () {
		// restart the image processing thread
		if (!Processing) {
			StartProcessing();
		}

		if (ProcessedImage != null) {
			Texture2D tex2d = ProcessedImage.GetTexture2D ();

			// if object was found
			if (MarkerRect != null ) {
				// update the objects position
				Vector2 markerCoordinate = getRelativeCoordinateFromTextureCoordinate (MarkerRect.MidPointX, MarkerRect.MidPointY, tex2d); // (320, 240);
				Vector3 position = new Vector3 (markerCoordinate.x, markerCoordinate.y, obj.transform.localPosition.z);
				obj.transform.localPosition = position;

				// hide text and display the object
				noMarker.material.color = new Color32 (1, 1, 1, 0);
				obj.SetActive (true);
			}

			// check whether or not we are supposed to display the processed image
			if (displayMode != DisplayMode.Normal) {
				GetComponent<Renderer> ().material.mainTexture = tex2d;
			}
		}

		if (MarkerRect == null) {
			// show text and hide the object
			noMarker.material.color = markerColor;
			obj.SetActive (false);
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
	Image image;
	CameraView cameraView;
	EuclideanFilter euclideanFilter;
	BinaryFilter binaryFilter;
	BlobFinder blobFinder;
	ImageScaler imageScaler;

	public ImageProcessor(Image image, CameraView cameraview) {
		this.image = image;
		this.cameraView = cameraview;

		euclideanFilter = new EuclideanFilter (new Color32 (235, 125, 35, 1), 60); // orange
		binaryFilter = new BinaryFilter (20);
		blobFinder = new BlobFinder ();
		blobFinder.MinWidth = 5;
		blobFinder.MinHeight = 5;
		imageScaler = new ImageScaler (0.1f);
	}

	public void ThreadRun() {
		try {
			Image processedImage = null;

			// scale image down to make processing faster
			Image processed  = imageScaler.Process(image);
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
			bin = new ImageObjectScaler(3).Apply(bin);
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
