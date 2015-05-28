using UnityEngine;
using UnityEngine.UI;

public class ButtonHandler : MonoBehaviour {
	void Start() {
		Button b = GetComponent<Button> ();
		b.onClick.AddListener (delegate() {
			ExecuteClick ();
		});
	}

	public void ExecuteClick() {
		InputField input = GameObject.Find ("cmd").GetComponent<InputField> ();
		string text = input.text;
		if (text.StartsWith ("load:http://")) {
			Debug.Log ("Load");
			// load object
		} else if (text.StartsWith ("scale:")) {
			Debug.Log ("scale:");
			// scale object
		} else if (text.StartsWith ("rotate:")) {
		} else if (text.StartsWith ("shiftx:")) {
		} else if (text.StartsWith ("shifty:")) {
		} else if (text.StartsWith ("shiftz:")) {
		}
	}
}
