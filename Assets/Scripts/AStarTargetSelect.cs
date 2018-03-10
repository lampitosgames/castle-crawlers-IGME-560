using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;
using UnityEngine;

public class AStarTargetSelect : MonoBehaviour {

	public Transform aStarTargetGameObject;
	public GameObject aStarCharacter;
	
	// Update is called once per frame
	void Update() {
		//Detect left click
		if (Input.GetMouseButtonDown(0)) {
			//Raycast from camera.  Collision sets the target of the A* algorithm
			Vector3 forward = GetComponent<Camera>().transform.forward;

			RaycastHit hit;
			Physics.Raycast(transform.position, forward, out hit, 1000.0f);
			if (hit.point != null) {
				aStarTargetGameObject.position = hit.point;
				aStarCharacter.GetComponent<NavMeshAgent>().destination = hit.point;
			}
		}
	}
}
