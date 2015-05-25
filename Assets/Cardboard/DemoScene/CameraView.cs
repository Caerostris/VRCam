using UnityEngine;
using System.Collections;
using System.IO;
using System;
using UnityColorFilters;
using System.Threading;

public class CameraView : MonoBehaviour {
	private WebCamTexture webcamTexture;
	private bool processing = false;
	public Image processedImage = null;

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
	}

	// Update is called once per frame
	void Update () {
		if (!processing) {
			StartProcessing();

//			Texture2D tex2d = processedImage.GetTexture2D ();
//			GetComponent<Renderer> ().material.mainTexture = tex2d;
		} else {
	//		Debug.Log ("Processing");
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
			Debug.Log ("Could not start thread");
		}
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

		euclideanFilter = new EuclideanFilter (new Color32 (235, 125, 35, 1), 60);
		binaryFilter = new BinaryFilter (20);
		blobFinder = new BlobFinder ();
		blobFinder.MinWidth = 50;
		blobFinder.MinHeight = 50;
	}

	public void ThreadRun() {
		Image processed = euclideanFilter.ApplyInPlace (image);
		GrayscaleFilter.ApplyInPlace (processed);
		binaryFilter.ApplyInPlace (processed);

		Debug.Log ("Starting blob analysis");
		Rectangle[] rectangles = blobFinder.Process (processed);
		Debug.Log ("Blob analysis finished" + ((rectangles.Length > 0) ? "" + rectangles.Length : ""));

		cameraView.processedImage = processed;
		cameraView.Processing = false;
	}
}
