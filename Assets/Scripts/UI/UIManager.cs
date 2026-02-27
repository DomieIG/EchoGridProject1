using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum MenuState { MainMenu, Lobby, Settings }

public sealed class UIManager : MonoBehaviour
{
    [Header("Config (Optional)")]
    [SerializeField] private UIConfig config;

    [Header("Panels")]
    [SerializeField] private UIPanel mainMenuPanel;
    [SerializeField] private UIPanel preMatchPanel;
    [SerializeField] private UIPanel settingsPanel;

    [Header("Default Selected (for keyboard/controller)")]
    [SerializeField] private Selectable mainMenuDefault;
    [SerializeField] private Selectable lobbyDefault;
    [SerializeField] private Selectable settingsDefault;

    [Header("Lobby UI")]
    [SerializeField] private Button startMatchButton;

    [Header("Audio (Optional)")]
    [SerializeField] private UIAudio uiAudio;

    [Header("Transition Lock")]
    [SerializeField, Min(0f)] private float inputLockExtra = 0.02f;

    public MenuState CurrentState { get; private set; } = MenuState.MainMenu;
    public bool InputLocked { get; private set; }

    Coroutine lockRoutine;

    private void Awake()
    {
        // Hard boot state with no flicker
        SetState(MenuState.MainMenu, instant: true);
        SetStartMatchInteractable(false);
    }

    public void SetState(MenuState state, bool instant = false)
    {
        CurrentState = state;

        // Lock input briefly so clicks don't bleed between panels
        LockInput(GetTransitionDuration(instant));

        // Hide all then show target
        if (mainMenuPanel) mainMenuPanel.Hide(instant);
        if (preMatchPanel) preMatchPanel.Hide(instant);
        if (settingsPanel) settingsPanel.Hide(instant);

        switch (state)
        {
            case MenuState.MainMenu:
                if (mainMenuPanel) mainMenuPanel.Show(instant);
                SelectAfter(mainMenuDefault, instant);
                break;

            case MenuState.Lobby:
                if (preMatchPanel) preMatchPanel.Show(instant);
                SelectAfter(lobbyDefault, instant);
                break;

            case MenuState.Settings:
                if (settingsPanel) settingsPanel.Show(instant);
                SelectAfter(settingsDefault, instant);
                break;
        }

        if (!instant && uiAudio) uiAudio.PlayPanelWhoosh();
    }

    public void SetStartMatchInteractable(bool canStart)
    {
        if (startMatchButton) startMatchButton.interactable = canStart;
    }

    public void LockInput(float seconds)
    {
        if (seconds <= 0f) return;

        InputLocked = true;
        if (lockRoutine != null) StopCoroutine(lockRoutine);
        lockRoutine = StartCoroutine(UnlockAfter(seconds + inputLockExtra));
    }

    IEnumerator UnlockAfter(float seconds)
    {
        yield return new WaitForSecondsRealtime(seconds);
        InputLocked = false;
        lockRoutine = null;
    }

    void SelectAfter(Selectable target, bool instant)
    {
        if (!target) return;

        // EventSystem might not exist in some scenes—safe guard
        if (!EventSystem.current) return;

        if (instant || (config && config.reducedMotion))
        {
            EventSystem.current.SetSelectedGameObject(target.gameObject);
        }
        else
        {
            // Wait a frame so the panel is active & interactable
            StartCoroutine(SelectNextFrame(target));
        }
    }

    IEnumerator SelectNextFrame(Selectable target)
    {
        yield return null;
        if (!EventSystem.current) yield break;
        EventSystem.current.SetSelectedGameObject(target.gameObject);
    }

    float GetTransitionDuration(bool instant)
    {
        if (instant) return 0f;
        if (config && config.reducedMotion) return 0f;
        return config ? config.panelDuration : 0.22f;
    }
}