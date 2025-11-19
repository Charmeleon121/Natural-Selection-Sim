using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WorldHandler : MonoBehaviour {
	// World/environment parameters
	private readonly float maxX = 10f;
	private readonly float maxZ = 10f;
	private readonly int foodPerDay = 50;
	private readonly int initialCreatures = 20;
	private float[] worldLimits;
	private int deciDayCounter = 0;
	private float dayTimer;

	// A list of all creatures in the world, and ones to be deleted
	private List<GameObject> creatures, creaturesForDeletion;

	// A list of all food in the world
	private GameObject[] food;

	// Prefabs
	private GameObject planePrefab, creaturePrefab, foodPrefab;

	void Start() {
		// Load the prefabs
		planePrefab = Resources.Load<GameObject>("Prefabs/Plane");
		creaturePrefab = Resources.Load<GameObject>("Prefabs/Creature");
		foodPrefab = Resources.Load<GameObject>("Prefabs/Food");

		// Establish the world boundaries
		worldLimits = new float[] {
			(-maxX * 10f / 2f) + 1f,
			(maxX * 10f / 2f) - 1f,
			(-maxZ * 10f / 2f) + 1f,
			(maxZ * 10f / 2f) - 1f
		};

		// Create the plane for the creatures to exist on
		GameObject plane = Instantiate(planePrefab, new(0f, 0f, 0f), Quaternion.Euler(0f, 0f, 0f));
		plane.transform.localScale = new(maxX, 1f, maxZ);

		SpawnFood();
		SpawnCreatures();

		creaturesForDeletion = new();

		// Start the day timer
		dayTimer = 0f;
	}

	void Update() {
		int previous = deciDayCounter;

		if (dayTimer >= 300f) {
			++deciDayCounter;
			dayTimer = 0f;
		} else {
			++dayTimer;
		}

		/*
		 * Check if a new day has dawned
		 */
		if (deciDayCounter % 10 == 0 && deciDayCounter > 0 && deciDayCounter != previous) {
			creatures = GameObject.FindGameObjectsWithTag("Creature").Where(c => c.GetComponent<Creature>().Survived()).ToList();
			creaturesForDeletion = GameObject.FindGameObjectsWithTag("Creature").Where(c => !c.GetComponent<Creature>().Survived()).ToList();
			food = GameObject.FindGameObjectsWithTag("Food");

			// Delete any dead creatures first
			foreach (GameObject creature in creaturesForDeletion) {
				Destroy(creature);
			}

			// Remove all the food and replace it
			foreach (GameObject foodObj in food) {
				Destroy(foodObj);
			}
			SpawnFood();

			Creature creatureScript;
			foreach (GameObject creature in creatures) {
				creatureScript = creature.GetComponent<Creature>();
				creatureScript.ResetCreature();
				creatureScript.SetSurvivedState(false);

				float distanceAway = Random.Range(-4, 5);
				Vector3 spawnLocation = new(creature.transform.position.x + distanceAway, creature.transform.position.y, creature.transform.position.z + distanceAway);

				if (spawnLocation.x < 0) {
					spawnLocation.x = Mathf.Max(spawnLocation.x, worldLimits[0]);
				} else {
					spawnLocation.x = Mathf.Min(spawnLocation.x, worldLimits[1]);
				}

				if (spawnLocation.z < 0) {
					spawnLocation.z = Mathf.Max(spawnLocation.z, worldLimits[2]);
				} else {
					spawnLocation.z = Mathf.Min(spawnLocation.z, worldLimits[3]);
				}

				Instantiate(creaturePrefab, spawnLocation, Quaternion.Euler(0f, 0f, 0f));
			}
		}
	}

	// Add in the food at random points across the plane
	private void SpawnFood() {
		float xPos, zPos;
		for (int i = 0; i < foodPerDay; ++i) {
			xPos = Random.Range(-maxX * 10f / 2f, maxX * 10f / 2f);
			zPos = Random.Range(-maxZ * 10f / 2f, maxZ * 10f / 2f);

			Instantiate(foodPrefab, new(xPos, 0.25f, zPos), Quaternion.Euler(0f, 0f, 0f));
		}
	}

	// Add in the initial creatures at the boundaries of the plane
	private void SpawnCreatures() {
		float xPos, zPos;
		for (int i = 0; i < initialCreatures; ++i) {
			float randomChance;
			float randomBoundaryChoice = Random.Range(0f, 9f);

			if (randomBoundaryChoice <= 4f) {
				randomChance = Random.Range(0f, 9f);
				if (randomChance <= 4f) {
					xPos = worldLimits[0];
				} else {
					xPos = worldLimits[1];
				}

				zPos = Mathf.RoundToInt(Random.Range(worldLimits[2], worldLimits[3]));
			} else {
				randomChance = Random.Range(0f, 9f);
				if (randomChance <= 4f) {
					zPos = worldLimits[2];
				} else {
					zPos = worldLimits[3];
				}

				xPos = Mathf.RoundToInt(Random.Range(worldLimits[0], worldLimits[1]));
			}

			Instantiate(creaturePrefab, new(xPos, 1f, zPos), Quaternion.Euler(0f, 0f, 0f));
		}
	}

	public float[] GetWorldLimits() {
		return worldLimits;
	}

	public float GetCurrentDay() {
		return deciDayCounter / 10f;
	}

	public int GetCreatureCount() {
		creatures = GameObject.FindGameObjectsWithTag("Creature").ToList();
		return creatures.Count;
	}
}
