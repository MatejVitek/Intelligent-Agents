using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[RequireComponent (typeof (SphereCollider))]
public class WeaponRack : MonoBehaviour {

	public GameObject tool, toolProp;
	public int nTools;
	public bool drawSpawnZonesInEditor;
	public LinkedList<BoxCollider> spawnZone { get; private set; }
	public int toolsLeft { get; private set; }

	private List<GameObject> currentTools;
	private bool toolsSpawned;
	private Transform nextToolTransform;
	private const float xDeltaBetweenTools = -1.263f;
	private ToolControl toolControl;
	private GameObject spotlight;
	private GameControl gameControl;

	void OnDrawGizmos() {
		if (!drawSpawnZonesInEditor)
			return;
		Gizmos.color = Color.yellow;
		foreach (Transform box in transform.Find ("Spawn Zone")) {
			Vector3[] points = Vision.GetVertices (box.GetComponent<BoxCollider>());
			for (int i = 0; i < points.Length - 1; i++)
				for (int j = i + 1; j < points.Length; j++) {
					Vector3 p1 = points[i];
					Vector3 p2 = points[j];
					if ((i ^ j) == 1 || (i ^ j) == 2 || (i ^ j) == 4)
						Gizmos.DrawLine (p1, p2);
				}
		}
	}

	void Awake() {
		toolsSpawned = false;
		currentTools = new List<GameObject> (nTools);
		spawnZone = new LinkedList<BoxCollider>();
	}

	void Start() {
		spotlight = transform.Find ("Spotlight").gameObject;
		nextToolTransform = transform.Find ("First Tool Location");

		toolControl = GameObject.FindGameObjectWithTag (Tags.player).GetComponent<ToolControl>();
		gameControl = GameObject.FindGameObjectWithTag (Tags.gameController).GetComponent<GameControl>();

		foreach (Transform box in transform.Find ("Spawn Zone"))
			spawnZone.AddLast (box.GetComponent<BoxCollider>());
	}

	void Update() {
		// Spotlight should be active if tools haven't spawned yet or we have tools to deliver.
		spotlight.SetActive (!toolsSpawned || toolControl.nTools > 0);
	}

	void OnTriggerEnter (Collider other) {
		if (!other.gameObject.CompareTag (Tags.player))
			return;
		if (!toolsSpawned) {
			toolsSpawned = true;
			SpawnTools();
		} 
		else {
			int tools = toolControl.RemoveTools();
			if (tools > 0) {
				AddToolsToRack (tools);
				toolsLeft -= tools;
				gameControl.IncreaseScore (tools);
			}
		}
	}

	public List<GameObject> GetCurrentTools() {
		// Delete picked up tools from the list of current tools
		if (currentTools.Count > 0)
			currentTools = currentTools.Where (tool => tool != null).ToList();

		return currentTools;
	}

	private void AddToolsToRack (int nTools) {
		for (int i = 0; i < nTools; i++) {
			GameObject toolInstance = (GameObject) Instantiate (toolProp, nextToolTransform.position, nextToolTransform.rotation);
			toolInstance.transform.parent = transform;
			toolInstance.transform.localScale = nextToolTransform.localScale;
			nextToolTransform.localPosition += new Vector3 (xDeltaBetweenTools, 0f, 0f);
		}
	}

	private void SpawnTools() {
		Vector3[] spawns = GetRandomPointsInToolSpawnZone (nTools);
		toolsLeft = nTools;
		foreach (Vector3 spawn in spawns) {
			GameObject toolInstance = Instantiate (tool);
			toolInstance.transform.position = spawn + 0.3f * Vector3.up;
			toolInstance.transform.Rotate (0f, Random.Range (-180f, 180f), 0f);
			currentTools.Add (toolInstance);
		}
	}

	private Vector3[] GetRandomPointsInToolSpawnZone (int nPoints) {
		Vector3[] points = new Vector3[nPoints];
		for (int i = 0; i < nPoints; i++) {
			BoxCollider box = GetRandomBoxInSpawnZone();
			points[i] = GetRandomPointInBox (box);
		}
		return points;
	}

	private BoxCollider GetRandomBoxInSpawnZone() {
		float sumWeights = 0f;
		foreach (BoxCollider box in spawnZone)
			sumWeights += GetArea (box);

		float rnd = Random.Range (0f, sumWeights);
		sumWeights = 0f;
		foreach (BoxCollider box in spawnZone) {
			sumWeights += GetArea (box);
			if (sumWeights >= rnd)
				return box;
		}
		return null;
	}

	private static float GetArea (BoxCollider box) {
		return Mathf.Abs (box.size.x * box.size.z);
	}

	public static Vector3 GetRandomPointInBox (BoxCollider box) {
		return box.transform.TransformPoint (box.center + new Vector3 (
			Mathf.Lerp (-box.size.x / 2, box.size.x / 2, Random.value),
			-box.size.y / 2,
			Mathf.Lerp (-box.size.z / 2, box.size.z / 2, Random.value)
		));
	}
}
