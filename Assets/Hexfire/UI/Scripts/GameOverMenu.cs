using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Hexfire.UI
{
  [DisallowMultipleComponent]
  public class GameOverMenu : MonoBehaviour
  {
    public static GameOverMenu Instance { get; private set; }

    public GameObject gameOverPanel;
    public TextMeshProUGUI titleText;
    public GameObject frameObject;
    public string menuSceneName = "Menu";

    static bool endingScene;

    public bool IsOpen => gameOverPanel != null && gameOverPanel.activeSelf;

    void Awake()
    {
      endingScene = false;

      if (Instance != null && Instance != this)
      {
        Destroy(this);
        return;
      }

      Instance = this;

      if (gameOverPanel != null)
        gameOverPanel.SetActive(false);
    }

    void OnDestroy()
    {
      if (Instance == this)
        Instance = null;
    }

    public void ShowGameOver()
    {
      OpenPanel("GAME OVER");
    }

    public void ShowWin()
    {
      OpenPanel("YOU WIN");
    }

    void OpenPanel(string title)
    {
      if (gameOverPanel == null)
        return;

      if (titleText != null)
        titleText.text = title;

      if (frameObject != null)
        frameObject.SetActive(false);

      gameOverPanel.SetActive(true);
    }

    public void Retry()
    {
      endingScene = true;
      SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ExitToMenu()
    {
      endingScene = true;
      SceneManager.LoadScene(menuSceneName);
    }

    public static bool ShouldIgnoreWinTrigger => endingScene;
  }
}