// UIConfig.cs
using UnityEngine;

[CreateAssetMenu(menuName = "UI/UI Config", fileName = "UIConfig")]
public sealed class UIConfig : ScriptableObject
{
    [Header("Motion")]
    [Range(0.05f, 1f)] public float panelDuration = 0.22f;
    [Range(0.85f, 1f)] public float panelHiddenScale = 0.97f;

    [Tooltip("If enabled, UI animations are shortened/removed.")]
    public bool reducedMotion = false;

    [Header("Button FX")]
    [Range(1f, 1.15f)] public float hoverScale = 1.05f;
    [Range(0.85f, 1f)] public float pressedScale = 0.98f;
    [Range(1f, 40f)] public float scaleLerpSpeed = 18f;

    [Header("Loading Overlay")]
    [Range(0.05f, 1f)] public float loadingFadeDuration = 0.2f;
}