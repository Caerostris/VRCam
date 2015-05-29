/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) 2015 Keno Schwalb
 */

using UnityEngine;

public class DisplayOptionsHandler : MonoBehaviour {
	public void TrackingButtonClicked () {
		CameraView.setDisplayMode (CameraView.DisplayMode.Tracking);
	}

	public void EdgeButtonClicked () {
		CameraView.setDisplayMode (CameraView.DisplayMode.Edge);
	}

	public void BinaryButtonClicked () {
		CameraView.setDisplayMode (CameraView.DisplayMode.Binary);
	}

	public void EuclideanButtonClicked () {
		CameraView.setDisplayMode (CameraView.DisplayMode.Euclidean);
	}

	public void NormalButtonClicked () {
		CameraView.setDisplayMode (CameraView.DisplayMode.Normal);
	}
}
