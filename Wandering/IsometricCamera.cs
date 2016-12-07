using UnityEngine;
using System.Collections;

[RequireComponent (typeof (Camera))]
public class IsometricCamera : MonoBehaviour {
	public float mouseSpeed;

	private Transform charTransform;
	private Camera cam;

	void Awake() {
		cam = GetComponent<Camera>();
	}
	
	void Start() {
		charTransform = GameObject.FindGameObjectWithTag (Tags.player).transform;
	}
	
	void Update() {
		// Only update if this is the currently used camera
		if (!cam.isActiveAndEnabled)
			return;

		// Input (mwheel to zoom in/out, mouse move to rotate)
		if (Input.GetAxis ("Mouse ScrollWheel") > 0)
			cam.orthographicSize *= 0.9f;
		else if (Input.GetAxis ("Mouse ScrollWheel") < 0)
			cam.orthographicSize *= 1.1f;
		transform.rotation = Quaternion.Euler (0f, mouseSpeed * Input.GetAxis ("Mouse X"), 0f) * transform.rotation;

		// Spherical coordinates of the camera relative to the player, translated into x,y,z
		// Since the camera is orthographic, we can simply assume a very large radius.
		float phi = (90f - transform.eulerAngles.x) * Mathf.Deg2Rad;
		float theta = (-90f - transform.eulerAngles.y) * Mathf.Deg2Rad;
		float xDelta = Mathf.Sin (phi) * Mathf.Cos (theta) * 100f;
		float yDelta = Mathf.Cos (phi) * 100f;
		float zDelta = Mathf.Sin (phi) * Mathf.Sin (theta) * 100f;

		// Apply the transform calculated above
		transform.position = new Vector3 (
			charTransform.position.x + xDelta,
			charTransform.position.y + yDelta,
			charTransform.position.z + zDelta
		);
	}
}
