using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent (typeof (Text))]
public class EndGame : MonoBehaviour {
	
	private Text text;
	
	void Start() {
		Cursor.visible = true;
		Cursor.lockState = CursorLockMode.None;
		text = GetComponent<Text>();
		text.text = "GAME OVER\nScore: " + PlayerPrefs.GetInt ("Score") + "\nTime: " + PlayerPrefs.GetString ("Time");
	}
	
	void Update() {
		if (Input.anyKey) Application.LoadLevel ("Main");
	}
}
