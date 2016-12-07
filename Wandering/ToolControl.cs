using UnityEngine;
using System.Collections;

public class ToolControl : MonoBehaviour {

	public int nTools { get; private set; }

	void Awake() {
		nTools = 0;
	}

	void OnTriggerEnter (Collider col) {
		GameObject tool = col.gameObject;
		if (!tool.CompareTag (Tags.tool))
		    return;

		nTools++;
		Destroy (col.gameObject);
	}

	public int RemoveTools() {
		int result = nTools;
		nTools = 0;
		return result;
	}
}
