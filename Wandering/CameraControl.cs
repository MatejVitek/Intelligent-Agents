using UnityEngine;
using System.Collections;

public class CameraControl : MonoBehaviour {
	public Camera initialCamera;
	
	private int currentCameraIndex;
	private Camera[] cameras;

	void Start() {
		GameObject[] cameraObjects = GameObject.FindGameObjectsWithTag (Tags.mainCamera);
		cameras = new Camera[cameraObjects.Length];
		for (int i = 0; i < cameraObjects.Length; i++) {
			Camera camera = cameraObjects[i].GetComponent<Camera>();
			cameras[i] = camera;
			cameras[i].enabled = camera.Equals (initialCamera);
			if (camera.Equals (initialCamera))
				currentCameraIndex = i;
		}
	}

	void Update() {
		if (Input.GetButtonDown ("Fire1")) {
			cameras[currentCameraIndex++].enabled = false;
			if (currentCameraIndex >= cameras.Length) currentCameraIndex = 0;
			cameras[currentCameraIndex].enabled = true;
		}
	}
}
