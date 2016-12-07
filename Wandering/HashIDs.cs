using UnityEngine;
using System.Collections;

public class HashIDs : MonoBehaviour {
	public int idleState { get; private set; }
	public int runState { get; private set; }
	public int speedFloat { get; private set; }

	void Awake() {
		idleState = Animator.StringToHash ("Base Layer.Idle");
		runState = Animator.StringToHash ("Base Layer.Run");
		speedFloat = Animator.StringToHash ("Speed");
	}
}
