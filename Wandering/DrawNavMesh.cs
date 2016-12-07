using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DrawNavMesh : MonoBehaviour {
	public Material material;

	private GameObject player;
	private UnityEngine.AI.NavMeshAgent agent;
	private Mesh navMesh, wpMesh;
	private bool draw;
	private Material pathMaterial;
	private List<GameObject> lineRenderers;
	private GameObject path;

	private BasicGBA gba;
	private StateBasedGBA sbgba;
	private MemoryBasedGBA mbgba;
	private ExplorationBasedGBA ebgba;
	private UBA uba;

	void Awake() {
		draw = false;

		UnityEngine.AI.NavMeshTriangulation nav = UnityEngine.AI.NavMesh.CalculateTriangulation();
		navMesh = new Mesh();
		navMesh.vertices = nav.vertices;
		navMesh.triangles = nav.indices;

		GameObject sphere = GameObject.CreatePrimitive (PrimitiveType.Sphere);
		sphere.transform.localScale = new Vector3 (0.01f, 0.01f, 0.01f);
		wpMesh = sphere.GetComponent<MeshFilter>().mesh;

		Shader s = Shader.Find ("Unlit/Color");
		Material lineMaterial = new Material (s);
		lineMaterial.hideFlags = HideFlags.HideAndDontSave;
		lineMaterial.color = Color.blue;
		pathMaterial = new Material (s);
		pathMaterial.hideFlags = HideFlags.HideAndDontSave;
		pathMaterial.color = Color.red;

		lineRenderers = new List<GameObject> (navMesh.triangles.Length / 3);
		/*for (int i = 0; i < navMesh.triangles.Length; i += 3) {
			GameObject obj = new GameObject ("renderer");
			LineRenderer renderer = obj.AddComponent<LineRenderer>();
			renderer.useWorldSpace = true;
			renderer.SetWidth (0.01f, 0.01f);
			renderer.material = lineMaterial;
			renderer.SetVertexCount (3);
			for (int j = 0; j < 3; j++)
				renderer.SetPosition (j, navMesh.vertices [navMesh.triangles [i + j]] + 0.15f * Vector3.up);
			lineRenderers.Add (obj);
		}*/
	}

	void Start() {
		player = GameObject.FindGameObjectWithTag (Tags.player);
		agent = player.GetComponent<UnityEngine.AI.NavMeshAgent>();

		gba = player.GetComponent<BasicGBA>();
		sbgba = player.GetComponent<StateBasedGBA>();
		mbgba = player.GetComponent<MemoryBasedGBA>();
		ebgba = player.GetComponent<ExplorationBasedGBA>();
		uba = player.GetComponent<UBA>();
	}

	void Update() {			
		if (path != null)
			Destroy (path);
		if (Input.GetKeyDown (KeyCode.N))
			draw = !draw;
		foreach (GameObject renderer in lineRenderers)
			renderer.SetActive (draw);
		if (draw) {
			Graphics.DrawMesh (navMesh, Vector3.zero + 0.1f * Vector3.up, Quaternion.identity, material, 0);
			DrawCurrentPath();
			DrawWaypoints();
		}
	}

	private void DrawCurrentPath() {
		path = new GameObject ("path");
		LineRenderer renderer = path.AddComponent<LineRenderer>();
		renderer.useWorldSpace = true;
		renderer.SetWidth (0.05f, 0.05f);
		renderer.material = pathMaterial;
		renderer.SetVertexCount (agent.path.corners.Length);
		for (int i = 0; i < agent.path.corners.Length; i++)
			renderer.SetPosition (i, agent.path.corners[i] + 0.3f * Vector3.up);
	}

	private void DrawWaypoints() {
		if (gba != null && gba.waypoints != null) {
			for (int i = gba.currentWP; i < gba.waypoints.Length; i++)
				DrawWaypoint (gba.waypoints[i]);
			return;
		}

		if (sbgba != null && sbgba.waypoints != null) {
			for (int i = sbgba.currentWP; i < sbgba.waypoints.Length; i++)
				DrawWaypoint (sbgba.waypoints[i]);
			return;
		}

		if (mbgba != null && mbgba.waypoints != null && mbgba.priorityGoals != null) {
			for (int i = mbgba.currentWP; i < mbgba.waypoints.Length; i++)
				DrawWaypoint (mbgba.waypoints[i]);
			foreach (GameObject wp in mbgba.priorityGoals)
				if (wp != null)
					DrawWaypoint (wp.transform.position);
			return;
		}

		if (ebgba != null && ebgba.priorityGoals != null) {
			DrawWaypoint (ebgba.exploreWP);
			foreach (GameObject wp in ebgba.priorityGoals)
				if (wp != null)
					DrawWaypoint (wp.transform.position);
			return;
		}

		if (uba != null && uba.waypoints != null) {
			for (int i = uba.currentWP; i < uba.waypoints.Count; i++)
				DrawWaypoint (uba.waypoints[i]);
			return;
		}
	}

	private void DrawWaypoint (Vector3 wp) {
		Graphics.DrawMesh (wpMesh, wp + 0.3f * Vector3.up, Quaternion.identity, pathMaterial, 0);
	}
}
