using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class MonsterSpawningHandler : MonoBehaviour
{
    public GameObject row1;
    public GameObject row2;
    public Button upArrow;
    public Button downArrow;
    public TextMeshProUGUI upgradeCounterText;
    public MonsterManager monsterManager;
    public GameObject costDisplayPrefab;
    public GameObject lockPrefab;

    public int playerMoney = 5000;
    private int currentTier = 1;
    private const int maxTier = 3;
    private Button[] row1Buttons;
    private Button[] row2Buttons;
    public Sprite[] healthIcons;
    public Sprite[] strengthIcons;
    public Sprite[] speedIcons;
    public Sprite[] invisibleIcons;
    public SpawningMonster spawningMonster;
    private CanvasGroup row2CanvasGroup;

    private Dictionary<string, bool> unlockedMonsters = new Dictionary<string, bool>();
    private Dictionary<Button, bool> buttonCostDisplayed = new Dictionary<Button, bool>(); // Tracks if cost is displayed

    void Start()
    {
        InitializeUnlockStatus();

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

        upArrow.onClick.AddListener(() => ChangeTier(-1));
        downArrow.onClick.AddListener(() => ChangeTier(1));

        row2CanvasGroup = row2.GetComponent<CanvasGroup>();

        UpdateUI();
    }

    void InitializeUnlockStatus()
    {
        unlockedMonsters["HealthMonster1"] = false;
        unlockedMonsters["StrengthMonster1"] = true; // Only StrengthMonster1 is unlocked by default
        unlockedMonsters["SpeedMonster1"] = false;
        unlockedMonsters["InvisibleMonster1"] = false;

        for (int tier = 2; tier <= maxTier; tier++)
        {
            unlockedMonsters[$"HealthMonster{tier}"] = false;
            unlockedMonsters[$"StrengthMonster{tier}"] = false;
            unlockedMonsters[$"SpeedMonster{tier}"] = false;
            unlockedMonsters[$"InvisibleMonster{tier}"] = false;
        }
    }

    void AssignButtonListeners(Button[] buttons, int tier, bool isRow2)
    {
        if (tier < 1 || tier > maxTier) return;

        string[] monsterTypes = { "Health", "Strength", "Speed", "Invisible" };

        for (int i = 0; i < buttons.Length; i++)
        {
            string monsterType = monsterTypes[i];
            string monsterName = $"{monsterType}Monster{tier}";

            int capturedIndex = i;
            string capturedMonsterType = monsterType;
            string capturedMonsterName = monsterName;

            buttons[capturedIndex].onClick.RemoveAllListeners();
            buttonCostDisplayed[buttons[capturedIndex]] = false; // Reset cost display flag

            buttons[capturedIndex].onClick.AddListener(() => OnMonsterButtonClick(capturedMonsterType, tier, buttons[capturedIndex]));
            UpdateButtonVisual(buttons[capturedIndex], capturedMonsterName);

            if (isRow2 && tier == 1) buttons[capturedIndex].gameObject.SetActive(false); // Hide Row2 if tier is 1
        }
    }

    void UpdateButtonVisual(Button button, string monsterName)
    {
        // Get the correct icon array based on the monster type
        Sprite[] iconArray = GetIconArrayForMonsterType(monsterName);

        if (iconArray == null)
        {
            Debug.LogError($"No icon array found for monster: {monsterName}");
            return;
        }

        // Find the Image component in the button's child hierarchy
        Transform imageTransform = button.transform.Find("Image");

        if (imageTransform == null)
        {
            Debug.LogError($"Button {button.name} does not have an 'Image' child.");
            return;
        }

        Image imageComponent = imageTransform.GetComponent<Image>();

        if (imageComponent == null)
        {
            Debug.LogError($"Button {button.name}'s child 'Image' does not have an Image component.");
            return;
        }

        // Extract the tier number from the monster name and validate it
        int tier;
        if (int.TryParse(monsterName.Substring(monsterName.Length - 1), out tier))
        {
            tier = Mathf.Clamp(tier - 1, 0, iconArray.Length - 1); // Ensure the tier index is within bounds
            imageComponent.sprite = iconArray[tier]; // Set the correct icon for the monster's tier
        }
        else
        {
            Debug.LogError($"Invalid monster name format: {monsterName}. Expected to extract a tier.");
        }

        // Check if the monster is unlocked and update the lock icon accordingly
        if (!unlockedMonsters[monsterName])
        {
            // Add LockPrefab as a child of the Image object
            if (imageTransform.Find("Lock") == null)
            {
                GameObject lockObj = Instantiate(lockPrefab, imageTransform);
                lockObj.name = "Lock";
            }
        }
        else
        {
            // Remove the lock if the monster is unlocked
            Transform lockIcon = imageTransform.Find("Lock");
            if (lockIcon != null)
            {
                Destroy(lockIcon.gameObject);
            }
        }
    }

    Sprite[] GetIconArrayForMonsterType(string monsterName)
    {
        if (monsterName.Contains("Health"))
            return healthIcons;
        else if (monsterName.Contains("Strength"))
            return strengthIcons;
        else if (monsterName.Contains("Speed"))
            return speedIcons;
        else if (monsterName.Contains("Invisible"))
            return invisibleIcons;
        return null;
    }

    void OnMonsterButtonClick(string monsterType, int tier, Button button)
    {
        string monsterName = $"{monsterType}Monster{tier}";
        Monster monster = monsterManager.GetMonsterByName(monsterName);

        if (!unlockedMonsters[monsterName])
        {
            if (buttonCostDisplayed[button])
            {
                int unlockCost = GetUnlockCost(tier);
                if (playerMoney >= unlockCost)
                {
                    playerMoney -= unlockCost;
                    unlockedMonsters[monsterName] = true;
                    Debug.Log($"Unlocked {monsterName}. Remaining money: {playerMoney}");

                    UpdateButtonVisual(button, monsterName);
                    HideCostDisplay(button);
                    buttonCostDisplayed[button] = false;
                }
                else
                {
                    Debug.Log("Not enough money to unlock this monster!");
                }
            }
            else
            {
                ShowCostDisplay(button, monsterName);
                buttonCostDisplayed[button] = true;
            }
        }
        else
        {
            if (playerMoney >= monster.cost)
            {
                playerMoney -= monster.cost;
                spawningMonster.SpawnMonster(monsterName);
                Debug.Log($"Summoned {monsterName}. Remaining money: {playerMoney}");
            }
            else
            {
                Debug.Log("Not enough money to summon this monster!");
            }
        }
    }

    void ShowCostDisplay(Button button, string monsterName)
    {
        if (!unlockedMonsters[monsterName])
        {
            int unlockCost = GetUnlockCost(currentTier);
            GameObject costDisplay = Instantiate(costDisplayPrefab, button.transform.Find("Image"));
            costDisplay.name = "CostDisplay";
            TextMeshProUGUI costText = costDisplay.GetComponentInChildren<TextMeshProUGUI>();
            costText.text = $"{unlockCost}";
        }
        else
        {
            Monster monster = monsterManager.GetMonsterByName(monsterName);
            GameObject costDisplay = Instantiate(costDisplayPrefab, button.transform.Find("Image"));
            costDisplay.name = "CostDisplay";
            TextMeshProUGUI costText = costDisplay.GetComponentInChildren<TextMeshProUGUI>();
            costText.text = $"{monster.cost}";
        }
    }

    void HideCostDisplay(Button button)
    {
        Transform costDisplay = button.transform.Find("Image/CostDisplay");
        if (costDisplay != null)
        {
            Destroy(costDisplay.gameObject);
        }
    }

    void ChangeTier(int direction)
    {
        currentTier += direction;
        currentTier = Mathf.Clamp(currentTier, 1, maxTier);
        UpdateUI();
    }

    void UpdateUI()
    {
        upgradeCounterText.text = $"Tier {currentTier}";

        if (currentTier == 1)
        {
            SetArrowOpacity(upArrow, 0.6f, false);
            SetArrowOpacity(downArrow, 1f, true);
            row2CanvasGroup.alpha = 0f;
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

        AssignButtonListeners(row1Buttons, currentTier, false);
        AssignButtonListeners(row2Buttons, currentTier - 1, true);
    }

    void SetArrowOpacity(Button arrow, float alpha, bool interactable)
    {
        CanvasGroup canvasGroup = arrow.GetComponent<CanvasGroup>();
        if (canvasGroup != null)
        {
            canvasGroup.alpha = alpha;
            arrow.interactable = interactable;
        }
    }

    int GetUnlockCost(int tier)
    {
        switch (tier)
        {
            case 1: return 150;
            case 2: return 550;
            case 3: return 1500;
            default: return 0;
        }
    }
}