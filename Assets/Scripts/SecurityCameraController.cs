using System.Collections.Generic;
using UnityEngine;

public class SecurityCameraController : MonoBehaviour
{
    [System.Serializable]
    public class CameraUnit
    {
        [Header("Camera Rendering")]
        public Camera cam;

        [Header("Rotation Rig (the thing that rotates)")]
        [Tooltip("Assign the Transform that actually rotates (often the camera pivot or parent).")]
        public Transform rotationRig;

        [Tooltip("If rotation is driven by scripts/animators, drag them here to disable.")]
        public List<Behaviour> motionDrivers;

        [Header("Monitor Screen (optional)")]
        public Renderer monitorRenderer;
        public int monitorMaterialIndex = 0;
        public Material liveMaterial;
        public Material staticMaterial;

        [Header("Sensor Light (optional)")]
        [Tooltip("Drag the Light component used as the red sensor light (if any).")]
        public Light sensorLight;

        [Tooltip("If the red glow is emissive on a mesh, drag that renderer here.")]
        public Renderer sensorGlowRenderer;

        [Tooltip("Name of the emissive color property. Built-in/URP often uses _EmissionColor.")]
        public string emissionColorProperty = "_EmissionColor";
        public Color sensorOffEmission = Color.black;

        [HideInInspector] public Quaternion savedRotation;
        [HideInInspector] public bool rotationFrozen;
    }

    [SerializeField] private List<CameraUnit> units = new List<CameraUnit>();

    public void DisableCamerasToStatic()
    {
        foreach (var u in units)
        {
            // 1) Stop rendering
            if (u.cam) u.cam.enabled = false;

            // 2) Disable motion drivers (scripts, Animator, etc.)
            if (u.motionDrivers != null)
            {
                foreach (var b in u.motionDrivers)
                    if (b) b.enabled = false;
            }

            // 2b) Freeze rotation rig even if something else still tries to move it
            if (u.rotationRig && !u.rotationFrozen)
            {
                u.savedRotation = u.rotationRig.rotation;
                u.rotationFrozen = true;
            }

            // 3) Static monitor screen
            if (u.monitorRenderer && u.staticMaterial)
            {
                var mats = u.monitorRenderer.sharedMaterials;
                if (u.monitorMaterialIndex >= 0 && u.monitorMaterialIndex < mats.Length)
                {
                    mats[u.monitorMaterialIndex] = u.staticMaterial;
                    u.monitorRenderer.sharedMaterials = mats;
                }
            }

            // 4) Turn off sensor light
            if (u.sensorLight) u.sensorLight.enabled = false;

            // 5) Turn off emissive glow (optional)
            if (u.sensorGlowRenderer)
            {
                var mats = u.sensorGlowRenderer.materials; // instance materials so we can change emissive
                for (int i = 0; i < mats.Length; i++)
                {
                    if (mats[i] && mats[i].HasProperty(u.emissionColorProperty))
                        mats[i].SetColor(u.emissionColorProperty, u.sensorOffEmission);
                }
            }
        }
    }

    public void EnableCamerasLive()
    {
        foreach (var u in units)
        {
            if (u.cam) u.cam.enabled = true;

            if (u.motionDrivers != null)
            {
                foreach (var b in u.motionDrivers)
                    if (b) b.enabled = true;
            }

            // Unfreeze rotation (let drivers rotate again)
            u.rotationFrozen = false;

            if (u.monitorRenderer && u.liveMaterial)
            {
                var mats = u.monitorRenderer.sharedMaterials;
                if (u.monitorMaterialIndex >= 0 && u.monitorMaterialIndex < mats.Length)
                {
                    mats[u.monitorMaterialIndex] = u.liveMaterial;
                    u.monitorRenderer.sharedMaterials = mats;
                }
            }

            if (u.sensorLight) u.sensorLight.enabled = true;

            // If you want emissive to come back on, set it here too (optional).
        }
    }

    private void LateUpdate()
    {
        // Hard-freeze rotation rig every frame after animations/scripts (LateUpdate is key).
        foreach (var u in units)
        {
            if (u.rotationFrozen && u.rotationRig)
                u.rotationRig.rotation = u.savedRotation;
        }
    }
}
