using UnityEngine;
using System.Collections;

public class TargetControl : MonoBehaviour {
	public GameObject target;
	public float secondsBetweenSpawns;

	private Transform[] targetTransforms;
	private int targetIndex;
	private GameObject currentTarget;
	private float lastSpawnTime;

	void Start() {
		GameObject[] locations = GameObject.FindGameObjectsWithTag (Tags.targetLocation);
		targetTransforms = new Transform[locations.Length];
		for (int i = 0; i < locations.Length; i++)
			targetTransforms [i] = locations [i].transform;
		Shuffle (targetTransforms);

		targetIndex = 0;
		Transform t = targetTransforms[targetIndex];
		currentTarget = (GameObject) Instantiate (target, t.position, t.rotation);

		lastSpawnTime = Time.time;
	}
	
	void Update() {
		if (Time.time - lastSpawnTime >= secondsBetweenSpawns) NextTarget();
	}

	public void NextTarget() {
		Destroy (currentTarget);
		targetIndex++;
		if (targetIndex >= targetTransforms.Length) targetIndex = 0;

		Transform t = targetTransforms[targetIndex];
		currentTarget = (GameObject) Instantiate (target, t.position, t.rotation);

		lastSpawnTime = Time.time;
	}

	public static void Shuffle<T> (T[] array) {
		for (int i = array.Length - 1; i > 0; i--) {
			int r = Random.Range (0, i + 1);
			T tmp = array[r];
			array[r] = array[i];
			array[i] = tmp;
		}
	}
}