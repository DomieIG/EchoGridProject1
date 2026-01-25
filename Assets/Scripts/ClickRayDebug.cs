using UnityEngine;

public class ClickRayDebug : MonoBehaviour
{
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("Mouse down detected.");

            var cam = Camera.main;
            if (cam == null)
            {
                Debug.LogError("Camera.main is null. Is any enabled camera tagged MainCamera?");
                return;
            }

            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 100f))
            {
                Debug.Log($"Hit: {hit.collider.name} (Layer: {LayerMask.LayerToName(hit.collider.gameObject.layer)})");
            }
            else
            {
                Debug.Log("Raycast hit nothing.");
            }
        }
    }
}
