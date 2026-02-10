// SecurityCameraController.cs
using System;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class SecurityCameraController : MonoBehaviour
{
    [Serializable]
    public class CameraUnit
    {
        [Header("Camera Rendering")]
        public Camera cam;

        [Header("Rotation Rig (the thing that rotates)")]
        public Transform rotationRig;

        [Tooltip("If rotation is driven by scripts/animators, drag them here to disable.")]
        public List<Behaviour> motionDrivers = new List<Behaviour>();

        [Header("Monitor Screen (optional)")]
        public Renderer monitorRenderer;
        [Min(0)] public int monitorMaterialIndex = 0;
        public Material liveMaterial;
        public Material staticMaterial;

        [Header("Sensor Light (optional)")]
        public Light sensorLight;

        [Header("Sensor Glow (optional emissive)")]
        public Renderer sensorGlowRenderer;
        public string emissionColorProperty = "_EmissionColor";
        public Color sensorOffEmission = Color.black;

        [HideInInspector] public Quaternion savedRotation;
        [HideInInspector] public bool rotationFrozen;

        [NonSerialized] public Material[] cachedGlowMaterials;
    }

    [Header("Camera Units")]
    [SerializeField] private List<CameraUnit> units = new List<CameraUnit>();

    [Header("Debug")]
    [SerializeField] private bool debugLogs = false;
    [SerializeField] private string debugTag = "[SEC_CAM]";

    private bool _camerasDisabledToStatic;

    public void DisableCamerasToStatic()
    {
        _camerasDisabledToStatic = true;

        for (int ui = 0; ui < units.Count; ui++)
        {
            var u = units[ui];
            if (u == null) continue;

            if (u.cam) u.cam.enabled = false;

            if (u.motionDrivers != null)
            {
                for (int i = 0; i < u.motionDrivers.Count; i++)
                {
                    var b = u.motionDrivers[i];
                    if (b) b.enabled = false;
                }
            }

            if (u.rotationRig && !u.rotationFrozen)
            {
                u.savedRotation = u.rotationRig.rotation;
                u.rotationFrozen = true;
            }

            TrySetRendererMaterial(u.monitorRenderer, u.monitorMaterialIndex, u.staticMaterial);

            if (u.sensorLight) u.sensorLight.enabled = false;

            SetGlowEmission(u, u.sensorOffEmission);
        }

        Log("DisableCamerasToStatic executed.");
    }

    public void EnableCamerasLive()
    {
        _camerasDisabledToStatic = false;

        for (int ui = 0; ui < units.Count; ui++)
        {
            var u = units[ui];
            if (u == null) continue;

            if (u.cam) u.cam.enabled = true;

            if (u.motionDrivers != null)
            {
                for (int i = 0; i < u.motionDrivers.Count; i++)
                {
                    var b = u.motionDrivers[i];
                    if (b) b.enabled = true;
                }
            }

            u.rotationFrozen = false;

            TrySetRendererMaterial(u.monitorRenderer, u.monitorMaterialIndex, u.liveMaterial);

            if (u.sensorLight) u.sensorLight.enabled = true;
        }

        Log("EnableCamerasLive executed.");
    }

    private void LateUpdate()
    {
        if (!_camerasDisabledToStatic) return;

        for (int ui = 0; ui < units.Count; ui++)
        {
            var u = units[ui];
            if (u == null) continue;

            if (u.rotationFrozen && u.rotationRig)
                u.rotationRig.rotation = u.savedRotation;
        }
    }

    private static void TrySetRendererMaterial(Renderer r, int materialIndex, Material newMat)
    {
        if (!r || !newMat) return;

        var mats = r.sharedMaterials;
        if (mats == null || mats.Length == 0) return;
        if (materialIndex < 0 || materialIndex >= mats.Length) return;

        if (mats[materialIndex] == newMat) return;

        mats[materialIndex] = newMat;
        r.sharedMaterials = mats;
    }

    private void SetGlowEmission(CameraUnit u, Color emissionColor)
    {
        if (!u.sensorGlowRenderer) return;

        if (u.cachedGlowMaterials == null || u.cachedGlowMaterials.Length == 0)
            u.cachedGlowMaterials = u.sensorGlowRenderer.materials; // instanced once

        var mats = u.cachedGlowMaterials;
        for (int i = 0; i < mats.Length; i++)
        {
            var m = mats[i];
            if (!m) continue;

            if (m.HasProperty(u.emissionColorProperty))
                m.SetColor(u.emissionColorProperty, emissionColor);
        }
    }

    private void Log(string msg)
    {
        if (!debugLogs) return;
        Debug.Log($"{debugTag} {msg}", this);
    }
}
