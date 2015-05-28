using UnityEngine;
using System.Collections;
using System.IO;
using System;
using UnityImageProcessing;
using System.Threading;

public class CameraView : MonoBehaviour {
	private WebCamTexture webcamTexture;
	private bool processing = false;
	public Image processedImage = null;
	public Rectangle rectangle = null;
	int count = 0;
	GameObject noMarker = null;
	GameObject screen = null;
	GameObject obj = null;

	public bool Processing {
		get {
			return processing;
		}

		set {
			processing = value;
		}
	}

	// Use this for initialization
	void Start () {
		// initialise the front camera and map it to the plane
		webcamTexture = new WebCamTexture ();
		GetComponent<Renderer> ().material.mainTexture = webcamTexture;
		webcamTexture.Play ();

		StartProcessing ();
		noMarker = GameObject.Find ("NoMarker");
		obj = GameObject.Find ("Sphere");
	}

	// Update is called once per frame
	void Update () {
		if (!processing) {
			StartProcessing();
		}

		if (rectangle != null) {
			Vector2 markerCoordinate = getRelativeCoordinateFromTextureCoordinate (rectangle.MidPointX, rectangle.MidPointY); // (320, 240);
			Vector3 position = new Vector3 (markerCoordinate.x, markerCoordinate.y, obj.transform.localPosition.z);
			obj.transform.localPosition = position;

			noMarker.SetActive(false);

			Debug.Log ("Rectangle Midpoint: " + rectangle.MidPointX + " " + rectangle.MidPointY);
			Debug.Log ("Transformed:        " + markerCoordinate.x + " " + markerCoordinate.y);
		} else {
			noMarker.SetActive(true);
		}

		if (rectangle != null) {
			if(processedImage != null)
			{
				Texture2D tex2d = processedImage.GetTexture2D ();
				GetComponent<Renderer> ().material.mainTexture = tex2d;
			}

			processedImage = null;
		} else {
			GetComponent<Renderer> ().material.mainTexture = webcamTexture;
		}
	}

	void StartProcessing() {
		processing = true;

		Image image = Image.FromWebCamTexture (webcamTexture);
		ImageProcessor processor = new ImageProcessor (image, this);
		Thread processorThread = new Thread (new ThreadStart (processor.ThreadRun));

		try {
			processorThread.Start();
		} catch(Exception e) {
			Debug.Log ("Could not start thread: " + e.ToString ());
		}
	}

	Vector2 getRelativeCoordinateFromTextureCoordinate(int x, int y) {
		Vector2 relativeCoordinate = new Vector2 ();

		// convert x & y from top-left based to center-based coordinates
		float xCenterBased = x - (webcamTexture.width / 2.0f);
		float yCenterBased = y - (webcamTexture.width / 2.0f);
		float xScaled = xCenterBased / (3.3333f * webcamTexture.width) * (-1);
		float yScaled = yCenterBased / (3.3333f * webcamTexture.width) * (-1);

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

	public ImageProcessor(Image image, CameraView cameraview) {
		this.image = image;
		this.cameraView = cameraview;

		euclideanFilter = new EuclideanFilter (new Color32 (235, 125, 35, 1), 70);
		binaryFilter = new BinaryFilter (20);
		blobFinder = new BlobFinder ();
		blobFinder.MinWidth = 50;
		blobFinder.MinHeight = 50;
	}

	public void ThreadRun() {
		try {
System.Diagnostics.Stopwatch s = System.Diagnostics.Stopwatch.StartNew ();
			// apply colour filters
			Image processed = euclideanFilter.ApplyInPlace (image);
			GrayscaleFilter.ApplyInPlace (processed);
			binaryFilter.ApplyInPlace (processed);

			// turn to a binary image and apply edge detection
			BinaryImage bin = BinaryImage.FromImage (processed);
			bin = EdgeDetection.Apply (bin);
			bin = new ImageObjectScaler(20).Apply(bin);
			processed = bin.GetImage ();

			// analyse the processed image for blobs
			Rectangle[] rectangles = blobFinder.Process (processed);
s.Stop ();
			if(rectangles.Length > 0) {
				Rectangle biggest = null;
				foreach(Rectangle rect in rectangles) {
					if(biggest == null || biggest.SurfaceArea < rect.SurfaceArea) {
						biggest = rect;
					}
				}

				biggest.StrokeWidth = 5;
				biggest.drawInPlace(image);
				cameraView.rectangle = biggest;
Debug.Log ("Time elapsed: " + s.ElapsedMilliseconds);
			} else {
				cameraView.rectangle = null;
			}

			cameraView.processedImage = image;
		} catch(Exception e) {
			Debug.Log (e.ToString ());
		}

		cameraView.Processing = false;
	}
}
