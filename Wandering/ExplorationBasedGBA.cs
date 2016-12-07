using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityStandardAssets.Characters.ThirdPerson;

[RequireComponent (typeof (UnityEngine.AI.NavMeshAgent))]
[RequireComponent (typeof (ThirdPersonCharacter))]
[RequireComponent (typeof (ToolControl))]
public class ExplorationBasedGBA : MonoBehaviour {
	
	private const int APPROACHING_RACK = 0, EXPLORING = 1, ROTATING = 2, APPROACHING_TOOL = 3;

	public UnityEngine.AI.NavMeshAgent agent { get; private set; }
	public ThirdPersonCharacter character { get; private set; }
	public Vision vision { get; private set; }
	public Vector3 exploreWP { get; private set; }
	public LinkedList<GameObject> priorityGoals { get; private set; }
	
	private int state;
	private GameControl gameControl;
	private ToolControl toolControl;
	
	private GameObject[] racks;
	private WeaponRack[] rackScripts;
	private Vector3[] rackWPs;
	private BoxCollider[][] spawnBoxes;
	private int currentRack, currentBox;
	private float timeOfRotateStart;
	
	void Awake() {
		state = APPROACHING_RACK;
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
			case APPROACHING_RACK:
				ApproachRack();
				break;
				
			case EXPLORING:
				Explore();
				break;
				
			case ROTATING:
				Rotate();
				break;
				
			case APPROACHING_TOOL:
				ApproachTool();
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
		if (priorityGoals.Count != 0) {
			ChangeState (APPROACHING_TOOL);
			return;
		}

		agent.SetDestination (rackWPs[currentRack]);
		character.Move (agent.desiredVelocity, false, false);

		if (AgentPathComplete()) {
			if (rackScripts[currentRack].GetCurrentTools().Count > 0)
				ChangeState (EXPLORING);
			else {
				currentRack++;
				if (currentRack >= racks.Length)
					gameControl.EndGame();
			}
		}
	}
	
	private void Explore() {
		if (priorityGoals.Count != 0) {
			ChangeState (APPROACHING_TOOL);
			return;
		}
		if (toolControl.nTools == rackScripts[currentRack].toolsLeft || currentBox >= spawnBoxes[currentRack].Length) {
			ChangeState (APPROACHING_RACK);
			return;
		}
		
		agent.SetDestination (exploreWP);
		character.Move (agent.desiredVelocity, false, false);

		if (AgentPathComplete())
			ChangeState (ROTATING);
	}

	private void Rotate() {
		if (priorityGoals.Count != 0) {
			ChangeState (APPROACHING_TOOL);
			return;
		}
		if (Time.time - timeOfRotateStart >= 1f) {
			currentBox++;
			ChangeState (currentBox >= spawnBoxes[currentRack].Length ? APPROACHING_RACK : EXPLORING);
			return;
		}

		transform.Rotate (0f, Time.deltaTime * 360f, 0f);
	}
	
	private void ApproachTool() {
		RemoveNullPriorityGoals();
		
		if (toolControl.nTools == rackScripts[currentRack].toolsLeft) {
			ChangeState (APPROACHING_RACK);
			return;
		}
		if (priorityGoals.Count == 0) {
			ChangeState (EXPLORING);
			return;
		}
		
		GameObject tool = priorityGoals.First.Value;
		
		agent.SetDestination (tool.transform.position);
		character.Move (agent.desiredVelocity, false, false);
		
		if (AgentPathComplete())
			priorityGoals.RemoveFirst();
	}
	
	private void ChangeState (int newState) {

		// Things to always do when exiting a state
		switch (state) {
			case APPROACHING_RACK:
				currentBox = 0;
				break;
		}

		// Things to always do when entering a state
		switch (newState) {
			case EXPLORING:
				exploreWP = WeaponRack.GetRandomPointInBox (spawnBoxes[currentRack][currentBox]);
				break;

			case ROTATING:
				timeOfRotateStart = Time.time;
				break;
		}

		state = newState;
	}
	
	private void GetRackInfo() {
		racks = GameObject.FindGameObjectsWithTag (Tags.rack).OrderBy (go => go.name).ToArray();
		rackWPs = new Vector3[racks.Length];
		rackScripts = new WeaponRack[racks.Length];
		spawnBoxes = new BoxCollider[racks.Length][];

		for (int i = 0; i < racks.Length; i++) {
			rackScripts[i] = racks[i].GetComponent<WeaponRack>();
			Vector3 rack = racks[i].transform.position;
			UnityEngine.AI.NavMeshHit hit;
			if (UnityEngine.AI.NavMesh.SamplePosition (rack, out hit, 0.5f, UnityEngine.AI.NavMesh.AllAreas))
				rack = hit.position;
			rackWPs[i] = rack;
			spawnBoxes[i] = rackScripts[i].spawnZone.ToArray();
			TargetControl.Shuffle<BoxCollider> (spawnBoxes[i]);
		}
	}
	
	private bool AgentPathComplete() {
		return !agent.pathPending && agent.remainingDistance < agent.stoppingDistance;
	}
}