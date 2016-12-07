using UnityEngine;
using System.Collections;

[RequireComponent (typeof (ShotControl))]
[RequireComponent (typeof (Vision))]
public class SRA : MonoBehaviour {
	public float camSpeedTargetVisible, camSpeedDefault, timeBetweenChanges, maxAngleX;

	private ShotControl shotControl;
	private Vision vision;

	private float camSpeed;
	private float nextRotationChange;
	private int nextXCheck;
	private float angle;

	void Awake() {
		shotControl = GetComponent<ShotControl>();
		vision = GetComponent<Vision>();

		camSpeed = camSpeedDefault;
		nextRotationChange = Time.time;
		nextXCheck = 0;
		angle = 0f;
	}

	void Update() {
		RandomRotation();

		GameObject target = GameObject.FindGameObjectWithTag (Tags.target);
		if (target != null && vision.IsVisible (target)) {
			if (Debug.isDebugBuild)
				Debug.Log ("Target visible");
			camSpeed = camSpeedTargetVisible;
			shotControl.Shoot();
		}
		else {
			if (Debug.isDebugBuild)
				Debug.Log ("Target out of sight");
			camSpeed = camSpeedDefault;
		}
	}

	private void RandomRotation() {
		if (Time.time >= nextRotationChange) {
			angle = Random.Range (-Mathf.PI, Mathf.PI);
			nextRotationChange = Time.time + timeBetweenChanges;
		}
		Rotate (transform.parent, transform);
	}
	
	private void Rotate (Transform character, Transform camera) {
		character.localRotation *= Quaternion.Euler (0f, camSpeed * Mathf.Cos (angle) * Time.deltaTime, 0f);
		camera.localRotation *= Quaternion.Euler (camSpeed * Mathf.Sin (angle) * Time.deltaTime, 0f, 0f);
		if (nextXCheck <= 0 && XOutOfBounds (camera.localRotation)) {
			angle = -angle;
			nextXCheck = 5;
		}
		else nextXCheck--;
	}
	
	
	private bool XOutOfBounds (Quaternion q) {
		float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan (q.x / q.w);
		return Mathf.Abs (angleX) >= maxAngleX;
	}
}
