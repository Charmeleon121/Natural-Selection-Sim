using TMPro;
using UnityEngine;

public class UIHandler : MonoBehaviour {
	// Environment information
	private WorldHandler worldHandler;
	private int currentDay, currentPop;

	// UI elements
	private TextMeshProUGUI dayCounter, popCounter;

	void Start() {
		worldHandler = GetComponent<WorldHandler>();

		dayCounter = GameObject.Find("Day Counter").GetComponent<TextMeshProUGUI>();
		popCounter = GameObject.Find("Pop Counter").GetComponent<TextMeshProUGUI>();
	}

	void Update() {
		currentDay = Mathf.FloorToInt(worldHandler.GetCurrentDay());
		currentPop = worldHandler.GetCreatureCount();

		dayCounter.text = $"Day: {currentDay}";
		popCounter.text = $"Pop: {currentPop}";
	}
}
