using UnityEngine;

public sealed class GameManager : MonoBehaviour
{
    [Header("Scenes")]
    [SerializeField] private string bankSceneName = "Bank";

    [Header("Refs")]
    [SerializeField] private UIManager ui;
    [SerializeField] private SceneLoader sceneLoader;
    [SerializeField] private UIConfirmDialog confirmDialog;

    private bool inMultiplayerLobby;
    private bool isPartyLeader;

    private void Awake()
    {
        if (!ui) ui = FindAnyObjectByType<UIManager>();
        if (!sceneLoader) sceneLoader = FindAnyObjectByType<SceneLoader>();

        // Find dialog even if it's inactive in hierarchy
        if (!confirmDialog) confirmDialog = FindInactiveSafe<UIConfirmDialog>();

        if (!ui) Debug.LogError("[GameManager] UIManager not found/assigned.", this);
        if (!sceneLoader) Debug.LogWarning("[GameManager] SceneLoader not found; will load scenes directly.", this);
        if (!confirmDialog) Debug.LogWarning("[GameManager] UIConfirmDialog not found/assigned; Exit will quit immediately.", this);
    }

    public void OnNewGamePressed()
    {
        inMultiplayerLobby = false;
        isPartyLeader = false;
        LoadBank();
    }

    public void OnMultiplayerPressed()
    {
        inMultiplayerLobby = true;

        if (!ui)
        {
            Debug.LogError("[GameManager] UIManager missing, cannot open Lobby panel.");
            return;
        }

        ui.SetState(MenuState.Lobby);
        SetPartyLeader(true); // placeholder until real host logic
    }

    public void OnSettingsPressed()
    {
        if (!ui)
        {
            Debug.LogError("[GameManager] UIManager missing, cannot open Settings panel.");
            return;
        }

        ui.SetState(MenuState.Settings);
    }

    public void OnBackToMenuPressed()
    {
        if (!ui)
        {
            Debug.LogError("[GameManager] UIManager missing, cannot open Main Menu panel.");
            return;
        }

        ui.SetState(MenuState.MainMenu);
    }

    public void OnStartMatchPressed()
    {
        if (inMultiplayerLobby && !isPartyLeader)
        {
            Debug.Log("Only the party leader can start the match.");
            return;
        }

        LoadBank();
    }

    public void OnExitPressed()
    {
        if (confirmDialog)
        {
            confirmDialog.Open(
                title: "Exit Game",
                body: "Are you sure you want to quit?",
                confirm: QuitGame
            );
        }
        else
        {
            QuitGame();
        }
    }

    public void SetPartyLeader(bool leader)
    {
        isPartyLeader = leader;
        if (ui) ui.SetStartMatchInteractable(isPartyLeader);
    }

    private void LoadBank()
    {
        if (sceneLoader)
            sceneLoader.LoadSceneAsync(bankSceneName, "Loading Bank...");
        else
            UnityEngine.SceneManagement.SceneManager.LoadScene(bankSceneName);
    }

    private void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    private static T FindInactiveSafe<T>() where T : Object
    {
        // Works even if object is disabled/inactive
        var all = Resources.FindObjectsOfTypeAll<T>();
        foreach (var obj in all)
        {
            // Filter out assets/prefabs, keep scene instances
            if (obj is Component c && c.gameObject.scene.IsValid())
                return obj;
        }
        return null;
    }
}