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
	private WebCamTexture webcamTexture;
	private Renderer noMarker = null;
	private Renderer objRenderer = null;
	private Color markerColor = new Color();
	private Color objColor = new Color();
	private GameObject screen = null;
	private GameObject obj = null;

	private bool displayProcessed = false;

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
		objRenderer = obj.GetComponent<Renderer> ();
		objColor = objRenderer.material.color;
	}

	// Update is called once per frame
	void Update () {
		// restart the image processing thread
		if (!Processing) {
			StartProcessing();
		}

		// if object was found
		if (MarkerRect != null) {
			// update the objects position
			Vector2 markerCoordinate = getRelativeCoordinateFromTextureCoordinate (MarkerRect.MidPointX, MarkerRect.MidPointY); // (320, 240);
			Vector3 position = new Vector3 (markerCoordinate.x, markerCoordinate.y, obj.transform.localPosition.z);
			obj.transform.localPosition = position;

			// hide text and display the object
			noMarker.material.color = new Color32(1, 1, 1, 0);
			obj.GetComponent<Renderer> ().material.color = objColor;
		} else {
			// show text and hide the object
			noMarker.material.color = markerColor;
			objRenderer.material.color = new Color32(1, 1, 1, 0);
		}

		// check whether or not we are supposed to display the processed image
		if (ProcessedImage != null && displayProcessed) {
			objRenderer.material.mainTexture = ProcessedImage.GetTexture2D ();
			ProcessedImage = null;
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

	Vector2 getRelativeCoordinateFromTextureCoordinate(int x, int y) {
		Vector2 relativeCoordinate = new Vector2 ();

		// convert x & y from top-left based to center-based coordinates
		float xCenterBased = x - (webcamTexture.width / 2.0f);
		float yCenterBased = y - (webcamTexture.height / 2.0f);
		float xScaled = xCenterBased / (1.5f * webcamTexture.width) * (-1);
		float yScaled = yCenterBased / (1.5f * webcamTexture.height) * (-1);

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
				cameraView.MarkerRect = biggest;
Debug.Log ("Time elapsed: " + s.ElapsedMilliseconds);
			} else {
				cameraView.MarkerRect = null;
			}

			cameraView.ProcessedImage = processed;
		} catch(Exception e) {
			Debug.Log (e.ToString ());
		}

		cameraView.Processing = false;
	}
}
