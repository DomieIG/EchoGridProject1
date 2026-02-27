using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class SceneLoader : MonoBehaviour
{
    [SerializeField] private UILoadingOverlay loadingOverlay;

    private void Awake()
    {
        if (!loadingOverlay) loadingOverlay = FindFirstObjectByType<UILoadingOverlay>();
    }

    public void LoadSceneAsync(string sceneName, string loadingText = "Loading...")
    {
        StartCoroutine(LoadRoutine(sceneName, loadingText));
    }

    IEnumerator LoadRoutine(string sceneName, string loadingText)
    {
        Time.timeScale = 1f;

        if (loadingOverlay) loadingOverlay.Show(loadingText);

        yield return null; // allow overlay to render

        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        op.allowSceneActivation = true;

        while (!op.isDone)
        {
            // Unity reports 0..0.9 until activation; normalize for UI
            float p = Mathf.Clamp01(op.progress / 0.9f);
            if (loadingOverlay) loadingOverlay.SetProgress01(p);
            yield return null;
        }

        if (loadingOverlay) loadingOverlay.Hide();
    }
}