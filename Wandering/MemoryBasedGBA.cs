using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityStandardAssets.Characters.ThirdPerson;

[RequireComponent (typeof (UnityEngine.AI.NavMeshAgent))]
[RequireComponent (typeof (ThirdPersonCharacter))]
[RequireComponent (typeof (ToolControl))]
public class MemoryBasedGBA : MonoBehaviour {

	private const int APPROACHING_NEW_RACK = 0, APPROACHING_WAYPOINT = 1, APPROACHING_TOOL = 2, RETURNING_TO_RACK = 3;
	
	public UnityEngine.AI.NavMeshAgent agent { get; private set; }
	public ThirdPersonCharacter character { get; private set; }
	public Vision vision { get; private set; }
	public LinkedList<GameObject> priorityGoals { get; private set; }
	public Vector3[] waypoints { get; private set; }
	public int currentWP { get; private set; }
	
	private int state;
	private GameControl gameControl;
	private ToolControl toolControl;
	
	private GameObject[] racks;
	private WeaponRack[] rackScripts;
	private Vector3[] rackWPs;
	private int currentRack;
	
	void Awake() {
		state = APPROACHING_NEW_RACK;
		currentRack = 0;
		
		agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
		character = GetComponent<ThirdPersonCharacter>();
		toolControl = GetComponent<ToolControl>();
		
		agent.updateRotation = false;
		agent.updatePosition = true;

		priorityGoals = new LinkedList<GameObject>();
	}
	
	void Start() {
		GetRackInfo();
		gameControl = GameObject.FindGameObjectWithTag (Tags.gameController).GetComponent<GameControl>();	
		vision = GetComponentInChildren<Vision>();
	}
	
	void Update() {
		CheckForVisibleTools();

		switch (state) {
			case APPROACHING_NEW_RACK:
				ApproachRack();
				break;
				
			case APPROACHING_WAYPOINT:
				ApproachWaypoint();
				break;
				
			case APPROACHING_TOOL:
				ApproachTool();
				break;
				
			case RETURNING_TO_RACK:
				ReturnToRack();
				break;
		}
	}

	// Add visible tools to priority goals if they're not there already
	private void CheckForVisibleTools() {
		foreach (GameObject tool in rackScripts[currentRack].GetCurrentTools())
			if (vision.IsVisible (tool) && !priorityGoals.Contains (tool))
				priorityGoals.AddLast (tool);
	}

	// Remove null (picked up) tool goals
	private void RemoveNullPriorityGoals() {	
		LinkedListNode<GameObject> node = priorityGoals.First;
		while (node != null) {
			LinkedListNode<GameObject> next = node.Next;
			if (node.Value == null)
				priorityGoals.Remove (node);
			node = next;
		}
	}
	
	private void ApproachRack() {
		agent.SetDestination (rackWPs[currentRack]);
		character.Move (agent.desiredVelocity, false, false);
		if (AgentPathComplete())
			ChangeState (APPROACHING_WAYPOINT);
	}
	
	private void ApproachWaypoint() {
		if (toolControl.nTools == rackScripts[currentRack].nTools) {
			ChangeState (RETURNING_TO_RACK);
			return;
		}
		if (priorityGoals.Count != 0) {
			ChangeState (APPROACHING_TOOL);
			return;
		}
		if (currentWP >= waypoints.Length) {
			ChangeState (RETURNING_TO_RACK);
			return;
		}
		
		agent.SetDestination (waypoints[currentWP]);
		character.Move (agent.desiredVelocity, false, false);
		if (AgentPathComplete())
			currentWP++;
	}
	
	private void ApproachTool() {
		RemoveNullPriorityGoals();

		if (toolControl.nTools == rackScripts[currentRack].nTools) {
			ChangeState (RETURNING_TO_RACK);
			return;
		}
		if (priorityGoals.Count == 0) {
			ChangeState (APPROACHING_WAYPOINT);
			return;
		}

		GameObject tool = priorityGoals.First.Value;

		agent.SetDestination (tool.transform.position);
		character.Move (agent.desiredVelocity, false, false);

		if (AgentPathComplete())
			priorityGoals.RemoveFirst();
	}
	
	private void ReturnToRack() {
		if (priorityGoals.Count != 0) {
			ChangeState (APPROACHING_TOOL);
			return;
		}
		
		agent.SetDestination (rackWPs[currentRack]);
		character.Move (agent.desiredVelocity, false, false);
		
		if (AgentPathComplete()) {
			currentRack++;
			ChangeState (APPROACHING_NEW_RACK);
		}
	}
	
	private void ChangeState (int newState) {
		
		// Things to always do when exiting a state
		switch (state) {
			case APPROACHING_NEW_RACK:
				currentWP = 0;
				GetWaypointsForCurrentRack();
				break;
		}
		
		// Things to always do when entering a state
		switch (newState) {
			case APPROACHING_NEW_RACK: 
				if (currentRack >= racks.Length) {
					gameControl.EndGame();
					return;
				}
				break;
		}
		
		state = newState;
	}
	
	private void GetRackInfo() {
		racks = GameObject.FindGameObjectsWithTag (Tags.rack).OrderBy (go => go.name).ToArray();
		rackWPs = new Vector3[racks.Length];
		rackScripts = new WeaponRack[racks.Length];
		
		for (int i = 0; i < racks.Length; i++) {
			rackScripts[i] = racks[i].GetComponent<WeaponRack>();
			Vector3 rack = racks[i].transform.position;
			UnityEngine.AI.NavMeshHit hit;
			if (UnityEngine.AI.NavMesh.SamplePosition (rack, out hit, 0.5f, UnityEngine.AI.NavMesh.AllAreas))
				rack = hit.position;
			rackWPs[i] = rack;
		}
	}
	
	private void GetWaypointsForCurrentRack() {
		GameObject[] wps = GameObject.FindGameObjectsWithTag (Tags.waypoint).Where (go => go.name.Contains ("Waypoint " + (currentRack + 1))).OrderBy (go => go.name).ToArray();
		waypoints = new Vector3[wps.Length];
		for (int i = 0; i < wps.Length; i++)
			waypoints[i] = wps[i].transform.position;
	}
	
	private bool AgentPathComplete() {
		return !agent.pathPending && agent.remainingDistance < agent.stoppingDistance;
	}
}