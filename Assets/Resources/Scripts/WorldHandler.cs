using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WorldHandler : MonoBehaviour {
	// UI handler script
	private UIHandler uiHandler;

	// World/environment parameters
	private readonly float maxX = 10f;
	private readonly float maxZ = 10f;
	private readonly int foodPerDay = 100;
	private readonly int initialCreatures = 20;
	private float[] worldLimits;
	private int deciDayCounter = 0;
	private float dayTimer;

	// A list of all creatures in the world, and ones to be deleted
	private List<GameObject> creatures, creaturesForDeletion;

	// A list of all food in the world
	private GameObject[] food;

	// Lists containing graphable information
	private readonly List<float> popData = new();
	private readonly List<float> speedData = new();

	// Prefabs
	private GameObject planePrefab, creaturePrefab, foodPrefab;

	void Start() {
		Time.timeScale = 4f;

		uiHandler = GetComponent<UIHandler>();

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

		popData.Add(initialCreatures);
		speedData.Add(0f);

		creaturesForDeletion = new();

		// Start the day timer
		dayTimer = 0f;
	}

	void Update() {
		int previous = deciDayCounter;

		if (dayTimer >= 60f) {
			++deciDayCounter;
			dayTimer = 0f;
		} else {
			++dayTimer;
		}

		/*
		 * Check if a new day has dawned
		 */
		if (deciDayCounter % 10 == 0 && deciDayCounter > 0 && deciDayCounter != previous) {
			uiHandler.UpdateUI();
			popData.Add(creatures.Count);

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

			float meanSpeed = 0f;
			Creature creatureScript;
			foreach (GameObject creature in creatures) {
				creatureScript = creature.GetComponent<Creature>();
				creatureScript.ResetCreature();
				creatureScript.SetSurvivedState(false);
				meanSpeed += creatureScript.GetTraits()[0];

				int distanceAway = Random.Range(-4, 5);
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

				GameObject offspring = Instantiate(creaturePrefab, spawnLocation, Quaternion.Euler(0f, 0f, 0f));
				float[] newTraits = {
						creatureScript.GetTraits()[0] + Random.Range(-1f, 1f),
						creatureScript.GetTraits()[1] + Random.Range(-1f, 1f),
						creatureScript.GetTraits()[2] + Random.Range(-1f, 1f)
				};
				offspring.GetComponent<Creature>().SetTraits(newTraits);
			}

			meanSpeed /= creatures.Count;
			speedData.Add(meanSpeed);
		}
	}

	// Add in the food at random points across the plane
	private void SpawnFood() {
		float xPos, zPos;
		for (int i = 0; i < foodPerDay; ++i) {
			xPos = Random.Range(worldLimits[0], worldLimits[1]);
			zPos = Random.Range(worldLimits[2], worldLimits[3]);

			Instantiate(foodPrefab, new(xPos, 0.25f, zPos), Quaternion.Euler(0f, 0f, 0f));
		}
	}

	// Add in the initial creatures at the boundaries of the plane
	private void SpawnCreatures() {
		float xPos, zPos;
		for (int i = 0; i < initialCreatures; ++i) {
			xPos = Random.Range(worldLimits[0], worldLimits[1]);
			zPos = Random.Range(worldLimits[2], worldLimits[3]);

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

	public float[] GetPopulationData() {
		return popData.ToArray();
	}

	public float[] GetSpeedData() {
		return speedData.ToArray();
	}

	public float[] GetMeanCreatureTraits() {
		float meanSpeed = 0f;
		float meanSenseRange = 0f;
		float meanSize = 0f;

		creatures = GameObject.FindGameObjectsWithTag("Creature").ToList();

		float[] creatureTraits;
		foreach (GameObject creature in creatures) {
			creatureTraits = creature.GetComponent<Creature>().GetTraits();

			meanSpeed += creatureTraits[0];
			meanSenseRange += creatureTraits[1];
			meanSize += creatureTraits[2];
		}

		meanSpeed /= creatures.Count;
		meanSenseRange /= creatures.Count;
		meanSize /= creatures.Count;

		return new float[] { meanSpeed, meanSenseRange, meanSize };
	}
}
