using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class MonsterSpawningHandler : MonoBehaviour
{
    public GameObject row1;
    public GameObject row2;
    public Button upArrow;
    public Button downArrow;
    public TextMeshProUGUI upgradeCounterText;
    private int currentTier = 1;
    private const int maxTier = 3;
    private Button[] row1Buttons;
    private Button[] row2Buttons;
    public Sprite[] healthIcons;
    public Sprite[] strengthIcons;
    public Sprite[] speedIcons;
    public Sprite[] InvisibleIcons;
    public SpawningMonster spawningMonster;
    private CanvasGroup row2CanvasGroup;

    void Start()
    {
        row1Buttons = new Button[4] {
            row1.transform.Find("Health").GetComponent<Button>(),
            row1.transform.Find("Strength").GetComponent<Button>(),
            row1.transform.Find("Speed").GetComponent<Button>(),
            row1.transform.Find("Invisible").GetComponent<Button>()
        };

        row2Buttons = new Button[4] {
            row2.transform.Find("Health").GetComponent<Button>(),
            row2.transform.Find("Strength").GetComponent<Button>(),
            row2.transform.Find("Speed").GetComponent<Button>(),
            row2.transform.Find("Invisible").GetComponent<Button>()
        };

        row1Buttons[0].onClick.AddListener(() => OnMonsterButtonClick("Health"));
        row1Buttons[1].onClick.AddListener(() => OnMonsterButtonClick("Strength"));
        row1Buttons[2].onClick.AddListener(() => OnMonsterButtonClick("Speed"));
        row1Buttons[3].onClick.AddListener(() => OnMonsterButtonClick("Invisible"));

        row2Buttons[0].onClick.AddListener(() => OnRow2ButtonClick("Health"));
        row2Buttons[1].onClick.AddListener(() => OnRow2ButtonClick("Strength"));
        row2Buttons[2].onClick.AddListener(() => OnRow2ButtonClick("Speed"));
        row2Buttons[3].onClick.AddListener(() => OnRow2ButtonClick("Invisible"));

        upArrow.onClick.AddListener(() => ChangeTier(-1));
        downArrow.onClick.AddListener(() => ChangeTier(1));

        row2CanvasGroup = row2.GetComponent<CanvasGroup>();

        UpdateUI();
    }

    void OnMonsterButtonClick(string monsterType)
    {
        if (IsPointerOverUIElement()) return;
        StartCoroutine(FlashButton(monsterType));
        string monsterName = monsterType + "Monster" + currentTier;
        spawningMonster.SpawnMonster(monsterName);
    }

    IEnumerator FlashButton(string monsterType)
    {
        Button button = GetButtonFromMonsterType(monsterType);
        Image buttonImage = button.transform.Find("Image").GetComponent<Image>();
        buttonImage.color = new Color32(205, 205, 205, 255);
        yield return new WaitForSeconds(0.1f);
        buttonImage.color = Color.white;
    }

    Button GetButtonFromMonsterType(string monsterType)
    {
        switch (monsterType)
        {
            case "Health": return row1Buttons[0];
            case "Strength": return row1Buttons[1];
            case "Speed": return row1Buttons[2];
            case "Invisible": return row1Buttons[3];
            default: return null;
        }
    }

    void ChangeTier(int direction)
    {
        currentTier += direction;
        currentTier = Mathf.Clamp(currentTier, 1, maxTier);
        UpdateUI();
    }

    void OnRow2ButtonClick(string monsterType)
    {
        string monsterName = monsterType + "Monster" + (currentTier - 1);
        spawningMonster.SpawnMonster(monsterName);
        ChangeTier(-1); // Move down a tier after spawning the lower-tier monster
    }

    void UpdateUI()
    {
        if (currentTier == 1)
        {
            SetArrowOpacity(upArrow, 0.6f, false);
            SetArrowOpacity(downArrow, 1f, true);
            row2CanvasGroup.alpha = 0f; // Hide row2 when in tier 1
            row2CanvasGroup.interactable = false;
        }
        else if (currentTier == maxTier)
        {
            SetArrowOpacity(upArrow, 1f, true);
            SetArrowOpacity(downArrow, 0.6f, false);
            row2CanvasGroup.alpha = 1f;
            row2CanvasGroup.interactable = true;
        }
        else
        {
            SetArrowOpacity(upArrow, 1f, true);
            SetArrowOpacity(downArrow, 1f, true);
            row2CanvasGroup.alpha = 1f;
            row2CanvasGroup.interactable = true;
        }

        upgradeCounterText.text = GetRomanNumeral(currentTier);
        row1Buttons[0].transform.Find("Image").GetComponent<Image>().sprite = healthIcons[currentTier - 1];
        row1Buttons[1].transform.Find("Image").GetComponent<Image>().sprite = strengthIcons[currentTier - 1];
        row1Buttons[2].transform.Find("Image").GetComponent<Image>().sprite = speedIcons[currentTier - 1];
        row1Buttons[3].transform.Find("Image").GetComponent<Image>().sprite = InvisibleIcons[currentTier - 1];

        if (currentTier > 1)
        {
            row2.SetActive(true);
            row2Buttons[0].transform.Find("Image").GetComponent<Image>().sprite = healthIcons[currentTier - 2];
            row2Buttons[1].transform.Find("Image").GetComponent<Image>().sprite = strengthIcons[currentTier - 2];
            row2Buttons[2].transform.Find("Image").GetComponent<Image>().sprite = speedIcons[currentTier - 2];
            row2Buttons[3].transform.Find("Image").GetComponent<Image>().sprite = InvisibleIcons[currentTier - 2];
        }
        else
        {
            row2.SetActive(false);
        }
    }

    void SetArrowOpacity(Button arrow, float opacity, bool interactable)
    {
        Image arrowImage = arrow.GetComponent<Image>();
        arrowImage.color = new Color(arrowImage.color.r, arrowImage.color.g, arrowImage.color.b, opacity);
        arrow.interactable = interactable;
    }

    string GetRomanNumeral(int tier)
    {
        switch (tier)
        {
            case 1: return "I";
            case 2: return "II";
            case 3: return "III";
            default: return "";
        }
    }

    bool IsPointerOverUIElement()
    {
        return !EventSystem.current.IsPointerOverGameObject();
    }
}