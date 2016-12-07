using UnityEngine;
using System.Collections;

public class BackToMain : MonoBehaviour {

	void Update() {
		if (Input.GetButtonDown ("Cancel"))
			Application.LoadLevel ("Main");
	}
}