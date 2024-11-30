using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class EscapeMenuPanel : MonoBehaviour
{
    [Header("UI References")]
    public GameObject escapeMenuPanel;
    public Canvas mainGameCanvas;
    public CanvasGroup gameplayUI;
    public Button exitButton;
    public Button resumeButton;

    private bool isEscapeMenuOpen = false;

    private void Start()
    {
        Debug.Log("Escape menu panel start");

        exitButton.onClick.AddListener(ExitGame);
        resumeButton.onClick.AddListener(ToggleEscapeMenu);
        
        // Initial state
        SetEscapeMenuVisibility(false);
    }

    protected void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleEscapeMenu();
        }
    }

    private void SetEscapeMenuVisibility(bool visible)
    {
        escapeMenuPanel.SetActive(visible);
        
        // Handle gameplay UI interaction
        if (gameplayUI != null)
        {
            gameplayUI.interactable = !visible;
            gameplayUI.blocksRaycasts = !visible;
        }
        
        // Handle main canvas interaction
        if (mainGameCanvas != null)
        {
            CanvasGroup mainCanvasGroup = mainGameCanvas.GetComponent<CanvasGroup>();
            if (mainCanvasGroup == null)
            {
                mainCanvasGroup = mainGameCanvas.gameObject.AddComponent<CanvasGroup>();
            }
            mainCanvasGroup.interactable = !visible;
            mainCanvasGroup.blocksRaycasts = !visible;
        }        
    }

    public void ToggleEscapeMenu()
    {
        isEscapeMenuOpen = !isEscapeMenuOpen;
        SetEscapeMenuVisibility(isEscapeMenuOpen);
        FindObjectOfType<PauseManager>()?.TogglePause();
    }

    public void ExitGame()
    {
        Debug.Log("Exit game");
        SceneManager.LoadScene("IntroScreen");
    }
}