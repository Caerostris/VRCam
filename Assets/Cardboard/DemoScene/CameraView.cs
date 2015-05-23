using UnityEngine;
using System.Collections;
using AForge.Imaging.Filters;
using AForge.Imaging;
using System.IO;
using System;
using System.Drawing;

public class CameraView : MonoBehaviour {
	GameObject screen;
	WebCamTexture webcamTexture;

	// Use this for initialization
	void Start () {
		// initialise the front camera and map it to the plane
		webcamTexture = new WebCamTexture ();
		GetComponent<Renderer> ().material.mainTexture = webcamTexture;
		webcamTexture.Play ();

		screen = GameObject.Find ("screen");
	}
	
	// Update is called once per frame
	void Update () {
		Texture2D t2d = getTexture2DFromTexture (GetComponent<Renderer> ().material.mainTexture);
		Bitmap bmp = textureToBitmap(t2d);
		UnmanagedImage image = bitmapToUnmanagedImage (bmp);

		EuclideanColorFiltering filter = new EuclideanColorFiltering ();
		filter.CenterColor = new RGB (0, 0, 0);
		filter.Radius = 100;
		filter.ApplyInPlace (image);

		Grayscale grayScaleFilter = new Grayscale (0.2125, 0.7154, 0.0721);
		UnmanagedImage grayImage = grayScaleFilter.Apply (image);

		BlobCounter blobCounter = new BlobCounter ();
		blobCounter.MinWidth = 5;
		blobCounter.MinHeight = 5;
		blobCounter.FilterBlobs = true;
		blobCounter.ProcessImage (grayImage);

		Rectangle[] rects = blobCounter.GetObjectsRectangles();;

		foreach (Rectangle rect in rects) {
			Drawing.Rectangle(image, rect, Color.green);
		}

		Texture2D rectsT2d = getTexture2DFromBitmap(bmp);

		screen.GetComponent<Renderer> ().material.mainTexture = rectsT2d;
	}

	private Texture2D getTexture2DFromBitmap(Bitmap bmp) {
		Texture2D t2d = new Texture2D (bmp.Width, bmp.Height);
		MemoryStream stream = new MemoryStream ();
		bmp.Save (stream, bmp.RawFormat);

		t2d.LoadImage (stream.ToArray ());
		return t2d;
	}

	private Texture2D getTexture2DFromTexture(Texture texture) {
		Texture2D t2d = new Texture2D (texture.width, texture.height);

		IntPtr pointer = texture.GetNativeTexturePtr ();
		t2d.UpdateExternalTexture (pointer);

		return t2d;
	}

	private UnmanagedImage bitmapToUnmanagedImage(Bitmap bmp) {
		return UnmanagedImage.FromManagedImage (bmp);
	}

	private Bitmap textureToBitmap (Texture2D t2d) {
		Bitmap bmp;

		using (var ms = new MemoryStream(t2d.EncodeToPNG())) {
			bmp = new Bitmap (ms);
		}

		return bmp;
	}
}
