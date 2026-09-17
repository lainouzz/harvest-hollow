using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Drives the main menu buttons and swaps between the main panel and the settings panel.
public class MainMenuController : MonoBehaviour
{
    [Header("Scene")]
    [SerializeField] private string gameSceneName = "SampleScene";

    [Header("Panels")]
    [SerializeField] private GameObject mainMenuRoot;   // "MainMenu" object (the 4 buttons)
    [SerializeField] private GameObject settingsRoot;   // "Settings" object

    [Header("Continue")]
    [SerializeField] private Button continueButton;     // greyed out until a save exists

    [Header("Fade")]
    [SerializeField] private CanvasGroup fadeGroup;     // full-screen black overlay
    [SerializeField] private float fadeDuration = 1f;

    // Set this key when you add saving; Continue enables itself when it exists.
    private const string SaveKey = "HasSave";

    private bool loading;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        ShowMain();

        if (continueButton != null)
            continueButton.interactable = PlayerPrefs.HasKey(SaveKey);

        if (fadeGroup != null)
        {
            fadeGroup.alpha = 1f;
            StartCoroutine(Fade(1f, 0f));
        }
    }

    public void StartNewGame()
    {
        // A real "new game" would clear the save here first.
        LoadGame();
    }

    public void Continue()
    {
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
