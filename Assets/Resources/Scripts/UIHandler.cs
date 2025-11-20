using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIHandler : MonoBehaviour {
	// Environment information
	private WorldHandler worldHandler;
	private float currentDay, meanSpeed, meanSenseRange, meanSize, prevMeanSpeed, prevMeanSenseRange, prevMeanSize;
	private int currentPop;

	// UI elements
	private TextMeshProUGUI dayCounter, popCounter, statDisplay;
	private Slider daySlider;

	// Elements for graphing
	private GameObject graphPanel, popGraph, speedGraph;
	private readonly float minDataPointY = 23f;
	private readonly float maxDataPointY = 275f;

	private bool initialUpdateDone;

	void Start() {
		worldHandler = GetComponent<WorldHandler>();

		prevMeanSpeed = 0f;
		prevMeanSenseRange = 0f;
		prevMeanSize = 0f;

		dayCounter = GameObject.Find("Day Counter").GetComponent<TextMeshProUGUI>();
		daySlider = GameObject.Find("Day Bar").GetComponent<Slider>();
		popCounter = GameObject.Find("Pop Counter").GetComponent<TextMeshProUGUI>();
		statDisplay = GameObject.Find("Stat Display").GetComponent<TextMeshProUGUI>();

		graphPanel = Resources.Load<GameObject>("Prefabs/Graph Background");
		popGraph = Graph(1680f, 810f, "Day", "Population");
		popGraph.transform.SetParent(GameObject.Find("UI").transform);
		//speedGraph = Graph(1680f, 810f, "Day", "Mean Speed");
		//speedGraph.transform.SetParent(GameObject.Find("UI").transform);

		initialUpdateDone = false;
	}

	void Update() {
		currentDay = worldHandler.GetCurrentDay();

		if (!initialUpdateDone) {
			UpdateUI();
			initialUpdateDone = true;
		}

		dayCounter.text = $"Day: {Mathf.FloorToInt(currentDay)}";
		daySlider.value = currentDay - Mathf.FloorToInt(currentDay);

		float[] popData = worldHandler.GetPopulationData();
		UpdateGraph(popGraph, popData);
		//float[] speedData = worldHandler.GetSpeedData();
		//UpdateGraph(speedGraph, speedData);
	}

	public void UpdateUI() {
		currentPop = worldHandler.GetCreatureCount();
		meanSpeed = worldHandler.GetMeanCreatureTraits()[0];
		meanSenseRange = worldHandler.GetMeanCreatureTraits()[1];
		meanSize = worldHandler.GetMeanCreatureTraits()[2];

		char deltaSpeed;
		if (meanSpeed > prevMeanSpeed) {
			deltaSpeed = '↑';
		} else if (meanSpeed < prevMeanSpeed) {
			deltaSpeed = '↓';
		} else {
			deltaSpeed = '-';
		}

		char deltaSense;
		if (meanSenseRange > prevMeanSenseRange) {
			deltaSense = '↑';
		} else if (meanSenseRange < prevMeanSenseRange) {
			deltaSense = '↓';
		} else {
			deltaSense = '-';
		}

		char deltaSize;
		if (meanSize > prevMeanSize) {
			deltaSize = '↑';
		} else if (meanSize < prevMeanSize) {
			deltaSize = '↓';
		} else {
			deltaSize = '-';
		}

		string meanSpeedStr = meanSpeed.ToString("n2");
		string meanSenseRangeStr = meanSenseRange.ToString("n2");
		string meanSizeStr = meanSize.ToString("n2");

		popCounter.text = $"Pop: {currentPop}";
		statDisplay.text = $"Mean speed: {meanSpeedStr} {deltaSpeed}\nMean sense range: {meanSenseRangeStr} {deltaSense}\nMean size: {meanSizeStr} {deltaSize}";

		prevMeanSpeed = meanSpeed;
		prevMeanSenseRange = meanSenseRange;
		prevMeanSize = meanSize;
	}

	// Create and return a graph which is displayed on the UI
	private GameObject Graph(float xPos, float yPos, string xLabel = "X", string yLabel = "Y") {
		GameObject graph = Instantiate(graphPanel, new(xPos, yPos, 0f), Quaternion.Euler(0f, 0f, 0f));

		graph.transform.Find("X Axis Line").transform.Find("X Axis Label").GetComponent<TextMeshProUGUI>().text = xLabel;
		graph.transform.Find("Y Axis Line").transform.Find("Y Axis Label").GetComponent<TextMeshProUGUI>().text = yLabel;

		return graph;
	}

	// Update the datapoints on a graph object
	private void UpdateGraph(GameObject targetGraph, float[] yData) {
		yData = yData.Skip(Mathf.Max(0, yData.Length - 17)).ToArray();
		float noOfDatapoints = yData.Length;

		float minY = 0f;
		float maxY = 100f + yData.Max();

		Transform dataPoint;
		float xPos, yPos;
		for (int i = 0; i < 17; ++i) {
			dataPoint = targetGraph.transform.Find($"Datapoint {i}");
			xPos = dataPoint.transform.localPosition.x;

			if (i < noOfDatapoints && noOfDatapoints > 0) {
				yPos = ((maxDataPointY - minDataPointY) / 2f) * (yData[i] / (maxY - minY));
				yPos -= 100f;
			} else {
				yPos = minDataPointY - 150f;
			}

			dataPoint.localPosition = new(xPos, yPos, 0f);
		}
	}
}
