using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRGrabInteractable))]
public class XRGrabState : MonoBehaviour
{
    public bool IsHeld { get; private set; }

    private XRGrabInteractable _grab;

    private void Awake()
    {
        _grab = GetComponent<XRGrabInteractable>();
    }

    private void OnEnable()
    {
        _grab.selectEntered.AddListener(_ => IsHeld = true);
        _grab.selectExited.AddListener(_ => IsHeld = false);
    }

    private void OnDisable()
    {
        _grab.selectEntered.RemoveAllListeners();
        _grab.selectExited.RemoveAllListeners();
    }
}
