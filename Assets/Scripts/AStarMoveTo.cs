using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class AStarMoveTo : MonoBehaviour
{
	public Transform goalPosition;
	public Material lineMaterial;

	NavMeshAgent agent = null;

	void Start ()
	{
		agent = GetComponent<NavMeshAgent> ();
		agent.destination = goalPosition.position;
	}

	void Update ()
	{
		if (agent == null || agent.path == null) {
			return;
		}

		LineRenderer line = this.GetComponent<LineRenderer> ();
		if (line == null) {
			line = this.gameObject.AddComponent<LineRenderer> ();
			line.material = lineMaterial;
			line.SetWidth (0.2f, 0.2f);
			line.SetColors (Color.cyan, Color.cyan);
		}

		NavMeshPath path = agent.path;

		line.positionCount = path.corners.Length;
		for (int i = 0; i < path.corners.Length; i++) {
			line.SetPosition (i, path.corners [i] + new Vector3 (0.0f, 1.0f, 0.0f));
		}
	}

}
