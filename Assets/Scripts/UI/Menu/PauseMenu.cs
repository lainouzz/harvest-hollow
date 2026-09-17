using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

// Esc toggles an in-game pause menu: freezes time, frees the cursor, and can open
// the same settings panel. Esc also backs out of settings before closing the menu.
public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject pauseRoot;
    [SerializeField] private GameObject settingsRoot;   // optional, nested under the pause menu
    [SerializeField] private PlayerLook playerLook;     // to gate camera look while paused
    [SerializeField] private string mainMenuScene = "MainMenu";

    private bool isPaused;

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
            CloseSettings();          // step back to the pause menu first
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
        // Safety: if this object is torn down while paused, don't leave time stopped.
        if (isPaused) Time.timeScale = 1f;
    }
}
