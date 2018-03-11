using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SteeringController : MonoBehaviour {
	public GameObject targetObject;

	List<GameObject> characters = new List<GameObject>();

	//Behaviors for flocking
	Separation separationBehavior = new Separation(null);
	Arrive arrivalBehavior = new Arrive(null, new Vector3());
	VelocityMatch velocityMatchBehavior = new VelocityMatch(null, new Vector3());

	// Use this for initialization
	void Start() {
	}
	
	// Update is called once per frame
	void Update() {
		/*
		 * My Implementation of flocking
		 */

		//Get the average group velocity and position
		Vector3 averagePosition = new Vector3();
		Vector3 averageVelocity = new Vector3();
		foreach (GameObject character in characters) {
			averagePosition += character.transform.position;
			averageVelocity += character.GetComponent<SteeringCharacter>().velocity;
		}
		averagePosition *= 1.0f / characters.Count;
		averageVelocity *= 1.0f / characters.Count;

		//averagePosition = targetObject.transform.position;

		//Set to arrive at the average position
		arrivalBehavior.target = averagePosition;
		//Set to match the average velocity
		velocityMatchBehavior.target = averageVelocity;
		//Loop through every character
		foreach (GameObject character in characters) {
			SteeringCharacter thisChar = character.GetComponent<SteeringCharacter>();

			arrivalBehavior.character = thisChar;
			separationBehavior.character = thisChar;
			velocityMatchBehavior.character = thisChar;

			SteeringOutput arrivalSteering = arrivalBehavior.getSteering();
			SteeringOutput separationSteering = separationBehavior.getSteering();
			SteeringOutput velocityMatchSteering = velocityMatchBehavior.getSteering();

			thisChar.steering = new SteeringOutput();
			//Combine all steering behaviors
			thisChar.steering.linear = arrivalSteering.linear + separationSteering.linear + velocityMatchSteering.linear;
		}
	}

	public void AddCharacter(GameObject character) {
		characters.Add(character);
		separationBehavior.targets.Add(character);
	}
}

/*
 * Start of steering behaviors section
 */

public abstract class SteeringBehavior {
	public abstract SteeringOutput getSteering();
}

public class Seek : SteeringBehavior {
	public SteeringCharacter character;
	public GameObject target;
	public float maxAcceleration;

	public Seek(SteeringCharacter _character, GameObject _target, float _maxAcceleration = 10.0f) {
		this.character = _character;
		this.target = _target;
		this.maxAcceleration = _maxAcceleration;
	}

	public override SteeringOutput getSteering() {
		SteeringOutput steering = new SteeringOutput();
		//Get the direction to the target
		steering.linear = target.transform.position - character.position;
		//Give full acceleration along this direction
		steering.linear.Normalize();
		steering.linear *= maxAcceleration;

		//Output the steering
		return steering;
	}
}

public class Flee : SteeringBehavior {
	public SteeringCharacter character;
	public GameObject target;
	public float maxAcceleration;

	public Flee(SteeringCharacter _character, GameObject _target, float _maxAcceleration = 10.0f) {
		this.character = _character;
		this.target = _target;
		this.maxAcceleration = _maxAcceleration;
	}

	public override SteeringOutput getSteering() {
		SteeringOutput steering = new SteeringOutput();
		//Get the direction to the target
		steering.linear = character.position - target.transform.position;
		//Give full acceleration along this direction
		steering.linear.Normalize();
		steering.linear *= maxAcceleration;

		//Output the steering
		return steering;
	}
}

public class Arrive : SteeringBehavior {
	public SteeringCharacter character;
	public Vector3 target;

	public float maxAcceleration;
	public float maxSpeed;
	public float targetRadius = 1.05f;
	public float slowRadius;
	public float timeToTarget;

	public Arrive(SteeringCharacter _character,
	              Vector3 _target,
	              float _maxAcceleration = 10.0f,
	              float _maxSpeed = 10.0f,
	              float _slowRadius = 10.0f,
	              float _timeToTarget = 0.1f) {
		this.character = _character;
		this.target = _target;
		this.maxAcceleration = _maxAcceleration;
		this.maxSpeed = _maxSpeed;
		this.slowRadius = _slowRadius;
		this.timeToTarget = _timeToTarget;
	}

	public override SteeringOutput getSteering() {
		SteeringOutput steering = new SteeringOutput();
		//Direction to target
		Vector3 direction = target - character.position;
		float distance = direction.magnitude;

		//Check if we are there
		if (distance < targetRadius) {
			VelocityMatch getToZero = new VelocityMatch(this.character, new Vector3(0.0f, 0.0f, 0.0f), 5.0f);
			steering = getToZero.getSteering();
			return steering;
		}

		//If we are outside the slow radius, go max speed
		float targetSpeed;
		if (distance > slowRadius) {
			targetSpeed = maxSpeed;
			//Otherwise calculate scaled speed
		} else {
			targetSpeed = maxSpeed * distance / slowRadius;
		}

		//Target velocity combines speed & direction
		Vector3 targetVelocity = direction.normalized * targetSpeed;

		//Acceleration tries to match target velocity
		steering.linear = targetVelocity - character.velocity;
		steering.linear /= timeToTarget;
		//Clamp acceleration
		if (steering.linear.magnitude > maxAcceleration) {
			steering.linear = steering.linear.normalized * maxAcceleration;
		}

		//Return steering data
		return steering;
	}
}

public class VelocityMatch : SteeringBehavior {
	public SteeringCharacter character;
	public Vector3 target;
	public float maxAcceleration;
	public float timeToTarget;

	public VelocityMatch(SteeringCharacter _character, Vector3 _target, float _maxAcceleration = 20.0f, float _timeToTarget = 0.1f) {
		this.character = _character;
		this.target = _target;
		this.maxAcceleration = _maxAcceleration;
		this.timeToTarget = _timeToTarget;
	}

	public override SteeringOutput getSteering() {
		SteeringOutput steering = new SteeringOutput();
		steering.linear = target - character.velocity;
		steering.linear /= timeToTarget;
		if (steering.linear.magnitude > maxAcceleration) {
			steering.linear = steering.linear.normalized * maxAcceleration;
		}
		return steering;
	}
}

public class Pursue : Seek {
	public float maxPrediction;

	public Pursue(SteeringCharacter _character, GameObject _target, float _maxAcceleration = 10.0f, float _maxPrediction = 5.0f) : base(_character, _target, _maxAcceleration) {
		this.maxPrediction = _maxPrediction;
	}

	public override SteeringOutput getSteering() {
		SteeringOutput steering = new SteeringOutput();

		Vector3 direction = target.transform.position - character.position;
		float distance = direction.magnitude;

		//Get current speed
		float speed = character.velocity.magnitude;

		float prediction;
		//Check if speed is too small to give good prediction
		if (speed <= distance / maxPrediction) {
			prediction = maxPrediction;
			//Else, calculate predicted time
		} else {
			prediction = distance / speed;
		}

		//Set the explicit target to the target position plus the predicted time

		Vector3 explicitTarget = target.transform.position + target.GetComponent<Rigidbody>().velocity * prediction;

		//Get the direction to the target
		steering.linear = explicitTarget - character.position;
		//Give full acceleration along this direction
		steering.linear.Normalize();
		steering.linear *= maxAcceleration;

		//Output the steering
		return steering;
	}
}

public class Evade : Flee {
	public float maxPrediction;

	public Evade(SteeringCharacter _character, GameObject _target, float _maxAcceleration = 10.0f, float _maxPrediction = 5.0f) : base(_character, _target, _maxAcceleration) {
		this.maxPrediction = _maxPrediction;
	}

	public override SteeringOutput getSteering() {
		SteeringOutput steering = new SteeringOutput();

		Vector3 direction = target.transform.position - character.position;
		float distance = direction.magnitude;

		//Get current speed
		float speed = character.velocity.magnitude;

		float prediction;
		//Check if speed is too small to give good prediction
		if (speed <= distance / maxPrediction) {
			prediction = maxPrediction;
			//Else, calculate predicted time
		} else {
			prediction = distance / speed;
		}

		//Set the explicit target to the target position plus the predicted time

		Vector3 explicitTarget = target.transform.position + target.GetComponent<Rigidbody>().velocity * prediction;

		//Get the direction to the target
		steering.linear = character.position - explicitTarget;
		//Give full acceleration along this direction
		steering.linear.Normalize();
		steering.linear *= maxAcceleration;

		//Output the steering
		return steering;
	}
}

public class Separation : SteeringBehavior {
	public SteeringCharacter character;
	public List<GameObject> targets = new List<GameObject>();
	public float threshold;
	public float decayCoefficient;
	public float maxAcceleration;

	public Separation(SteeringCharacter _character, float _threshold = 5.0f, float _decayCoefficient = 100.0f, float _maxAcceleration = 10.0f) {
		this.character = _character;
		this.threshold = _threshold;
		this.decayCoefficient = _decayCoefficient;
		this.maxAcceleration = _maxAcceleration;
	}

	public override SteeringOutput getSteering() {
		SteeringOutput steering = new SteeringOutput();
		//Loop through every target
		foreach (GameObject target in targets) {
			//Can't separate from self
			if (target == character.gameObject) {
				continue;
			}
			//Check if the target is close
			Vector3 direction = target.transform.position - character.position;
			float distance = direction.magnitude;
			if (distance < threshold) {
				//Calculate the strength of repulsion
				float strength = Mathf.Min(decayCoefficient / (distance * distance), maxAcceleration);
				//Add it to the steering linear force
				steering.linear += strength * -direction.normalized;
			}
		}
		//Finished looping, return
		return steering;
	}
}
