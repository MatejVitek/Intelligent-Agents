using UnityEngine;
using System.Collections;

public class SpawnControl : MonoBehaviour {

	void Start() {
		GameObject player = GameObject.FindGameObjectWithTag (Tags.player);
		UnityEngine.AI.NavMeshAgent agent = player.GetComponent<UnityEngine.AI.NavMeshAgent>();
		if (agent != null)
			agent.enabled = false;
		GameObject[] spawns = GameObject.FindGameObjectsWithTag (Tags.spawn);
		int rnd = Random.Range (0, spawns.Length);
		player.transform.position = spawns[rnd].transform.position;
		player.transform.rotation = spawns[rnd].transform.rotation;
		if (agent != null)
			agent.enabled = true;
	}
}
