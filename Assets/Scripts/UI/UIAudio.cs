// UIAudio.cs
using UnityEngine;

[DisallowMultipleComponent]
public sealed class UIAudio : MonoBehaviour
{
    [SerializeField] private AudioSource source;
    [SerializeField] private AudioClip hoverClip;
    [SerializeField] private AudioClip clickClip;
    [SerializeField] private AudioClip panelWhooshClip;

    [Header("Polish")]
    [SerializeField] private Vector2 pitchRange = new Vector2(0.98f, 1.02f);

    private void Reset()
    {
        source = GetComponent<AudioSource>();
    }

    private void Awake()
    {
        if (!source) source = GetComponent<AudioSource>();
    }

    public void PlayHover() => Play(hoverClip);
    public void PlayClick() => Play(clickClip);
    public void PlayPanelWhoosh() => Play(panelWhooshClip);

    private void Play(AudioClip clip)
    {
        if (!source || !clip) return;

        float originalPitch = source.pitch;
        source.pitch = Random.Range(pitchRange.x, pitchRange.y);
        source.PlayOneShot(clip);
        source.pitch = originalPitch;
    }
}