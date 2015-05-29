/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) 2015 Keno Schwalb
 */

using UnityEngine;

public class SettingsButtonHandler : MonoBehaviour {
	private GameObject displayOptionsPanel = null;
	private GameObject cardboardOptionsPanel = null;
	private GameObject imageAnalysisOptionsPanel = null;
	private bool activeState = false;

	public void Start () {
		displayOptionsPanel = GameObject.Find ("DisplayOptionsPanel");
		cardboardOptionsPanel = GameObject.Find ("CardboardOptionsPanel");
		imageAnalysisOptionsPanel = GameObject.Find ("ImageAnalysisOptionsPanel");

		displayOptionsPanel.SetActive (activeState);
		cardboardOptionsPanel.SetActive (activeState);
		imageAnalysisOptionsPanel.SetActive (activeState);
	}

	public void ButtonClicked () {
		CameraView.startCalibration = true;
		activeState = !activeState;

		displayOptionsPanel.SetActive (activeState);
		cardboardOptionsPanel.SetActive (activeState);
		imageAnalysisOptionsPanel.SetActive (activeState);
	}
}
