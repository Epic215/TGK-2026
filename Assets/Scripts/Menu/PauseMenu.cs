using UnityEngine;
using UnityEngine.SceneManagement;
using Hexfire.UI;

public class PauseMenu : MonoBehaviour
{
    public GameObject pausePanel;

    void Update()
    {
        if (GameOverMenu.Instance != null && GameOverMenu.Instance.IsOpen)
            return;

        if (UnityEngine.InputSystem.Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            bool paused = !pausePanel.activeSelf;
            pausePanel.SetActive(paused);
            Time.timeScale = paused ? 0f : 1f;
        }
    }

    public void Resume()
    {
        pausePanel.SetActive(false);
        Time.timeScale = 1f;
    }

    public void ExitToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Menu");
    }
}