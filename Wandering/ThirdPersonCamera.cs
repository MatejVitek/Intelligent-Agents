using UnityEngine;
using System.Collections;

[RequireComponent (typeof (Camera))]
public class ThirdPersonCamera : MonoBehaviour {
	private Transform charTransform;
	private float zoom;
	private Camera cam;

	void Awake() {
		zoom = 5f;
		cam = GetComponent<Camera>();
	}

	void Start() {
		charTransform = GameObject.FindGameObjectWithTag (Tags.player).transform;
	}

	void Update() {
		// Only update if this is the currently used camera
		if (!cam.isActiveAndEnabled)
			return;
		
		// Input (mwheel to zoom in/out)
		if (Input.GetAxis ("Mouse ScrollWheel") > 0)
			zoom *= 0.9f;
		else if (Input.GetAxis ("Mouse ScrollWheel") < 0)
			zoom *= 1.1f;

		// Smoothly rotate camera toward player's rotation
		Quaternion target = Quaternion.Euler (
			transform.eulerAngles.x,
			charTransform.eulerAngles.y,
			transform.eulerAngles.z
		);
		transform.rotation = Quaternion.Slerp (transform.rotation, target, 2.0f * Time.deltaTime);

		// Spherical coordinates of the camera relative to the player, translated into x,y,z.
		// Zoom = sphere radius (distance to the player)
		float phi = (90f - transform.eulerAngles.x) * Mathf.Deg2Rad;
		float theta = (-90f - transform.eulerAngles.y) * Mathf.Deg2Rad;
		float xDelta = Mathf.Sin (phi) * Mathf.Cos (theta) * zoom;
		float yDelta = Mathf.Cos (phi) * zoom;
		float zDelta = Mathf.Sin (phi) * Mathf.Sin (theta) * zoom;
		
		// Apply the transform calculated above
		transform.position = new Vector3 (
			charTransform.position.x + xDelta,
			charTransform.position.y + yDelta + 1.5f,	// add 1.5 for player height (so the camera is centered at shoulder height)
			charTransform.position.z + zDelta
		);
	}
}
