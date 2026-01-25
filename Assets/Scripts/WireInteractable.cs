using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRSimpleInteractable))]
public class WireInteractable : MonoBehaviour
{
    [SerializeField] private WirePanelXR panel;
    [SerializeField] private int wireIndex;
    [SerializeField] private bool disableAfterCut = true;

    private XRSimpleInteractable interactable;

    private void Awake()
    {
        interactable = GetComponent<XRSimpleInteractable>();
        if (!panel) panel = GetComponentInParent<WirePanelXR>();
    }

    private void OnEnable()
    {
        interactable.activated.AddListener(OnActivated);
    }

    private void OnDisable()
    {
        interactable.activated.RemoveListener(OnActivated);
    }

    private void OnActivated(ActivateEventArgs args)
    {
        TryCut();
    }

    public void OnToolCut()
    {
        TryCut();
    }

    private void TryCut()
    {
        if (!panel) return;

        bool didCut = panel.CutWireByIndex(wireIndex);
        if (didCut && disableAfterCut)
        {
            interactable.enabled = false;

            var col = GetComponent<Collider>();
            if (col) col.enabled = false;
        }
    }

    public void SetIndex(WirePanelXR owner, int index)
    {
        panel = owner;
        wireIndex = index;
    }
}
