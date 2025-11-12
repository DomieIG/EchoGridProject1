using UnityEngine;

[ExecuteAlways]
public class UVLightController : MonoBehaviour
{
    [SerializeField] private float range = 5f;
    [SerializeField] private Color lightColor = new Color(0.6f, 0.2f, 1f, 1f);

    void Update()
    {
        Shader.SetGlobalVector("_UVLightPos", transform.position);
        Shader.SetGlobalFloat("_UVLightRange", range);
        Shader.SetGlobalColor("_UVLightColor", lightColor);
    }
}
