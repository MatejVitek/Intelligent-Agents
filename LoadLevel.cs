using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LoadLevel : MonoBehaviour {
	public GameObject loadingImage;

	private string levelName;
	private GameObject[] primary, alternative;

	void Awake() {
		Cursor.visible = true;
		Cursor.lockState = CursorLockMode.None;
	}

	void Start() {
		primary = GameObject.FindGameObjectsWithTag (Tags.primary);
		alternative = GameObject.FindGameObjectsWithTag (Tags.alternative);
		foreach (GameObject button in alternative)
			button.SetActive (false);
	}

	public void LoadScene (string level) {
		loadingImage.SetActive (true);
		foreach (Transform child in transform.Find ("Content Panel"))
			child.gameObject.SetActive (false);
		Application.LoadLevel (level);
	}

	public void LoadScene (bool alternative) {
		LoadScene (levelName + (alternative ? " Moved" : ""));
	}

	public void SetName (string name) {
		levelName = name;
		transform.Find ("Content Panel").Find ("Level Name").GetComponent<Text>().text = name;
		foreach (GameObject button in primary)
			button.SetActive (false);
		foreach (GameObject button in alternative)
			button.SetActive (true);
	}

	public void Back() {
		foreach (GameObject button in primary)
			button.SetActive (true);
		foreach (GameObject button in alternative)
			button.SetActive (false);
	}
}
