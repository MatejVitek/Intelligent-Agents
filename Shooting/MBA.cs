using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent (typeof (ShotControl))]
[RequireComponent (typeof (Vision))]
public class MBA : MonoBehaviour {
	
	private const int SEARCHING = 0, AIMING = 1, SHOOTING = 2;
	
	public float searchingStateHSpeed, searchingStateMaxVSpeed, aimingStateSpeed;
	public float timeBetweenChanges;
	public float maxAngleX;
	
	private ShotControl shotControl;
	private Vision vision;
	
	private GameObject target;
	private float nextRotationChange;
	private int nextXCheck;
	private float xRot;
	private int state;

	private Dictionary<Position, Position> nextTargetMemory;
	private Position previousTarget, currentTarget;
	private float timeBetweenTargetChanges, nextTargetChange;
	
	void Awake() {
		shotControl = GetComponent<ShotControl>();
		vision = GetComponent<Vision>();
		
		state = SEARCHING;
		
		nextRotationChange = Time.time;
		nextXCheck = 0;
		xRot = 0f;

		nextTargetMemory = new Dictionary<Position, Position>();
		previousTarget = null;
		currentTarget = null;
	}

	void Start() {
		timeBetweenTargetChanges = GameObject.FindGameObjectWithTag (Tags.gameController).GetComponent<TargetControl> ().secondsBetweenSpawns;
		nextTargetChange = Time.time + timeBetweenTargetChanges;
	}
	
	void Update() {
		target = GameObject.FindGameObjectWithTag (Tags.target);

		if (Time.time > nextTargetChange) {
			previousTarget = null;
			nextTargetChange += timeBetweenTargetChanges;
		}

		switch (state) {
			case SEARCHING:
				Search();
				break;
			case AIMING:
				Aim();
				break;
			case SHOOTING:
				Shoot();
				break;
			default:
				Debug.Log ("Invalid state");
				break;
		}
	}
	
	
	
	private void Search() {
		if (Debug.isDebugBuild)
			Debug.Log ("Searching");

		Rotate();

		if (target != null && vision.IsVisible (target)) {
			currentTarget = new Position (target.transform.position);
			if (previousTarget != null)
				if (!nextTargetMemory.ContainsKey (previousTarget))
					nextTargetMemory.Add (previousTarget, currentTarget);
			state = AIMING;
		}

		if (previousTarget != null)
			if (nextTargetMemory.TryGetValue (previousTarget, out currentTarget))
				state = AIMING;
	}
	
	private void Rotate() {
		if (Time.time >= nextRotationChange) {
			xRot = Random.Range (-1.0f, 1.0f);
			nextRotationChange = Time.time + timeBetweenChanges;
		}
		Rotate (transform.parent, transform);
	}
	
	private void Rotate (Transform character, Transform camera) {
		character.localRotation *= Quaternion.Euler (0f, searchingStateHSpeed * Time.deltaTime, 0f);
		camera.localRotation *= Quaternion.Euler (xRot * searchingStateMaxVSpeed * Time.deltaTime, 0f, 0f);
		if (nextXCheck <= 0 && XOutOfBounds (camera.localRotation)) {
			xRot = -xRot;
			nextXCheck = 5;
		}
		else nextXCheck--;
	}
	
	
	private bool XOutOfBounds (Quaternion q) {
		float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan (q.x / q.w);
		return Mathf.Abs (angleX) >= maxAngleX;
	}
	
	
	
	private void Aim() {
		if (Debug.isDebugBuild)
			Debug.Log ("Aiming");
		
		RotateToTarget();
		
		if (Time.time > nextTargetChange) {
			previousTarget = currentTarget;
			nextTargetChange += timeBetweenTargetChanges;
			state = SEARCHING;
		}
		else if (LookingAtTarget())
			state = SHOOTING;
	}
	
	private void RotateToTarget() {
		Quaternion targetRotation = Quaternion.LookRotation (currentTarget.v - transform.position);
		Quaternion targetX = Quaternion.Euler (targetRotation.eulerAngles.x, 0, 0);
		Quaternion targetY = Quaternion.Euler (0, targetRotation.eulerAngles.y, 0);
		
		float s = aimingStateSpeed * Time.deltaTime;
		transform.localRotation = Quaternion.Slerp (transform.localRotation, targetX, s);
		transform.parent.localRotation = Quaternion.Slerp (transform.parent.localRotation, targetY, s);
	}
	
	private bool LookingAtTarget() {
		RaycastHit hit;
		if (Physics.Raycast (transform.position, transform.TransformDirection (Vector3.forward), out hit, 50))
			return hit.transform.gameObject.Equals (target);
		return false;
	}
	
	
	
	private void Shoot() {
		if (Debug.isDebugBuild)
			Debug.Log ("Shooting");
		
		if (shotControl.Shoot()) {
			previousTarget = currentTarget;
			nextTargetChange = Time.time + timeBetweenTargetChanges;
			state = SEARCHING;
		}
		else
			state = AIMING;
	}




	
	private class Position {
		public Vector3 v;

		public Position (Vector3 v) {
			this.v = v;
		}

		public override bool Equals (object o) {
			if (!(o is Position))
				return false;

			Position other = (Position) o;
			return this.GetHashCode() == other.GetHashCode();
		}

		public override int GetHashCode() {
			return v.x.GetHashCode() + v.y.GetHashCode() + v.z.GetHashCode();
		}
	}
}