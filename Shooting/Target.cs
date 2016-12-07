using UnityEngine;
using System.Collections;

public class Target : MonoBehaviour {

	private GameObject controller;

	void Start() {
		controller = GameObject.FindGameObjectWithTag (Tags.gameController);
	}

	public void HitTarget() {
		controller.SendMessage ("NextTarget");
		controller.SendMessage ("IncreaseScore");
	}
}
