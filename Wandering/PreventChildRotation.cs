using UnityEngine;
using System.Collections;

public class PreventChildRotation : MonoBehaviour {

	private Quaternion rotation;

	void Awake() {
		rotation = transform.rotation;
	}
	
	void LateUpdate() {
		transform.position = transform.parent.position;
		transform.rotation = rotation;
	}
}
