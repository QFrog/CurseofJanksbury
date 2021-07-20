// Copyright (c) 2019 Jakob Bjerkness. All rights reserved.

using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

/// <summary>
/// Updates a TextMeshProUGUI component's text with the current FPS and frame frequency every 'updateFrequency' seconds.
/// Uses cached strings, a character array, and a StringBuilder object to avoid creating any allocations.
/// </summary>
///
public class FPSCounter : MonoBehaviour
{
	// ----------------------------------------------------------------------------
	// Inspector Fields
	// ----------------------------------------------------------------------------

	//============== Text ==============

	[Header("Text")]

	[SerializeField]
	[Tooltip("The text mesh component to update with the FPS stats.")]
	private TextMeshProUGUI targetTextMesh = null; // Can't be null in inspector

	[SerializeField]
	[Tooltip("The initial length of the character array used to update the text. " +
	"If the text grows longer than this value it will cause a new array to be allocated.")]
	private int estimatedMaxTextLength = 174;

	[Tooltip("Defines how much information is shown. All calculations will continue to happen despite this setting's value.")]
	public FPSCounterState displayStyle = FPSCounterState.Full;


	//============== Time ==============

	[Header("Time")]

	[SerializeField]
	[Tooltip("The frequency (in seconds) at which the FPS stats are recalculated and the text updated.")]
	private float updateFrequency = 0.7f; // Can't be zero

	[SerializeField]
	[Tooltip("The amount of time in the past to calculate the average FPS.")]
	private int previousSecondsToAverage = 10; // Can't be negative

	[SerializeField]
	[Tooltip("The amount of time in the past to calculate the minimum and maximum FPS.")]
	private int previousSecondsToMinMax = 20; // Can't be negative

	//============== Color ==============

	[Header("Color")]

	[SerializeField]
	[Tooltip("FPS at or below this value is shown in the warning color.")]
	private int warningFPSLevel = 60;

	[SerializeField]
	[Tooltip("FPS at or below this value is shown in the critical color.")]
	private int criticalFPSLevel = 30;

	[SerializeField]
	private Color nominalColor = new Color(91 / 255f, 233 / 255f, 110 / 255f, 255 / 255f);

	[SerializeField]
	private Color warningColor = new Color(236 / 255f, 224 / 255f, 88 / 255f, 255 / 255f);

	[SerializeField]
	private Color criticalColor = new Color(255 / 255f, 107 / 255f, 107 / 255f, 255 / 255f);

	// ----------------------------------------------------------------------------
	// Public Properties
	// ----------------------------------------------------------------------------

	public int FramesPerSecond { get; private set; }

	public double FrameFrequencyRounded { get; private set; }

	public int AverageFPS { get; private set; }

	public double AverageFrameFrequencyRounded { get; private set; }

	public int MinimumRecentFPS { get; private set; }

	public int MaximumRecentFPS { get; private set; }

	// ----------------------------------------------------------------------------
	// Public Fields
	// ----------------------------------------------------------------------------

	public enum FPSCounterState { Full, Minimal, Hidden };

	// ----------------------------------------------------------------------------
	// Private Fields
	// ----------------------------------------------------------------------------

	private int frameCount;
	private int lastFramesPerSecond = -1;
	private float timeOfNextMeasurement = 0.0f;
	private StringBuilder finalText;
	private char[] finalTextAsCharArray;
	private List<int> previousFPSBuffer;
	private int previousFPSBufferMaxSize;
	private int FPSAverageCalcBufferSize;
	private int FPSMinMaxCalcBufferSize;

	private string nominalColorHex;
	private string warningColorHex;
	private string criticalColorHex;


	void Start()
	{
		// Clear out any placeholder text
		targetTextMesh.text = string.Empty;

		finalText = new StringBuilder(estimatedMaxTextLength);
		finalTextAsCharArray = new char[estimatedMaxTextLength];

		// Cache strings for color hex values
		nominalColorHex = ColorUtility.ToHtmlStringRGB(nominalColor);
		warningColorHex = ColorUtility.ToHtmlStringRGB(warningColor);
		criticalColorHex = ColorUtility.ToHtmlStringRGB(criticalColor);

		// Avoid bad property values
		updateFrequency = (updateFrequency <= 0) ? 1 : updateFrequency;
		previousSecondsToAverage = (previousSecondsToAverage < 0) ? 0 : previousSecondsToAverage;
		previousSecondsToMinMax = (previousSecondsToMinMax < 0) ? 0 : previousSecondsToMinMax;

		// Buffer sizes are equal to the number of "updateFrequency's" that can fit in the designated amount of time
		FPSAverageCalcBufferSize = (int)(previousSecondsToAverage / updateFrequency);
		FPSMinMaxCalcBufferSize = (int)(previousSecondsToMinMax / updateFrequency);

		// Both calculations share the same data structure so use the bigger of the two sizes
		previousFPSBufferMaxSize = Mathf.Max(FPSAverageCalcBufferSize, FPSMinMaxCalcBufferSize);
		previousFPSBuffer = new List<int>(previousFPSBufferMaxSize);
	}

	void Update()
	{
		frameCount += 1;

		// True once every 'updateFrequency' seconds
		if (Time.realtimeSinceStartup >= timeOfNextMeasurement)
		{
			FramesPerSecond = (int)(frameCount / updateFrequency);

			// Reset for next measurement
			frameCount = 0;
			timeOfNextMeasurement = Time.realtimeSinceStartup + updateFrequency;

			// Keep the buffer at the max size
			if (previousFPSBuffer.Count >= previousFPSBufferMaxSize)
			{
				pushPopIntList(previousFPSBuffer, FramesPerSecond);
			}
			// Otherwise add to the end (increase size)
			else
			{
				previousFPSBuffer.Add(FramesPerSecond);
			}

			// Average length of a frame in milliseconds over that last 'updateFrequency' seconds rounded to two places
			FrameFrequencyRounded = Math.Round((1000.0f / FramesPerSecond), 2, MidpointRounding.AwayFromZero);

			// Reverse through the array because new values are added to the end
			AverageFPS = getAverageOfIntListOnRangeReversed(previousFPSBuffer, FPSAverageCalcBufferSize);

			// Average length of a frame in milliseconds over that last 'previousSecondsToAverageOver' rounded to two places
			AverageFrameFrequencyRounded = Math.Round((1000.0f / AverageFPS), 2, MidpointRounding.AwayFromZero);

			// Reverse through the array because new values are added to the end
			getMinMaxOfIntListOnRangeReversed(previousFPSBuffer, FPSMinMaxCalcBufferSize, out int min, out int max);
			MinimumRecentFPS = min;
			MaximumRecentFPS = max;

			switch (displayStyle)
			{
				case FPSCounterState.Full:
					{
						finalText.Clear();
						finalText.Append("<color=#");
						appendColorHexBasedOnFPS(FramesPerSecond);
						finalText.Append(">FPS:    ");
						StringBuilderUtilities.appendPotentiallyLargeInteger(finalText, FramesPerSecond);
						finalText.Append("  [");
						StringBuilderUtilities.appendRoundedDecimal(finalText, FrameFrequencyRounded);
						finalText.Append(" MS]</color>\n");

						finalText.Append("<color=#");
						appendColorHexBasedOnFPS(AverageFPS);
						finalText.Append(">AVG:   ");
						StringBuilderUtilities.appendPotentiallyLargeInteger(finalText, AverageFPS);
						finalText.Append("  [");
						StringBuilderUtilities.appendRoundedDecimal(finalText, AverageFrameFrequencyRounded);
						finalText.Append(" MS]</color>\n");

						finalText.Append("<color=#");
						appendColorHexBasedOnFPS(MinimumRecentFPS);
						finalText.Append(">MIN:    ");
						StringBuilderUtilities.appendPotentiallyLargeInteger(finalText, MinimumRecentFPS);
						finalText.Append("</color>   <color=#");
						appendColorHexBasedOnFPS(MaximumRecentFPS);
						finalText.Append(">MAX:    ");
						StringBuilderUtilities.appendPotentiallyLargeInteger(finalText, MaximumRecentFPS);
						finalText.Append("</color>");

						// Update the text mesh with a char[] to avoid allocations
						StringBuilderUtilities.copyStringBuilderToCharArray(finalText, ref finalTextAsCharArray);
						targetTextMesh.SetCharArray(finalTextAsCharArray);

						break;
					}
				case FPSCounterState.Minimal:
					{
						if (FramesPerSecond != lastFramesPerSecond)
						{
							finalText.Clear();
							finalText.Append("<color=#");
							appendColorHexBasedOnFPS(FramesPerSecond);
							finalText.Append(">FPS:  ");
							StringBuilderUtilities.appendPotentiallyLargeInteger(finalText, FramesPerSecond);
							finalText.Append("  [");
							StringBuilderUtilities.appendRoundedDecimal(finalText, FrameFrequencyRounded);
							finalText.Append(" MS]</color>\n");

							// Update the text mesh with a char[] to avoid allocations
							StringBuilderUtilities.copyStringBuilderToCharArray(finalText, ref finalTextAsCharArray);
							targetTextMesh.SetCharArray(finalTextAsCharArray);
						}

						break;
					}
				case FPSCounterState.Hidden:
					{
						break;
					}
			}

			lastFramesPerSecond = FramesPerSecond;
		}
	}

	/// <summary>
	/// Appends the cached color hex values set from the respective inspector fields.
	/// </summary>
	/// <param name="fps"></param>
	void appendColorHexBasedOnFPS(int fps)
	{
		if (fps <= criticalFPSLevel)
		{
			finalText.Append(criticalColorHex);
		}
		else if (fps <= warningFPSLevel)
		{
			finalText.Append(warningColorHex);
		}
		else
		{
			finalText.Append(nominalColorHex);
		}
	}

	/// <summary>
	/// Travels backward through a list 'iterationDistance' number of indices
	/// and outputs the average of the values in that range.
	/// </summary>
	int getAverageOfIntListOnRangeReversed(List<int> list, int iterationDistance)
	{
		int lastIndex = (list.Count - 1);
		int total = 0;

		int iterationBound = list.Count - iterationDistance;
		for (int i = lastIndex; (i >= iterationBound && i >= 0); --i)
		{
			total += list[i];
		}

		return (total / iterationDistance);
	}

	/// <summary>
	/// Travels backward through a list 'iterationDistance' number of indices
	/// and outputs the maximum and minimum values found in that range.
	/// </summary>
	void getMinMaxOfIntListOnRangeReversed(List<int> list, int iterationDistance, out int min, out int max)
	{
		int lastIndex = (list.Count - 1);
		min = list[lastIndex];
		max = list[lastIndex];

		int iterationBound = list.Count - iterationDistance;
		for (int i = (lastIndex - 1); (i >= iterationBound && i >= 0); --i)
		{
			if (list[i] < min)
			{
				min = list[i];
			}
			else if (list[i] > max)
			{
				max = list[i];
			}
		}
	}

	/// <summary>
	/// Remove the front value of the list and add 'value' to the back.
	/// Speed is O(n). Shifts the entire list one index forward.
	/// </summary>
	void pushPopIntList(List<int> list, int value)
	{
		for (int i = 1; i < list.Count; ++i)
		{
			list[i - 1] = list[i];
		}

		list.RemoveAt(list.Count - 1);
		previousFPSBuffer.Add(value);
	}
}
