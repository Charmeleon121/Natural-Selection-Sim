using UnityEngine;

public class Creature : MonoBehaviour {
	// Environment information
	private WorldHandler worldHandler;
	private float[] worldLimits;
	private Vector3 initialLocation;

	// Whether the creature has survived the day
	private bool survived;

	// The energy of the creature
	private float energy;

	// The traits of the creature
	private float speed;
	private float detectionRadius;

	void Awake() {
		speed = Random.Range(1f, 10f);
		detectionRadius = Random.Range(1f, 10f);

		survived = false;
		energy = 100f;
	}

	void Start() {
		worldHandler = GameObject.Find("EventSystem").GetComponent<WorldHandler>();
		worldLimits = worldHandler.GetWorldLimits();

		initialLocation = transform.position;

		Transform detectionRing = transform.Find("Detection");
		detectionRing.localScale = new(detectionRadius * 100f, detectionRadius * 100f, 100f);
	}

	void Update() {
		if (survived) {
			transform.position = initialLocation;
		}
	}

	void FixedUpdate() {
		if (!survived && energy > 0) {
			Collider[] hits = Physics.OverlapSphere(transform.position, detectionRadius);
			Transform target = null;
			foreach (Collider hit in hits) {
				if (hit.CompareTag("Food")) {
					target = hit.transform;
					break;
				}
			}

			Vector3 direction;
			if (target != null) {
				direction = (target.position - transform.position).normalized;
			} else {
				direction = new Vector3(
					Mathf.PerlinNoise(Time.time * 0.5f, transform.position.z) - 0.5f,
					0f,
					Mathf.PerlinNoise(transform.position.x, Time.time * 0.5f) - 0.5f
				).normalized;
			}

			Vector3 newPosition = transform.position + speed * Time.fixedDeltaTime * direction;

			newPosition.x = Mathf.Clamp(newPosition.x, worldLimits[0], worldLimits[1]);
			newPosition.z = Mathf.Clamp(newPosition.z, worldLimits[2], worldLimits[3]);
			newPosition.y = 1f;

			Vector3 oldPosition = transform.position;
			transform.position = newPosition;

			energy = Mathf.Max(0f, energy - Vector3.Distance(oldPosition, newPosition));
		}
	}

	// Completely reset a creature to initial state and position
	public void ResetCreature() {
		energy = 100;
		transform.position = initialLocation;
	}

	private void OnCollisionEnter(Collision collision) {
		if (collision.gameObject.CompareTag("Food")) {
			/* 
			 * The creature has found food - send it back to its
			 * initial position and replenish its energy
			 * 
			 * Then, remove (consume) the food object
			 */
			survived = true;
			ResetCreature();

			Destroy(collision.gameObject);
		}
	}

	// Get whether the creature survived or not
	public bool Survived() {
		return survived;
	}

	// Set the survived state (only for new days)
	public void SetSurvivedState(bool state) {
		survived = state;
	}
}
