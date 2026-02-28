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
    private bool isLoadingScene;

    private MenuState stateBeforeExitPrompt = MenuState.MainMenu;

    private void Awake()
    {
        if (!ui) ui = FindAnyObjectByType<UIManager>();
        if (!sceneLoader) sceneLoader = FindAnyObjectByType<SceneLoader>();
        if (!confirmDialog) confirmDialog = FindInactiveSafe<UIConfirmDialog>();

        if (!ui) Debug.LogError("[GameManager] UIManager not found/assigned.", this);
        if (!confirmDialog) Debug.LogWarning("[GameManager] UIConfirmDialog not found. Exit will quit immediately.", this);
    }

    public void OnNewGamePressed()
    {
        if (isLoadingScene) return;
        if (ui && ui.InputLocked) return;

        inMultiplayerLobby = false;
        isPartyLeader = false;

        LoadBank();
    }

    public void OnMultiplayerPressed()
    {
        if (isLoadingScene) return;
        if (ui && ui.InputLocked) return;

        inMultiplayerLobby = true;

        if (!ui) return;
        ui.SetState(MenuState.Lobby);

        // placeholder until real host/party logic
        SetPartyLeader(true);
    }

    public void OnSettingsPressed()
    {
        if (isLoadingScene) return;
        if (ui && ui.InputLocked) return;

        if (!ui) return;
        ui.SetState(MenuState.Settings);
    }

    public void OnBackToMenuPressed()
    {
        if (isLoadingScene) return;
        if (ui && ui.InputLocked) return;

        if (!ui) return;
        ui.SetState(MenuState.MainMenu);
    }

    public void OnStartMatchPressed()
    {
        if (isLoadingScene) return;
        if (ui && ui.InputLocked) return;

        if (inMultiplayerLobby && !isPartyLeader)
        {
            Debug.Log("Only the party leader can start the match.");
            return;
        }

        LoadBank();
    }

    public void OnExitPressed()
    {
        if (ui && ui.InputLocked) return;

        // Remember what the user was viewing
        if (ui) stateBeforeExitPrompt = ui.CurrentState;

        if (confirmDialog)
        {
            confirmDialog.Open(
                title: "Exit Game",
                body: "Are you sure you want to quit?",
                confirm: QuitGame,
                cancel: RestoreUIAfterExitPrompt
            );
        }
        else
        {
            QuitGame();
        }
    }

    private void RestoreUIAfterExitPrompt()
    {
        // Return to exactly what it was
        if (ui) ui.SetState(stateBeforeExitPrompt);
    }

    public void SetPartyLeader(bool leader)
    {
        isPartyLeader = leader;
        if (ui) ui.SetStartMatchInteractable(isPartyLeader);
    }

    private void LoadBank()
    {
        if (isLoadingScene) return;
        isLoadingScene = true;

        if (sceneLoader)
        {
            sceneLoader.LoadSceneAsync(bankSceneName, "Loading Bank...", () => isLoadingScene = false);
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(bankSceneName);
            isLoadingScene = false;
        }
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
        var all = Resources.FindObjectsOfTypeAll<T>();
        foreach (var obj in all)
        {
            if (obj is Component c && c.gameObject.scene.IsValid())
                return obj;
        }
        return null;
    }
}