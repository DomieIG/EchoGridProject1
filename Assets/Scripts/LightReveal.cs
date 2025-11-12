using UnityEngine;

[RequireComponent(typeof(Collider))]
public class LightReveal : MonoBehaviour
{
    public Light spotLight;
    public string targetTag = "Revealable";
    public bool hideWhenNotLit = true;

    private void Start()
    {
        if (spotLight == null)
            spotLight = GetComponent<Light>();

        // Ensure trigger is enabled
        Collider col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag(targetTag)) return;
        if (spotLight == null) return;

        Vector3 directionToObject = other.transform.position - spotLight.transform.position;
        float distanceToObject = directionToObject.magnitude;

        // Default to visible inside trigger
        bool shouldBeVisible = true;

        // For spotlights, check if object is within cone
        if (spotLight.type == LightType.Spot)
        {
            float angle = Vector3.Angle(spotLight.transform.forward, directionToObject);
            if (angle > spotLight.spotAngle / 2f)
                shouldBeVisible = false;
        }

        // Optional: also check range for other light types
        if (distanceToObject > spotLight.range)
            shouldBeVisible = false;

        // Make sure nothing blocks it
        if (shouldBeVisible)
        {
            if (Physics.Raycast(spotLight.transform.position, directionToObject.normalized, out RaycastHit hit, distanceToObject))
            {
                if (hit.collider != other)
                    shouldBeVisible = false;
            }
        }

        // Apply result
        SetRendererVisible(other, shouldBeVisible);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(targetTag)) return;
        if (hideWhenNotLit)
            SetRendererVisible(other, false);
    }

    private void SetRendererVisible(Collider col, bool visible)
    {
        Renderer rend = col.GetComponent<Renderer>();
        if (rend != null)
            rend.enabled = visible;

        foreach (Renderer child in col.GetComponentsInChildren<Renderer>())
            child.enabled = visible;
    }
}
