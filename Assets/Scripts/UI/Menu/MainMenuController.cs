using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [Header("Scene")]
    [SerializeField] private string gameSceneName = "SampleScene";

    [Header("Panels")]
    [SerializeField] private GameObject mainMenuRoot;
    [SerializeField] private GameObject settingsRoot;

    [Header("Continue")]
    [SerializeField] private Button continueButton;

    [Header("Fade")]
    [SerializeField] private CanvasGroup fadeGroup;
    [SerializeField] private float fadeDuration = 1f;

    private bool loading;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        ShowMain();

        if (continueButton != null)
            continueButton.interactable = SaveSystem.HasSave;

        if (fadeGroup != null)
        {
            fadeGroup.alpha = 1f;
            StartCoroutine(Fade(1f, 0f));
        }
    }

    // The game scene loads the save by itself whenever one exists, so a new game
    // just deletes it first. Note: this wipes the old farm immediately.
    public void StartNewGame()
    {
        SaveSystem.Delete();
        LoadGame();
    }

    public void Continue()
    {
        if (!SaveSystem.HasSave) return;
        LoadGame();
    }

    public void OpenSettings()
    {
        if (mainMenuRoot != null) mainMenuRoot.SetActive(false);
        if (settingsRoot != null) settingsRoot.SetActive(true);
    }

    public void CloseSettings() => ShowMain();

    public void Quit()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    private void ShowMain()
    {
        if (settingsRoot != null) settingsRoot.SetActive(false);
        if (mainMenuRoot != null) mainMenuRoot.SetActive(true);
    }

    private void LoadGame()
    {
        if (loading) return;
        loading = true;
        StartCoroutine(LoadRoutine());
    }

    private IEnumerator LoadRoutine()
    {
        yield return Fade(0f, 1f);
        SceneManager.LoadScene(gameSceneName);
    }

    private IEnumerator Fade(float from, float to)
    {
        if (fadeGroup == null) yield break;

        fadeGroup.blocksRaycasts = to > 0.5f;
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            fadeGroup.alpha = Mathf.Lerp(from, to, t / fadeDuration);
            yield return null;
        }
        fadeGroup.alpha = to;
    }
}
