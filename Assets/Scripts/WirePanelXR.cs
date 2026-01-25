using System.Collections.Generic;
using UnityEngine;

public class WirePanelXR : MonoBehaviour
{
    public enum CorrectWireMode { Fixed, RandomPerPlaythrough }

    [Header("Wire Setup (drag your wire GameObjects here in the exact order you want)")]
    [SerializeField] private List<WireInteractable> wires = new List<WireInteractable>();

    [Header("Correct Wire Mode")]
    [SerializeField] private CorrectWireMode mode = CorrectWireMode.Fixed;

    [Tooltip("Used only when Mode = Fixed. This is the index into the 'wires' list.")]
    [SerializeField] private int fixedCorrectWireIndex = 0;

    [Header("Debug / Readonly")]
    [SerializeField] private int activeCorrectWireIndex = -1;

    [Header("Systems")]
    [SerializeField] private SecurityCameraController cameraController;

    private bool[] _cut;

    private void Awake()
    {
        if (!cameraController)
            cameraController = FindFirstObjectByType<SecurityCameraController>();

        _cut = new bool[wires.Count];

        // Assign indices automatically so you never mismatch
        for (int i = 0; i < wires.Count; i++)
        {
            if (wires[i])
                wires[i].SetIndex(this, i);
        }

        // Choose the correct wire
        if (mode == CorrectWireMode.RandomPerPlaythrough)
        {
            activeCorrectWireIndex = Random.Range(0, wires.Count);
        }
        else
        {
            activeCorrectWireIndex = Mathf.Clamp(fixedCorrectWireIndex, 0, Mathf.Max(0, wires.Count - 1));
        }
    }

    public bool CutWireByIndex(int index)
    {
        if (index < 0 || index >= _cut.Length) return false;
        if (_cut[index]) return false;

        _cut[index] = true;

        if (index == activeCorrectWireIndex)
        {
            cameraController.DisableCamerasToStatic();
            return true;
        }
        else
        {
            AlarmSystem.Instance.TriggerAlarm();
            return true;
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (wires == null) return;
        fixedCorrectWireIndex = Mathf.Clamp(fixedCorrectWireIndex, 0, Mathf.Max(0, wires.Count - 1));
    }
#endif
}
