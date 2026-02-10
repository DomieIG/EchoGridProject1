// WirePanelXR.cs
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class WirePanelXR : MonoBehaviour
{
    public enum CorrectWireMode { Fixed, RandomPerPlaythrough }

    [Header("Wire Setup (drag wire interactables here in the exact order you want)")]
    [SerializeField] private List<WireInteractable> wires = new List<WireInteractable>();

    [Header("Correct Wire Mode")]
    [SerializeField] private CorrectWireMode mode = CorrectWireMode.Fixed;

    [Tooltip("Used only when Mode = Fixed. This is the index into the 'wires' list.")]
    [SerializeField] private int fixedCorrectWireIndex = 0;

    [Header("Systems")]
    [SerializeField] private SecurityCameraController cameraController;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = true;
    [SerializeField] private string debugTag = "[WIRE_PANEL]";
    [SerializeField] private int activeCorrectWireIndex = -1;

    private bool[] _cut;

    private void Awake()
    {
        if (!cameraController)
            cameraController = FindFirstObjectByType<SecurityCameraController>();

        int count = wires != null ? wires.Count : 0;
        _cut = new bool[count];

        for (int i = 0; i < count; i++)
        {
            if (wires[i])
                wires[i].Configure(this, i);
        }

        ChooseCorrectWireIndex();
        ValidateSetup();
    }

    private void ChooseCorrectWireIndex()
    {
        int count = wires != null ? wires.Count : 0;
        if (count <= 0)
        {
            activeCorrectWireIndex = -1;
            return;
        }

        activeCorrectWireIndex = (mode == CorrectWireMode.RandomPerPlaythrough)
            ? Random.Range(0, count)
            : Mathf.Clamp(fixedCorrectWireIndex, 0, count - 1);

        Log($"Correct wire chosen: index={activeCorrectWireIndex} (mode={mode})");
    }

    public bool CutWireByIndex(int index)
    {
        if (_cut == null || _cut.Length == 0) return false;
        if (index < 0 || index >= _cut.Length) return false;
        if (_cut[index]) return false;

        _cut[index] = true;

        // --- Correct wire: disable the whole security response (timer + alarm), then disable cameras ---
        if (index == activeCorrectWireIndex)
        {
            var alarm = AlarmSystem.Instance;
            if (alarm != null)
            {
                // Stop alarm no matter what state it’s in
                alarm.StopAlarm("Correct wire cut");

                // Stop the police ETA/timer by resetting escalation to a clean state
                // (UI that hides when alarm inactive will disappear immediately; ETA will reset as well)
                alarm.ResetPoliceEscalation("Correct wire cut");
            }
            else
            {
                Log("Correct wire cut but AlarmSystem.Instance is missing.");
            }

            if (cameraController)
                cameraController.DisableCamerasToStatic();
            else
                Log("Correct wire cut but SecurityCameraController is missing.");

            Log("Correct wire cut -> Security disabled (alarm stopped + police timer reset + cameras static).");
            return true;
        }

        // --- Wrong wire: trigger alarm ---
        if (AlarmSystem.Instance != null)
            AlarmSystem.Instance.TriggerAlarm("Wrong wire cut");
        else
            Log("Wrong wire cut but AlarmSystem.Instance is missing.");

        return true;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (wires == null) return;

        if (fixedCorrectWireIndex < 0) fixedCorrectWireIndex = 0;
        if (wires.Count > 0)
            fixedCorrectWireIndex = Mathf.Clamp(fixedCorrectWireIndex, 0, wires.Count - 1);
    }
#endif

    private void ValidateSetup()
    {
        if (wires == null || wires.Count == 0)
            Log("No wires assigned. Panel will not function.");

        if (cameraController == null)
            Log("SecurityCameraController not assigned/found. Correct wire will not disable cameras.");

        if (activeCorrectWireIndex >= 0 && activeCorrectWireIndex < (wires != null ? wires.Count : 0))
            return;

        if (wires != null && wires.Count > 0)
            Log($"activeCorrectWireIndex out of range: {activeCorrectWireIndex}");
    }

    private void Log(string msg)
    {
        if (!debugLogs) return;
        Debug.Log($"{debugTag} {msg}", this);
    }
}
