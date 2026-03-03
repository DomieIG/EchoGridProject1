// UIManager.cs
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public sealed class UIManager : MonoBehaviour
{
    [Header("Panels (UIPanel components)")]
    [SerializeField] private UIPanel mainMenuPanel;
    [SerializeField] private UIPanel preMatchPanel;
    [SerializeField] private UIPanel settingsPanel;

    [Header("Default Selected (optional)")]
    [SerializeField] private Selectable mainMenuDefault;
    [SerializeField] private Selectable lobbyDefault;
    [SerializeField] private Selectable settingsDefault;

    [Header("Lobby UI")]
    [SerializeField] private Button startMatchButton;

    [Header("Input Lock")]
    [SerializeField, Min(0f)] private float inputLockDuration = 0.15f;

    public MenuState CurrentState { get; private set; } = MenuState.MainMenu;
    public bool InputLocked { get; private set; }

    private Coroutine inputLockRoutine;

    private void Awake()
    {
        // Hard boot state
        if (preMatchPanel) preMatchPanel.Hide(true);
        if (settingsPanel) settingsPanel.Hide(true);
        if (mainMenuPanel) mainMenuPanel.Show(true);

        CurrentState = MenuState.MainMenu;
        SetStartMatchInteractable(false);

        // Ensure selection happens after objects are enabled
        StartCoroutine(SelectNextFrame(MenuState.MainMenu));
    }

    public void SetState(MenuState state, bool instant = false)
    {
        CurrentState = state;

        if (!instant)
            LockInput(inputLockDuration);

        UIPanel target = state switch
        {
            MenuState.MainMenu => mainMenuPanel,
            MenuState.Lobby => preMatchPanel,
            MenuState.Settings => settingsPanel,
            _ => mainMenuPanel
        };

        if (!target)
        {
            Debug.LogError($"[UIManager] Missing panel reference for '{state}'.", this);
            return;
        }

        // Show target first (prevents blank screen)
        target.Show(instant);

        if (mainMenuPanel && mainMenuPanel != target) mainMenuPanel.Hide(instant);
        if (preMatchPanel && preMatchPanel != target) preMatchPanel.Hide(instant);
        if (settingsPanel && settingsPanel != target) settingsPanel.Hide(instant);

        // Selection is safer one frame later (layout + enable order)
        StartCoroutine(SelectNextFrame(state));
    }

    public void SetStartMatchInteractable(bool canStart)
    {
        if (startMatchButton)
            startMatchButton.interactable = canStart;
    }

    private void LockInput(float seconds)
    {
        if (seconds <= 0f) return;

        InputLocked = true;

        if (inputLockRoutine != null)
            StopCoroutine(inputLockRoutine);

        inputLockRoutine = StartCoroutine(UnlockAfter(seconds));
    }

    private IEnumerator UnlockAfter(float seconds)
    {
        yield return new WaitForSecondsRealtime(seconds);
        InputLocked = false;
        inputLockRoutine = null;
    }

    private IEnumerator SelectNextFrame(MenuState state)
    {
        yield return null; // wait one frame for enable/layout

        if (!EventSystem.current) yield break;

        Selectable target = state switch
        {
            MenuState.MainMenu => mainMenuDefault,
            MenuState.Lobby => lobbyDefault,
            MenuState.Settings => settingsDefault,
            _ => null
        };

        if (!target) yield break;

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(target.gameObject);
    }
}