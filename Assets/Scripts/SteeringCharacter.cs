using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SteeringCharacter : MonoBehaviour
{

	public Vector3 velocity = new Vector3 (0.0f, 0.0f, 0.0f);
	public float maxSpeed = 3.0f;

	// Use this for initialization
	void Start ()
	{
		
	}
	
	// Update is called once per frame
	void FixedUpdate ()
	{
		velocity = new Vector3 (1.0f, 0.0f, 0.0f);

		//Get a normal to the floor by spherecasting
		RaycastHit hitInfo;
		Physics.SphereCast (transform.position, 0.5f, Vector3.down, out hitInfo, 3.0f, Physics.AllLayers, QueryTriggerInteraction.Ignore);
		//Project the velocity onto the new plane, normalize, and re-lengthen it to the default speed
		velocity = Vector3.ProjectOnPlane (velocity, hitInfo.normal).normalized * maxSpeed;

		//Update the velocity based on steering
		//Move the character
		transform.position += velocity * Time.fixedDeltaTime;
	}
}
