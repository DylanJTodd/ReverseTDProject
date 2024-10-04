using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Collections;

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
    public GameObject CooldownPrefab;

    public float summonCooldown = 1f;
    public int playerMoney = 5000;

    private int currentTier = 1;
    private const int maxTier = 3;
    private float lastSummonTime;
    private Button[] row1Buttons;
    private Button[] row2Buttons;
    public Sprite[] healthIcons;
    public Sprite[] strengthIcons;
    public Sprite[] speedIcons;
    public Sprite[] invisibleIcons;
    public SpawningMonster spawningMonster;
    private CanvasGroup row2CanvasGroup;

    private Dictionary<string, bool> unlockedMonsters = new Dictionary<string, bool>();
    private Dictionary<Button, bool> buttonCostDisplayed = new Dictionary<Button, bool>();

    private Dictionary<Button, Coroutine> activeCooldowns = new Dictionary<Button, Coroutine>();

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

        lastSummonTime = -summonCooldown;
        UpdateUI();
    }

    IEnumerator HandleCooldown(Button button)
    {
        Transform imageTransform = button.transform.Find("Image");

        foreach (Transform child in imageTransform)
        {
            Destroy(child.gameObject);
        }

        GameObject cooldownIndicator = Instantiate(CooldownPrefab, imageTransform);
        cooldownIndicator.name = "CooldownPrefab";
        Image circleImage = cooldownIndicator.transform.Find("Circle").GetComponent<Image>();
        circleImage.fillAmount = 0f;

        float elapsedTime = 0f;
        while (elapsedTime < summonCooldown)
        {
            elapsedTime += Time.deltaTime;
            circleImage.fillAmount = Mathf.Clamp01(elapsedTime / summonCooldown);
            yield return null;

            if (button.transform.parent == row2.transform && row1.transform.Find(button.name) != null)
            {
                Button newRow1Button = row1.transform.Find(button.name).GetComponent<Button>();
                if (newRow1Button != null && newRow1Button.transform.Find("Image").Find("CooldownPrefab") == null)
                {
                    GameObject newCooldownIndicator = Instantiate(CooldownPrefab, newRow1Button.transform.Find("Image"));
                    newCooldownIndicator.name = "CooldownPrefab";
                    newCooldownIndicator.transform.Find("Circle").GetComponent<Image>().fillAmount = circleImage.fillAmount;

                    Destroy(cooldownIndicator);
                    cooldownIndicator = newCooldownIndicator;
                }
            }
        }

        Destroy(cooldownIndicator);

        if (IsPointerOverButton(button))
        {
            string monsterName = button.name;
            ShowCostDisplay(button, monsterName);
        }

        activeCooldowns.Remove(button);
    }

    bool IsPointerOverButton(Button button)
    {
        PointerEventData pointerEventData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerEventData, results);

        foreach (var result in results)
        {
            if (result.gameObject == button.gameObject)
                return true;
        }

        return false;
    }

    void InitializeUnlockStatus()
    {
        unlockedMonsters["HealthMonster1"] = false;
        unlockedMonsters["StrengthMonster1"] = true;
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
            buttonCostDisplayed[buttons[capturedIndex]] = false;

            buttons[capturedIndex].onClick.AddListener(() => OnMonsterButtonClick(capturedMonsterType, tier, buttons[capturedIndex], isRow2));

            AddHoverListeners(buttons[capturedIndex], capturedMonsterName);

            UpdateButtonVisual(buttons[capturedIndex], capturedMonsterName);
        }
    }

    void AddHoverListeners(Button button, string monsterName)
    {
        EventTrigger trigger = button.GetComponent<EventTrigger>();
        if (trigger != null)
        {
            Destroy(trigger);
        }

        trigger = button.gameObject.AddComponent<EventTrigger>();

        EventTrigger.Entry pointerEnter = new EventTrigger.Entry();
        pointerEnter.eventID = EventTriggerType.PointerEnter;
        pointerEnter.callback.AddListener((eventData) => ShowCostDisplay(button, monsterName));
        trigger.triggers.Add(pointerEnter);

        EventTrigger.Entry pointerExit = new EventTrigger.Entry();
        pointerExit.eventID = EventTriggerType.PointerExit;
        pointerExit.callback.AddListener((eventData) => HideCostDisplay(button, monsterName));
        trigger.triggers.Add(pointerExit);
    }

    void ShowCostDisplay(Button button, string monsterName)
    {
        if (button.transform.Find("Image").Find("CooldownPrefab") != null)
        {
            return;
        }

        if (unlockedMonsters[monsterName])
        {
            Monster monster = monsterManager.GetMonsterByName(monsterName);
            GameObject costDisplay = Instantiate(costDisplayPrefab, button.transform.Find("Image"));
            costDisplay.name = "CostDisplay";
            TextMeshProUGUI costText = costDisplay.GetComponentInChildren<TextMeshProUGUI>();
            costText.text = $"{monster.cost}";

            CanvasGroup cg = costDisplay.GetComponent<CanvasGroup>();
            if (cg != null)
            {
                cg.alpha = 1f;
            }
        }
    }

    void ShowUnlockCostDisplay(Button button, string monsterName)
    {
        int unlockCost = GetUnlockCost(currentTier);

        GameObject costDisplay = Instantiate(costDisplayPrefab, button.transform.Find("Image"));
        costDisplay.name = "CostDisplay";
        TextMeshProUGUI costText = costDisplay.GetComponentInChildren<TextMeshProUGUI>();
        costText.text = $"{unlockCost}";

        CanvasGroup cg = costDisplay.GetComponent<CanvasGroup>();
        if (cg != null)
        {
            cg.alpha = 1f;
        }
    }

    void HideCostDisplay(Button button, string monsterName)
    {
        Transform costDisplay = button.transform.Find("Image/CostDisplay");
        if (costDisplay != null)
        {
            Destroy(costDisplay.gameObject);
        }

        if (!unlockedMonsters[monsterName] && buttonCostDisplayed[button])
        {
            Transform imageTransform = button.transform.Find("Image");
            if (imageTransform != null && imageTransform.Find("Lock") == null)
            {
                GameObject lockObj = Instantiate(lockPrefab, imageTransform);
                lockObj.name = "Lock";
            }
            buttonCostDisplayed[button] = false;
        }
    }

    void OnMonsterButtonClick(string monsterType, int tier, Button button, bool isRow2)
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
                    UpdateButtonVisual(button, monsterName);
                    HideCostDisplay(button, monsterName);
                    buttonCostDisplayed[button] = false;
                }
            }
            else
            {
                ShowUnlockCostDisplay(button, monsterName);
                buttonCostDisplayed[button] = true;
            }
        }
        else
        {
            if (Time.time - lastSummonTime < summonCooldown) return;
            lastSummonTime = Time.time;

            if (isRow2)
            {
                // Store the monster info and switch the tier
                string storedMonsterType = monsterType;
                int storedTier = tier;

                // Switch the tier down, so row1 gets updated
                ChangeTier(-1);

                // Now apply cooldown to the corresponding row1 button
                Button correspondingButton = FindButtonByMonsterType(row1Buttons, storedMonsterType, storedTier);
                if (correspondingButton != null)
                {
                    ApplyCooldown(correspondingButton);
                }
            }
            else
            {
                // Directly apply cooldown to the clicked row1 button
                if (playerMoney >= monster.cost)
                {
                    playerMoney -= monster.cost;
                    spawningMonster.SpawnMonster(monsterName);

                    ApplyCooldown(button);
                }
            }
        }
    }

    void ApplyCooldown(Button button)
    {
        // Ensure no active cooldown exists
        if (!activeCooldowns.ContainsKey(button))
        {
            Coroutine cooldownCoroutine = StartCoroutine(HandleCooldown(button));
            activeCooldowns[button] = cooldownCoroutine;
        }
    }

    Button FindButtonByMonsterType(Button[] buttons, string monsterType, int tier)
    {
        foreach (Button btn in buttons)
        {
            string btnMonsterName = $"{monsterType}Monster{tier}";
            if (btn.name == monsterType || btnMonsterName == btn.name)
            {
                return btn;
            }
        }
        return null;
    }

    void ChangeTier(int direction)
    {
        currentTier += direction;
        currentTier = Mathf.Clamp(currentTier, 1, maxTier);
        UpdateUI();
    }

    void UpdateButtonVisual(Button button, string monsterName)
    {
        Sprite[] iconArray = GetIconArrayForMonsterType(monsterName);

        if (iconArray == null)
        {
            Debug.LogError($"No icon array found for monster: {monsterName}");
            return;
        }

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

        int tier;
        if (int.TryParse(monsterName.Substring(monsterName.Length - 1), out tier))
        {
            tier = Mathf.Clamp(tier - 1, 0, iconArray.Length - 1);
            imageComponent.sprite = iconArray[tier];
        }
        else
        {
            Debug.LogError($"Invalid monster name format: {monsterName}. Expected to extract a tier.");
        }

        if (!unlockedMonsters[monsterName])
        {
            if (imageTransform.Find("Lock") == null)
            {
                GameObject lockObj = Instantiate(lockPrefab, imageTransform);
                lockObj.name = "Lock";
            }
        }
        else
        {
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

    void UpdateUI()
    {
        upgradeCounterText.text = $"{currentTier}";

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
