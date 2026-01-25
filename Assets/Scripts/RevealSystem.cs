using UnityEngine;
using System.Collections;

public class RevealSystem : MonoBehaviour
{
    [Header("References")]
    public SecurityCameraRig cameraRig;

    [Header("Reveal Settings")]
    public float revealDuration = 3f;
    public string revealLayerName = "RevealLayer";

    bool revealing;

    public void Reveal()
    {
        if (revealing || cameraRig == null || cameraRig.CurrentCamera == null) return;
        StartCoroutine(RevealRoutine(cameraRig.CurrentCamera));
    }

    IEnumerator RevealRoutine(Camera cam)
    {
        revealing = true;

        int revealLayer = LayerMask.NameToLayer(revealLayerName);
        if (revealLayer < 0)
        {
            Debug.LogError($"Reveal layer '{revealLayerName}' not found. Create it in Project Settings > Tags and Layers.");
            revealing = false;
            yield break;
        }

        int originalMask = cam.cullingMask;
        cam.cullingMask = originalMask | (1 << revealLayer);

        // Optional: add a small “glitch” feel by briefly toggling
        // yield return new WaitForSeconds(0.08f);
        // cam.cullingMask = originalMask;
        // yield return new WaitForSeconds(0.08f);
        // cam.cullingMask = originalMask | (1 << revealLayer);

        yield return new WaitForSeconds(revealDuration);

        cam.cullingMask = originalMask;
        revealing = false;
    }
}
