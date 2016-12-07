using UnityEngine;
using System.Collections;

[RequireComponent (typeof (CharacterController))]
public class OldController : MonoBehaviour {
	public float startupTime;

	private Animator anim;
	private HashIDs hash;
	private CharacterController cc;
	private Vector3 lastPosition;
	private float startTime;
	
	void Awake() {
		cc = GetComponent<CharacterController>();
		lastPosition = transform.position;
		startTime = Time.time;
	}

	void Start() {
		anim = GetComponentInChildren<Animator>();
		hash = GameObject.FindGameObjectWithTag (Tags.gameController).GetComponent<HashIDs>();
	}
	
	void Update() {
		anim.SetFloat (hash.speedFloat, 5.5f);
		Vector3 velocity = transform.rotation * (3.5f * Vector3.forward);
		cc.SimpleMove (velocity);
		if (Time.time - startTime > startupTime && Mathf.Abs (transform.position.x - lastPosition.x) < Time.deltaTime && Mathf.Abs (transform.position.z - lastPosition.z) < Time.deltaTime) {
			int angle = Random.Range (90, 270);
			transform.Rotate (0, angle, 0);
		}
		lastPosition = transform.position;
	}
}
