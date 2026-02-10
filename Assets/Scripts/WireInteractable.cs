// WireInteractable.cs
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[DisallowMultipleComponent]
[RequireComponent(typeof(XRSimpleInteractable))]
public class WireInteractable : MonoBehaviour
{
    [Header("Wiring")]
    [SerializeField] private WirePanelXR panel;
    [SerializeField] private int wireIndex;

    [Header("Behavior")]
    [SerializeField] private bool disableAfterCut = true;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = false;
    [SerializeField] private string debugTag = "[WIRE]";

    private XRSimpleInteractable _interactable;
    private Collider _collider;
    private bool _cut;

    private void Awake()
    {
        _interactable = GetComponent<XRSimpleInteractable>();
        _collider = GetComponent<Collider>();

        if (!panel)
            panel = GetComponentInParent<WirePanelXR>();
    }

    private void OnEnable()
    {
        if (_interactable != null)
            _interactable.activated.AddListener(OnActivated);
    }

    private void OnDisable()
    {
        if (_interactable != null)
            _interactable.activated.RemoveListener(OnActivated);
    }

    private void OnActivated(ActivateEventArgs _)
    {
        TryCut();
    }

    public void OnToolCut() => TryCut();

    private void TryCut()
    {
        if (_cut) return;

        if (!panel)
        {
            Log("TryCut blocked: no WirePanelXR assigned/found.");
            return;
        }

        bool didCut = panel.CutWireByIndex(wireIndex);
        if (!didCut) return;

        _cut = true;

        if (disableAfterCut)
        {
            if (_interactable) _interactable.enabled = false;
            if (_collider) _collider.enabled = false;
        }

        Log($"Wire cut processed. index={wireIndex}, disableAfterCut={disableAfterCut}");
    }

    public void Configure(WirePanelXR owner, int index)
    {
        panel = owner;
        wireIndex = index;
    }

    private void Log(string msg)
    {
        if (!debugLogs) return;
        Debug.Log($"{debugTag} {msg}", this);
    }
}