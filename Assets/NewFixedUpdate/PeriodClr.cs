using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.LowLevel;

public class PeriodClr : MonoBehaviour
{
	public float newPeriod = 0.5f; // Initial new update period

	void Start()
	{
		// Set the initial custom update period
		PlayerLoopExt.SetUpdatePeriod(newPeriod);
	}

	void Update()
	{
		// Press Space to toggle between two different update periods
		if (Input.GetKeyDown(KeyCode.Space))
		{
			// Toggle between two update periods
			newPeriod = (newPeriod == 0.5f) ? 1.5f : 0.5f;
			PlayerLoopExt.SetUpdatePeriod(newPeriod);
			Debug.Log("Custom update period set to: " + newPeriod + " seconds");
		}
	}
}
