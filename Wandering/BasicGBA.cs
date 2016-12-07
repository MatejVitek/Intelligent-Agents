using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityStandardAssets.Characters.ThirdPerson;

[RequireComponent (typeof (UnityEngine.AI.NavMeshAgent))]
[RequireComponent (typeof (ThirdPersonCharacter))]
public class BasicGBA : MonoBehaviour {
	public UnityEngine.AI.NavMeshAgent agent { get; private set; }
	public ThirdPersonCharacter character { get; private set; }
	public Vector3[] waypoints { get; private set; }
	public int currentWP { get; private set; }

	private GameControl gameControl;

	void Awake() {
		currentWP = 0;

		agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
		character = GetComponent<ThirdPersonCharacter>();
		
		agent.updateRotation = false;
		agent.updatePosition = true;
	}

	void Start() {
		GameObject[] wps = GameObject.FindGameObjectsWithTag (Tags.waypoint).OrderBy (go => go.name).ToArray();
		List<Vector3> wplist = new List<Vector3> (wps.Length + 15);
		GameObject[] racks = GameObject.FindGameObjectsWithTag (Tags.rack).OrderBy (go => go.name).ToArray();
		int i = 0;
		foreach (GameObject r in racks) {
			int n = int.Parse (r.name.Last().ToString());
			Vector3 rack = r.transform.position;
			UnityEngine.AI.NavMeshHit hit;
			if (UnityEngine.AI.NavMesh.SamplePosition (rack, out hit, 0.5f, UnityEngine.AI.NavMesh.AllAreas))
				rack = hit.position;
			wplist.Add (rack);
			while (i < wps.Length && wps[i].name.Contains (n + "-")) {
				wplist.Add (wps[i].transform.position);
				i++;
			}
			wplist.Add (rack);
		}
		waypoints = wplist.ToArray();

		gameControl = GameObject.FindGameObjectWithTag (Tags.gameController).GetComponent<GameControl>();
		
		agent.SetDestination (waypoints[currentWP]);
	}
	
	
	void Update() {
		if (!agent.pathPending && agent.remainingDistance < agent.stoppingDistance /*&& (!agent.hasPath || agent.velocity.sqrMagnitude == 0f)*/)
			currentWP++;

		if (currentWP >= waypoints.Length) {
			gameControl.EndGame();
			return;
		}

		agent.SetDestination (waypoints[currentWP]);
		character.Move (agent.desiredVelocity, false, false);		
	}
}
