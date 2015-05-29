using UnityEngine;

public class DisplayOptionsHandler : MonoBehaviour {
	public void TrackingButtonClicked () {
		CameraView.displayMode = CameraView.DisplayMode.Tracking;
	}

	public void EdgeButtonClicked () {
		CameraView.displayMode = CameraView.DisplayMode.Edge;
	}

	public void BinaryButtonClicked () {
		CameraView.displayMode = CameraView.DisplayMode.Binary;
	}

	public void EuclideanButtonClicked () {
		CameraView.displayMode = CameraView.DisplayMode.Euclidean;
	}

	public void NormalButtonClicked () {
		CameraView.displayMode = CameraView.DisplayMode.Normal;
	}
}
