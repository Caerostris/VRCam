using UnityEngine;

public class SettingsButtonHandler : MonoBehaviour {
	private GameObject displayOptionsPanel = null;
	private GameObject cardboardOptionsPanel = null;
	private bool activeState = false;

	public void Start () {
		displayOptionsPanel = GameObject.Find ("DisplayOptionsPanel");
		cardboardOptionsPanel = GameObject.Find ("CardboardOptionsPanel");

		displayOptionsPanel.SetActive (activeState);
		cardboardOptionsPanel.SetActive (activeState);
	}

	public void ButtonClicked () {
		activeState = !activeState;
		displayOptionsPanel.SetActive (activeState);
		cardboardOptionsPanel.SetActive (activeState);
	}
}
