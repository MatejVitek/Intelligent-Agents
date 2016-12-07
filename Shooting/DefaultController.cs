using UnityEngine;
using System.Collections;

[RequireComponent (typeof (ShotControl))]
public class DefaultController : MonoBehaviour {
	private ShotControl s;

	void Start() {
		s = GetComponent<ShotControl>();
		Cursor.visible = false;
		Cursor.lockState = CursorLockMode.Locked;
	}

	void Update() {
		if (Input.GetButtonDown ("Fire1"))
			s.Shoot();
	}
}
