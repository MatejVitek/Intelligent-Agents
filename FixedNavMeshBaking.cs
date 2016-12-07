#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

#pragma warning disable 0618

public class NavMeshFixer : ScriptableObject {

	[MenuItem ("Assets/Bake Fixed Navigation Mesh")]
	public static void FixNavMesh() {        
		Undo.RegisterSceneUndo ("BakeNavMesh");
		
		List<Renderer> disabledObjects = new List<Renderer>();
		
		foreach (Renderer item in Object.FindObjectsOfType<Renderer>())
			//Check if its marked as NavigationStatic, and it has a disabled renderer
			if (GameObjectUtility.AreStaticEditorFlagsSet (item.gameObject, StaticEditorFlags.NavigationStatic) && !item.enabled) {
				disabledObjects.Add (item);
				item.enabled = true;
			}
		
		UnityEditor.AI.NavMeshBuilder.BuildNavMesh();
		
		disabledObjects.ForEach (obj => obj.enabled = false); //Disable the objects again.
		
		Debug.Log ("Done building navmesh, " + disabledObjects.Count + " objects affected.");
	}
}
#endif