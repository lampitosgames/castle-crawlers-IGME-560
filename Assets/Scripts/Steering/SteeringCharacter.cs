using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SteeringCharacter : MonoBehaviour {
	public Vector3 position = new Vector3();
	public Vector3 velocity = new Vector3();
	public float orientation = 0.0f;
	public float rotation = 0.0f;

	public float maxSpeed = 10.0f;
	public float maxRotation = 10.0f;

	public SteeringOutput steering = new SteeringOutput(new Vector3(), 0.0f);

	//Private vars
	private Rigidbody thisRb;
	private SteeringController steeringController;

	// Use this for initialization
	void Start() {
		thisRb = GetComponent<Rigidbody>();
		thisRb.freezeRotation = true;

		steeringController = GameObject.Find("SteeringController").GetComponent<SteeringController>();
		steeringController.AddCharacter(gameObject);

		this.position = transform.position;
		this.velocity = thisRb.velocity;
		this.orientation = transform.eulerAngles.y;
		this.rotation = 0.0f;

	}
	
	// Update is called once per frame
	void FixedUpdate() {
		//position set implicitly by the rigidbody
		this.position = transform.position;
		this.orientation += rotation * Time.deltaTime;
		//Apply steering
		this.velocity += steering.linear * Time.deltaTime;
		this.rotation += steering.angular * Time.deltaTime;
		//Clamp to respective maximums
		this.rotation = Mathf.Clamp(this.rotation, 0.0f, maxRotation);
		if (this.velocity.magnitude > maxSpeed) {
			this.velocity = this.velocity.normalized * maxSpeed;
		}

		//Code after this is to convert the steering code into unity-understandable outputs.

		//Get a normal to the floor by spherecasting
		RaycastHit hitInfo;
		Physics.SphereCast(transform.position, 0.5f, Vector3.down, out hitInfo, 3.0f, Physics.AllLayers, QueryTriggerInteraction.Ignore);

		//Project the velocity onto the new plane, normalize, and re-lengthen it to the default speed
		thisRb.velocity = Vector3.ProjectOnPlane(velocity, hitInfo.normal).normalized * velocity.magnitude;
		//Set the rotation
		transform.eulerAngles = new Vector3(transform.eulerAngles.x, orientation, transform.eulerAngles.z);
	}
}

public class SteeringOutput {
	public Vector3 linear = new Vector3();
	public float angular = 0.0f;

	public SteeringOutput() {
	}

	public SteeringOutput(Vector3 _linear) {
		this.linear = _linear;
	}

	public SteeringOutput(Vector3 _linear, float _angular) {
		this.linear = _linear;
		this.angular = _angular;
	}
}