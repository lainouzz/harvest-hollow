using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject pauseRoot;
    [SerializeField] private GameObject settingsRoot;
    [SerializeField] private PlayerLook playerLook;
    [SerializeField] private string mainMenuScene = "MainMenu";

    private bool isPaused;

    private void Awake()
    {
        if (playerLook == null) playerLook = FindAnyObjectByType<PlayerLook>();
    }

    private void Start()
    {
        if (pauseRoot != null) pauseRoot.SetActive(false);
        if (settingsRoot != null) settingsRoot.SetActive(false);
    }

    private void Update()
    {
        if (Keyboard.current == null) return;
        if (!Keyboard.current.escapeKey.wasPressedThisFrame) return;

        // Don't hijack Esc while shopping (unless we're the one who paused).
        if (!isPaused && playerLook != null && playerLook.isInShopUI) return;

        if (isPaused && settingsRoot != null && settingsRoot.activeSelf)
            CloseSettings();
        else if (isPaused)
            Resume();
        else
            Pause();
    }

    public void Pause()
    {
        isPaused = true;
        Time.timeScale = 0f;

        if (pauseRoot != null) pauseRoot.SetActive(true);
        if (settingsRoot != null) settingsRoot.SetActive(false);

        SetGameplayInput(false);
    }

    public void Resume()
    {
        isPaused = false;
        Time.timeScale = 1f;

        if (settingsRoot != null) settingsRoot.SetActive(false);
        if (pauseRoot != null) pauseRoot.SetActive(false);

        SetGameplayInput(true);
    }

    public void OpenSettings()
    {
        if (pauseRoot != null) pauseRoot.SetActive(false);
        if (settingsRoot != null) settingsRoot.SetActive(true);
    }

    public void CloseSettings()
    {
        if (settingsRoot != null) settingsRoot.SetActive(false);
        if (pauseRoot != null) pauseRoot.SetActive(true);
    }

    public void QuitToMenu()
    {
        Time.timeScale = 1f;   // never leave the next scene frozen
        SceneManager.LoadScene(mainMenuScene);
    }

    private void SetGameplayInput(bool enabled)
    {
        if (playerLook != null) playerLook.isInShopUI = !enabled;

        Cursor.lockState = enabled ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !enabled;
    }

    private void OnDisable()
    {
        if (isPaused) Time.timeScale = 1f;
    }
}
