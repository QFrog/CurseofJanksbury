using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controls the UI canvas containing all debug GUI objects.
/// </summary>
public class DebugCanvas : MonoBehaviour
{
	[SerializeField] private GameObject statMonitorCanvas = null;
	[SerializeField] private bool showStatMonitorByDefault = true;
	[SerializeField] private GameObject playerDebugCanvas = null;
	[SerializeField] private bool showPlayerDebugByDefault = false;

	// Start is called before the first frame update
	void Start()
	{
		statMonitorCanvas.SetActive(showStatMonitorByDefault);
		playerDebugCanvas.SetActive(showPlayerDebugByDefault);
	}

	// Update is called once per frame
	void Update()
	{
		// Toggle Stat Monitor with "~" key
		if (Input.GetKeyDown(KeyCode.BackQuote))
		{
			statMonitorCanvas.SetActive(!statMonitorCanvas.activeInHierarchy);
		}

		// Toggle player debug with "F1" key
		if (Input.GetKeyDown(KeyCode.F1))
		{
			playerDebugCanvas.SetActive(!playerDebugCanvas.activeInHierarchy);
		}
	}
}
