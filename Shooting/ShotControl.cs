using UnityEngine;
using System.Collections;

[RequireComponent (typeof (Camera))]
public class ShotControl : MonoBehaviour {
	public AudioClip gunSound, hitSound;
	public GameObject impactDecal;
	public float recoil;

	private float nextFire;
	private float weaponSpread;
	private Camera mainCamera;

	void Awake() {
		nextFire = Time.time;
		weaponSpread = 0.02f;
	}

	void Start() {
		mainCamera = GetComponent<Camera>();
	}

	public bool Shoot() {
		if (Time.time < nextFire) return false;
		nextFire = Time.time + 0.5f;
		AudioSource.PlayClipAtPoint (gunSound, mainCamera.transform.position);

		RaycastHit hit;
		Vector3 dir = mainCamera.transform.TransformDirection (new Vector3 (Random.Range (-weaponSpread, weaponSpread), Random.Range (-weaponSpread, weaponSpread), 1));
		mainCamera.transform.localRotation *= Quaternion.Euler (recoil, 0, 0);
		if (Physics.Raycast (mainCamera.transform.position, dir, out hit, 50)) {
			GameObject obj = hit.transform.gameObject;
			if (obj.CompareTag (Tags.target)) {
				AudioSource.PlayClipAtPoint (hitSound, mainCamera.transform.position);
				obj.SendMessage ("HitTarget");
				return true;
			}
			else if (obj.CompareTag (Tags.wall)) {
				GameObject hitDecal = (GameObject) Instantiate (impactDecal, hit.point, Quaternion.LookRotation (hit.normal));
				Destroy (hitDecal, 5);
			}
		}

		return false;
	}
}
