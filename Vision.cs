using UnityEngine;
using System.Collections;

[RequireComponent (typeof (Camera))]
public class Vision : MonoBehaviour {

	public float range;
	public Camera visionCamera { get; private set; }

	private float vAngle, hAngle;

	void Awake() {
		visionCamera = GetComponent<Camera> ();
		vAngle = visionCamera.fieldOfView / 2;
		hAngle = Mathf.Atan (Mathf.Tan (vAngle * Mathf.Deg2Rad) * visionCamera.aspect) * Mathf.Rad2Deg;
	}

	// Debug purposes only, no real functionality
	void Update() {
		Quaternion rotation = transform.rotation;
		Debug.DrawRay (transform.position, rotation * Vector3.forward * 50, Color.red);
		Quaternion[] directions = {
			Quaternion.Euler (-vAngle, -hAngle, 0),
			Quaternion.Euler (vAngle, -hAngle, 0),
			Quaternion.Euler (-vAngle, hAngle, 0),
			Quaternion.Euler (vAngle, hAngle, 0)
		};
		foreach (Quaternion dir in directions)
			Debug.DrawRay (transform.position, rotation * dir * Vector3.forward * 50);
	}

	public bool IsVisible (GameObject obj) {
		if (Vector3.Distance (transform.position, obj.transform.position) > range)
			return false;

		if (IsInAngleRange (obj.transform.position) && RaycastSuccessful (obj.transform.position, obj))
			return true;

		Collider col = obj.GetComponent<BoxCollider>();
		if (col != null)
			foreach (Vector3 vertex in GetVertices ((BoxCollider) col))
				if (IsInAngleRange (vertex) && RaycastSuccessful (vertex, obj))
					return true;
		return false;
	}

	public static Vector3[] GetVertices (BoxCollider col) {
		Vector3[] vertices = new Vector3[8];
		for (int i = 0; i < vertices.Length; i++)
			vertices[i] = col.gameObject.transform.TransformPoint (col.center + new Vector3 (
				((i & 4) != 0 ? 1 : -1) * col.size.x / 2,
				((i & 2) != 0 ? 1 : -1) * col.size.y / 2,
				((i & 1) != 0 ? 1 : -1) * col.size.z / 2
			));
		return vertices;
	}
	
	private bool IsInAngleRange (Vector3 pos) {
		Vector3 forward = transform.rotation * Vector3.forward;
		Vector3 dir = pos - transform.position;
		float hDelta = hDeltaAngle (forward, dir);
		float vDelta = vDeltaAngle (forward, dir);

		return hDelta <= hAngle && vDelta <= vAngle;
	}

	private float hDeltaAngle (Vector3 u1, Vector3 u2) {
		// Project the second vector to the horizontal (relative to the looking direction) plane.
		u2 = Vector3.ProjectOnPlane (u2, transform.rotation * Vector3.up).normalized;

		if (u2.magnitude == 0f)
			return 0f;
		return Vector3.Angle (u1, u2);
	}

	private float vDeltaAngle (Vector3 u1, Vector3 u2) {
		// Project the second vector to the vertical (relative to the looking direction) plane.
		u2 = Vector3.ProjectOnPlane (u2, transform.rotation * Vector3.right).normalized;

		if (u2.magnitude == 0f)
			return 0f;
		return Vector3.Angle (u1, u2);
	}

	private bool RaycastSuccessful (Vector3 targetPosition, GameObject targetObject) {
		RaycastHit hit;
		Vector3 dir = targetPosition - transform.position;

		if (Physics.Raycast (transform.position, dir, out hit))
			return hit.transform.gameObject.Equals (targetObject);

		return false;
	}
}