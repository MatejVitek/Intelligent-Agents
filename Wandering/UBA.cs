using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityStandardAssets.Characters.ThirdPerson;

[RequireComponent (typeof (UnityEngine.AI.NavMeshAgent))]
[RequireComponent (typeof (ThirdPersonCharacter))]
[RequireComponent (typeof (ToolControl))]
public class UBA : MonoBehaviour {
	
	private const int APPROACHING_RACK = 0, EXPLORING = 1, ROTATING = 2, PICKING_UP_TOOLS = 3, RETURNING_TO_RACK = 4;
	private const int nStepsIn2Opt = 30, nStepsInWPPlanning = 20;

	public UnityEngine.AI.NavMeshAgent agent { get; private set; }
	public ThirdPersonCharacter character { get; private set; }
	public Vision vision { get; private set; }
	public List<Vector3> waypoints { get; private set; }
	public int currentWP { get; private set; }
	
	private int state;
	private GameControl gameControl;
	private ToolControl toolControl;

	private WeaponRack[] rackScripts;
	private List<Vector3> rackWPs;
	private BoxCollider[][] spawnBoxes;
	private LinkedList<GameObject> priorityGoals;
	private int currentRack;
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
		if (Debug.isDebugBuild)
			Debug.Log (state);
		
		CheckForVisibleTools();
		RemoveNullPriorityGoals();

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
				
			case PICKING_UP_TOOLS:
				PickUpTools();
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

	private void AddVisibleToolsToExploreWPs() {
		foreach (GameObject tool in priorityGoals)
			if (!waypoints.Contains (tool.transform.position)) {
				float bestDist = float.PositiveInfinity;
				Vector3 best = Vector3.zero;
				foreach (Vector3 wp in waypoints) {
					float dist = GetDistance (tool.transform.position, wp);
					if (dist < bestDist) {
						bestDist = dist;
						best = wp;
					}
				}
				waypoints.Insert (waypoints.IndexOf (best) + 1, tool.transform.position);
			}
	}
	
	private void ApproachRack() {
		agent.SetDestination (rackWPs[currentRack]);
		character.Move (agent.desiredVelocity, false, false);
		
		if (AgentPathComplete())
			ChangeState (EXPLORING);
	}

	private void ReturnToRack() {
		if (priorityGoals.Count != 0) {
			ChangeState (PICKING_UP_TOOLS);
			return;
		}
		
		agent.SetDestination (rackWPs[currentRack]);
		character.Move (agent.desiredVelocity, false, false);

		if (AgentPathComplete()) {
			currentRack++;
			if (currentRack >= rackWPs.Count)
				gameControl.EndGame();
			else ChangeState (APPROACHING_RACK);
		}
	}
	
	private void Explore() {
		if (toolControl.nTools == rackScripts[currentRack].toolsLeft) {
			ChangeState (RETURNING_TO_RACK);
			return;
		}
		if (priorityGoals.Count == rackScripts[currentRack].toolsLeft) {
			ChangeState (PICKING_UP_TOOLS);
			return;
		}
		if (currentWP >= waypoints.Count) {
			ChangeState (APPROACHING_RACK);
			return;
		}

		AddVisibleToolsToExploreWPs();

		agent.SetDestination (waypoints[currentWP]);
		character.Move (agent.desiredVelocity, false, false);
		
		if (AgentPathComplete() && Time.time - timeOfRotateStart >= 1.5f)
			ChangeState (ROTATING);
	}
	
	private void Rotate() {
		AddVisibleToolsToExploreWPs();

		if (Time.time - timeOfRotateStart >= 1f) {
			currentWP++;
			ChangeState (EXPLORING);
			return;
		}
		
		transform.Rotate (0f, Time.deltaTime * 360f, 0f);
	}
	
	private void PickUpTools() {
		AddVisibleToolsToExploreWPs();
		
		if (toolControl.nTools == rackScripts[currentRack].toolsLeft) {
			ChangeState (RETURNING_TO_RACK);
			return;
		}
		if (currentWP >= waypoints.Count || priorityGoals.Count == 0) {
			ChangeState (APPROACHING_RACK);
			return;
		}
		
		agent.SetDestination (waypoints[currentWP]);
		character.Move (agent.desiredVelocity, false, false);
		
		if (AgentPathComplete())
			currentWP++;
	}
	
	private void ChangeState (int newState) {
		
		// Things to always do when exiting a state
		switch (state) {
			case APPROACHING_RACK:
				currentWP = 0;
				GetWaypointsForCurrentRack();
				break;
			}
		
		// Things to always do when entering a state
		switch (newState) {
			case ROTATING:
				timeOfRotateStart = Time.time;
				break;
			
			case PICKING_UP_TOOLS:
				currentWP = 0;
				waypoints = new List<Vector3> (priorityGoals.Count);
				foreach (GameObject pg in priorityGoals)
					waypoints.Add (pg.transform.position);
				waypoints.Insert (0, transform.position);
				waypoints = SolveTSP (waypoints);
				waypoints.RemoveAt (0);
				break;
		}
				
		state = newState;
	}
	
	private void GetRackInfo() {
		GameObject[] racks = GameObject.FindGameObjectsWithTag (Tags.rack);
		rackWPs = new List<Vector3> (racks.Length);
		for (int i = 0; i< racks.Length; i++) {
			Vector3 rack = racks[i].transform.position;
			UnityEngine.AI.NavMeshHit hit;
			if (UnityEngine.AI.NavMesh.SamplePosition (rack, out hit, 0.5f, UnityEngine.AI.NavMesh.AllAreas))
				rack = hit.position;
			rackWPs.Add (rack);
		}
		rackWPs.Insert (0, transform.position);
		rackWPs = SolveTSP (rackWPs);
		rackWPs.RemoveAt (0);

		rackScripts = new WeaponRack[racks.Length];
		spawnBoxes = new BoxCollider[racks.Length][];
		
		for (int i = 0; i < racks.Length; i++) {
			GameObject rack = FindClosestRack (racks, rackWPs[i]);
			rackScripts[i] = rack.GetComponent<WeaponRack>();
			spawnBoxes[i] = rackScripts[i].spawnZone.ToArray();
		}
	}

	private static GameObject FindClosestRack (GameObject[] racks, Vector3 point) {
		float bestDist = float.PositiveInfinity;
		GameObject best = null;
		foreach (GameObject rack in racks) {
			float dist = Vector3.Distance (rack.transform.position, point);
			if (dist < bestDist) {
				bestDist = dist;
				best = rack;
			}
		}
		return best;
	}

	private void GetWaypointsForCurrentRack() {
		List<Vector3> best = new List<Vector3>();
		for (int i = 0; i < nStepsInWPPlanning; i++) {
			List<BoxCollider> boxes = spawnBoxes[currentRack].ToList();
			List<Vector3> current = new List<Vector3> (boxes.Count / 4);
			while (boxes.Count > 0) {
				int rnd = Random.Range (0, boxes.Count);
				Vector3 point = WeaponRack.GetRandomPointInBox (boxes[rnd]);
				current.Add (point);
				boxes.RemoveAt (rnd);

				int k = 0;
				while (k < boxes.Count) {
					if (FullyVisible (boxes[k], point))
						boxes.RemoveAt (k);
					else
						k++;
				}
			}
			if (best.Count == 0 || current.Count < best.Count)
				best = current;
		}
		best.Insert (0, transform.position);
		waypoints = SolveTSP (best);
		waypoints.RemoveAt (0);
	}

	private bool FullyVisible (BoxCollider box, Vector3 point) {
		point += vision.visionCamera.transform.localPosition.y * Vector3.up;
		foreach (Vector3 vertex in Vision.GetVertices (box)) {
			Vector3 dir = vertex - point;
			if (dir.magnitude > vision.range)
				return false;
			RaycastHit hit;
			if (Physics.Raycast (point, dir, out hit, dir.magnitude))
				return false;
		}
		return true;
	}



	private bool AgentPathComplete() {
		return !agent.pathPending && agent.remainingDistance < agent.stoppingDistance;
	}





	// Traveling salesman problem. Ours is technically a slight variation of the original TSP, as we have ...
	// ... a fixed starting point, and don't need to return to it.
	// For small samples, we use the naive approach of checking all permutations
	// For larger samples, we use a nearest neighbor algorithm, along with 2-Opt local optimization to fix intersecting paths
	private static List<Vector3> SolveTSP (List<Vector3> points) {
		if (points.Count <= 7)
			return SolveTSPNaive (points);
		else
			return SolveTSPWithNN2Opt (points);
	}
	


	// Generates permutations of the goal points and returns the one with the shortest total path distance
	private static List<Vector3> SolveTSPNaive (List<Vector3> points) {
		// Generate the matrix of distances between points for faster lookups
		float[,] d = GetDistanceMatrix (points);

		// Starting permutation
		int[] currPerm = new int[points.Count - 1];
		for (int i = 0; i < currPerm.Length; i++)
			currPerm[i] = i + 1;

		// Find the best permutation
		float bestDist = float.PositiveInfinity;
		int[] bestPerm = null;
		do {
			float totalDist = d[0, currPerm[0]];
			for (int i = 0; i < currPerm.Length - 1; i++)
				totalDist += d[currPerm[i], currPerm[i + 1]];
			if (totalDist < bestDist) {
				bestDist = totalDist;
				bestPerm = (int[]) currPerm.Clone();
			}
		} while (GetNextPermutation (currPerm));

		// Add the points in the order given by the best permutation
		List<Vector3> result = new List<Vector3> (points.Count);
		result.Add (points[0]);
		foreach (int i in bestPerm)
			result.Add (points[i]);

		return result;
	}

	// Returns the (symmetric) matrix of distances between points
	// Distances are actual path distances (as calculated by the NavMesh), not straight lines
	private static float[,] GetDistanceMatrix (List<Vector3> points) {
		float[,] d = new float[points.Count, points.Count];
		for (int i = 0; i < points.Count - 1; i++)
		for (int j = i + 1; j < points.Count; j++) {
			d[i, j] = GetDistance (points[i], points[j]);
			d[j, i] = d[i, j];
		}
		return d;
	}

	// Generates the next permutation given the previous one
	// Returns false if the previous one was the last one
	private static bool GetNextPermutation (int[] p) {
		for (int i = p.Length - 1; i >= 0; i--) {
			p[i]++;
			if (p[i] <= p.Length)
				return Repeats (p) ? GetNextPermutation (p) : true;
			p[i] = 1;
		}
		return false;
	}

	// Are there any repeating digits in the current permutation?
	private static bool Repeats (int[] p) {
		for (int i = 0; i < p.Length - 1; i++)
			for (int j = i + 1; j < p.Length; j++)
				if (p[i] == p[j])
					return true;
		return false;
	}




	// Apply a NN algorithm to our list of points and optimize it with 2-Opt
	private static List<Vector3> SolveTSPWithNN2Opt (List<Vector3> points) {
		points = NearestNeighbours (points);
		points = TwoOpt (points);
		return points;
	}

	// Nearest Neighbours (NN)
	private static List<Vector3> NearestNeighbours (List<Vector3> points) {
		List<Vector3> result = new List<Vector3> (points.Count);

		// Add the starting point to the path and remove it from the set of points
		result.Add (points[0]);
		points.RemoveAt (0);

		// While the set of goal points isn't empty
		while (points.Count > 0) {
			// Find the closest neighbour to the last point added
			float bestDist = float.PositiveInfinity;
			Vector3 best = Vector3.zero;
			foreach (Vector3 p in points) {
				float dist = GetDistance (result.Last(), p);
				if (dist < bestDist) {
					bestDist = dist;
					best = p;
				}
			}

			// Add it to the path and remove it from the set of points
			result.Add (best);
			points.Remove (best);
		}
		return result;
	}

	// 2-Opt local optimization incrementally inverts parts of the path to see if a better alternative can be made
	// Fixes intersecting paths
	private static List<Vector3> TwoOpt (List<Vector3> points) {
		// We limit the number of steps to prevent this optimization from becoming too time-consuming
		int steps = nStepsIn2Opt;
		while (steps-- > 0)
			// If the path cannot be locally improved, we're done
			if (!TwoOptImprove (ref points))
				break;

		return points;
	}

	// Attempt to locally optimize the current path
	private static bool TwoOptImprove (ref List<Vector3> points) {
		float dist = GetPathDistance (points);
		// Invert every possible subpath in the path from the first goal point to the last
		for (int i = 1; i < points.Count - 1; i++)
			for (int j = i + 1; j < points.Count; j++) {
				List<Vector3> currPath = TwoOptInvert (points, i, j);

				// If a better path has been found, return true to start over with it
				if (GetPathDistance (currPath) < dist) {
					points = currPath;
					return true;
				}
			}

		// When no improvements have been made, return false
		return false;
	}

	// Invert the subpath from j to k (inclusive)
	private static List<Vector3> TwoOptInvert (List<Vector3> points, int j, int k) {
		List<Vector3> result = new List<Vector3> (points.Count);
		for (int i = 0; i < j; i++)
			result.Add (points[i]);
		for (int i = k; i >= j; i--)
			result.Add (points[i]);
		for (int i = k + 1; i < points.Count; i++)
			result.Add (points[i]);
		return result;
	}



	// Calculate the distance between two points on the NavMesh
	// Returns Infinity if a valid navigation path cannot be found
	private static float GetDistance (Vector3 p1, Vector3 p2) {
		UnityEngine.AI.NavMeshPath path = new UnityEngine.AI.NavMeshPath();
		UnityEngine.AI.NavMeshHit hit;
		if (UnityEngine.AI.NavMesh.SamplePosition (p1, out hit, 1f, UnityEngine.AI.NavMesh.AllAreas))
		    p1 = hit.position;
		if (UnityEngine.AI.NavMesh.SamplePosition (p2, out hit, 1f, UnityEngine.AI.NavMesh.AllAreas))
		    p2 = hit.position;
		if (UnityEngine.AI.NavMesh.CalculatePath (p1, p2, UnityEngine.AI.NavMesh.AllAreas, path)) {
			float distance = 0f;
			for (int i = 1; i < path.corners.Length; i++)
				distance += Vector3.Distance (path.corners[i - 1], path.corners[i]);
			return distance;
		}
		return float.PositiveInfinity;
	}

	// Calculate the total distance of a path, given as a list of points on the mesh
	private static float GetPathDistance (List<Vector3> points) {
		float d = 0f;
		for (int i = 0; i < points.Count - 1; i++)
			d += GetDistance (points[i], points[i + 1]);
		return d;
	}
}