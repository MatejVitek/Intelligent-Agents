using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent (typeof (Text))]
public class EndGameControl : MonoBehaviour {

	private Text text;

	void Start() {
		Cursor.visible = true;
		Cursor.lockState = CursorLockMode.None;
		text = GetComponent<Text>();
		text.text = "GAME OVER\nScore: " + PlayerPrefs.GetInt ("Score");
	}

	void Update() {
		if (Input.anyKey) Application.LoadLevel ("Main");
	}
}
