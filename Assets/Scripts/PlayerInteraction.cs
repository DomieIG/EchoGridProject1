using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Interaction")]
    [SerializeField] private Transform rayOrigin;
    [SerializeField] private float interactDistance = 3f;
    [SerializeField] private LayerMask interactionLayers = ~0;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI promptText;

    private IInteractable currentInteractable;

    private void Awake()
    {
        if (rayOrigin == null && Camera.main != null)
            rayOrigin = Camera.main.transform;
    }

    private void Start()
    {
        SetPrompt("");
    }

    private void Update()
    {
        if (rayOrigin == null)
        {
            Debug.LogWarning("PlayerInteraction: No rayOrigin assigned.");
            SetPrompt("");
            return;
        }

        Debug.DrawRay(rayOrigin.position, rayOrigin.forward * interactDistance, Color.green);

        Ray ray = new Ray(rayOrigin.position, rayOrigin.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, interactionLayers, QueryTriggerInteraction.Collide))
        {
            Debug.Log("Ray hit: " + hit.collider.name);

            IInteractable interactable = FindInteractable(hit.collider);

            if (interactable != null)
            {
                Debug.Log("Interactable found: " + interactable.GetPrompt());

                currentInteractable = interactable;
                SetPrompt("[E] " + currentInteractable.GetPrompt());

                if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
                {
                    currentInteractable.Interact();
                }

                return;
            }
            else
            {
                Debug.Log("Hit collider, but no IInteractable found on it or its parents.");
            }
        }
        else
        {
            Debug.Log("Ray hit nothing.");
        }

        currentInteractable = null;
        SetPrompt("");
    }

    private IInteractable FindInteractable(Collider col)
    {
        MonoBehaviour[] behaviours = col.GetComponentsInParent<MonoBehaviour>();

        foreach (MonoBehaviour behaviour in behaviours)
        {
            if (behaviour is IInteractable interactable)
                return interactable;
        }

        return null;
    }

    private void SetPrompt(string message)
    {
        if (promptText == null)
        {
            Debug.LogWarning("PlayerInteraction: Prompt Text is not assigned.");
            return;
        }

        promptText.text = message;
    }
}