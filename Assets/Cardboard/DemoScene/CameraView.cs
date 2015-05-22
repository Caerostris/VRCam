using UnityEngine;
using System.Collections;

public class CameraView : MonoBehaviour {
	WebCamTexture webcamTexture;

	// Use this for initialization
	void Start () {
		// initialise the front camera and map it to the plane
		webcamTexture = new WebCamTexture ();
		GetComponent<Renderer> ().material.mainTexture = webcamTexture;
		webcamTexture.Play ();

		Color32[] pixels = webcamTexture.GetPixels32 ();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
