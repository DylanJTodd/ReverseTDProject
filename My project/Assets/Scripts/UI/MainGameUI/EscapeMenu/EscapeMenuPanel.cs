using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class EscapeMenuPanel : MonoBehaviour
{
    [Header("UI References")]
    public GameObject escapeMenuPanel;
    public Canvas mainGameCanvas;
    public CanvasGroup gameplayUI;
    public Button exitButton;
    public Button resumeButton;
    public Color hoverColor = Color.yellow;
    private Color normalColor;

    private bool isEscapeMenuOpen = false;

    private void Start()
    {
        Debug.Log("Escape menu panel start");

        exitButton.onClick.AddListener(ExitGame);
        resumeButton.onClick.AddListener(ToggleEscapeMenu);
        AddHoverEffects(exitButton);
        AddHoverEffects(resumeButton);
        
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

    private void AddHoverEffects(Button button)
    {
        var eventTrigger = button.gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();

        var entry1 = new UnityEngine.EventSystems.EventTrigger.Entry();
        entry1.eventID = UnityEngine.EventSystems.EventTriggerType.PointerEnter;
        entry1.callback.AddListener((eventData) => { OnPointerEnter(button); });
        eventTrigger.triggers.Add(entry1);

        var entry2 = new UnityEngine.EventSystems.EventTrigger.Entry();
        entry2.eventID = UnityEngine.EventSystems.EventTriggerType.PointerExit;
        entry2.callback.AddListener((eventData) => { OnPointerExit(button); });
        eventTrigger.triggers.Add(entry2);
    }

    private void OnPointerEnter(Button button)
    {
        button.GetComponent<Image>().color = hoverColor;

        TMP_Text buttonText = button.GetComponentInChildren<TMP_Text>();
        if (buttonText != null)
        {
            buttonText.fontStyle = FontStyles.Bold;
        }
    }

    private void OnPointerExit(Button button)
    {
        button.GetComponent<Image>().color = normalColor;

        TMP_Text buttonText = button.GetComponentInChildren<TMP_Text>();
        if (buttonText != null)
        {
            buttonText.fontStyle = FontStyles.Normal;
        }
    }

    public void ExitGame()
    {
        Debug.Log("Exit game");
        
        // Reset all game states
        Time.timeScale = 1f;
        isEscapeMenuOpen = false;
        SetEscapeMenuVisibility(false);
        
        // Find and destroy the PauseManager
        PauseManager pauseManager = FindObjectOfType<PauseManager>();
        if (pauseManager != null)
        {
            Destroy(pauseManager.gameObject);
        }
        
        // Find and reset any other persistent managers that might affect the level select
        // For example, if you have a SelectLevel manager:
        SelectLevel selectLevel = FindObjectOfType<SelectLevel>();
        if (selectLevel != null)
        {
            Destroy(selectLevel.gameObject);
        }
        
        // Load the intro screen
        SceneManager.LoadScene("IntroScreen");
    }
}