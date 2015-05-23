using UnityEngine;
using System.Collections;
using System.IO;
using System;
using UnityColorFilters;

public class CameraView : MonoBehaviour {
	GameObject screen;
	WebCamTexture webcamTexture;
	EuclideanFilter euclideanFilter;
	BinaryFilter binaryFilter;

	// Use this for initialization
	void Start () {
		// initialise the front camera and map it to the plane
		webcamTexture = new WebCamTexture ();
	//	GetComponent<Renderer> ().material.mainTexture = webcamTexture;
		webcamTexture.Play ();

//		screen = GameObject.Find ("screen");
		euclideanFilter = new EuclideanFilter (new Color32 (0, 145, 245, 1), 120);
		binaryFilter = new BinaryFilter (60);
	}

	// Update is called once per frame
	void Update () {
		Image image = Image.FromWebCamTexture (webcamTexture);
		Image euclidean = euclideanFilter.Process (image);
		Image grayscale = GrayscaleFilter.Process (euclidean);
		Image binary = binaryFilter.Process (grayscale);

		Texture2D tex2d = binary.GetTexture2D ();
		GetComponent<Renderer> ().material.mainTexture = tex2d;
	//	screen.GetComponent<Renderer> ().material.mainTexture = tex2d;
	}
}
