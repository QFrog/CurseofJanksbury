// Copyright (c) 2019 Jakob Bjerkness. All rights reserved.

using System;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Profiling;

/// <summary>
/// Updates a TextMeshProUGUI component's text with the current memory stats every 'updateFrequency'.
/// Uses cached strings, a character array, and a StringBuilder object to avoid creating any allocations.
/// </summary>
///
public class MemoryCounter : MonoBehaviour
{
    // ----------------------------------------------------------------------------
    // Inspector Fields
    // ----------------------------------------------------------------------------

    //============== Text ==============

    [Header("Text")]

    [SerializeField]
    [Tooltip("The text mesh component to update with the memory stats.")]
    private TextMeshProUGUI targetTextMesh = null; // Can't be null in inspector

    [SerializeField]
    [Tooltip("The initial length of the character array used to update the text. " +
        "If the text grows longer than this value it will cause a new array to be allocated.")]
    private int estimatedMaxTextLength = 80;

    [Tooltip("Defines how much information is shown. All calculations will continue to happen despite this setting's value.")]
    public MemoryCounterState displayStyle = MemoryCounterState.Full;

    //============== Time ==============

    [Header("Time")]

    [SerializeField]
    [Tooltip("The frequency (in seconds) at which the memory stats are recalculated and the text updated.")]
    private float updateFrequency = 2.0f;

    // ----------------------------------------------------------------------------
    // Public Properties
    // ----------------------------------------------------------------------------

    public double TotalMemoryRounded { get; private set; }

    public double AllocatedMemoryRounded { get; private set; }

    public double MonoMemoryRounded { get; private set; }

    // ----------------------------------------------------------------------------
    // Public Fields
    // ----------------------------------------------------------------------------

    public enum MemoryCounterState { Full, Hidden };

    // ----------------------------------------------------------------------------
    // Private Fields
    // ----------------------------------------------------------------------------

    private float timeOfNextMeasurement = 0.0f;
    private StringBuilder finalText;
    private char[] finalTextAsCharArray;
    private const double SIZE_OF_A_MEGABYTE = 1048576.0;


    void Start()
    {
        // Clear out any placeholder text
        targetTextMesh.text = string.Empty;

        finalText = new StringBuilder(estimatedMaxTextLength);
        finalTextAsCharArray = new char[estimatedMaxTextLength];
    }

    void Update()
    {
        // True once every 'updateFrequency' seconds
        if (Time.realtimeSinceStartup >= timeOfNextMeasurement)
        {
            timeOfNextMeasurement = Time.realtimeSinceStartup + updateFrequency;

            // Bytes converted to megabytes and rounded to two places
            TotalMemoryRounded = Math.Round((Profiler.GetTotalReservedMemoryLong() / SIZE_OF_A_MEGABYTE), 2, MidpointRounding.AwayFromZero);
            AllocatedMemoryRounded = Math.Round((Profiler.GetTotalAllocatedMemoryLong() / SIZE_OF_A_MEGABYTE), 2, MidpointRounding.AwayFromZero);
            MonoMemoryRounded = Math.Round((GC.GetTotalMemory(false) / SIZE_OF_A_MEGABYTE), 2, MidpointRounding.AwayFromZero);

            switch (displayStyle)
            {
                case MemoryCounterState.Full:
                    {
                        finalText.Clear();
                        finalText.Append("MEM TOTAL:   ");
                        StringBuilderUtilities.appendPotentiallyLargeRoundedDecimal(finalText, TotalMemoryRounded);
                        finalText.Append("  MB\n");

                        finalText.Append("MEM ALLOC:   ");
                        StringBuilderUtilities.appendPotentiallyLargeRoundedDecimal(finalText, AllocatedMemoryRounded);
                        finalText.Append("  MB\n");

                        finalText.Append("MEM MONO:    ");
                        StringBuilderUtilities.appendPotentiallyLargeRoundedDecimal(finalText, MonoMemoryRounded);
                        finalText.Append("  MB");

                        // Update the text mesh with a char[] to avoid allocations
                        StringBuilderUtilities.copyStringBuilderToCharArray(finalText, ref finalTextAsCharArray);
                        targetTextMesh.SetCharArray(finalTextAsCharArray);

                        break;
                    }
                case MemoryCounterState.Hidden:
                    {
                        break;
                    }
            }
        }
    }
}
