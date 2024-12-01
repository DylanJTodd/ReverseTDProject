using System;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MonsterDisplayHandler : MonoBehaviour
{
    public CanvasGroup monsterDisplayCanvasGroup;
    public Image monsterFeedImage;
    public Sprite[] healthIcons;
    public Sprite[] strengthIcons;
    public Sprite[] speedIcons;
    public Sprite[] invisibleIcons;
    public MonsterManager monsterManager;
    public GameObject text;
    public Image healthBar;
    public MoneyHandler moneyHandler;

    private Monster focusedMonster;
    private bool isDisplayActive;

    private void Start()
    {
        HideMonsterDisplay();
    }

    private void Update()
    {
        if (isDisplayActive && focusedMonster != null)
        {
            UpdateHealthDisplay();
        }
    }

    public void UpdateHealth(Monster monster)
    {
        if (!isDisplayActive || monster != focusedMonster) return;

        UpdateHealthDisplay();
    }

    private void UpdateHealthDisplay()
    {
        if (focusedMonster == null) return;

        float healthPercentage = focusedMonster.currentHealth / focusedMonster.health;
        healthBar.fillAmount = Mathf.Clamp01(healthPercentage);

        if (healthPercentage <= 0)
        {
            HideMonsterDisplay();
        }
    }

    private int GetMovementSpeed(float movementSpeed)
    {
        if (movementSpeed <= 0.41f) return 1;
        if (movementSpeed <= 0.71f) return 2;
        if (movementSpeed <= 1.1f) return 3;
        if (movementSpeed <= 1.51f) return 4;
        return 5;
    }

    public void ChangeChildrenText(Monster myMonster)
    {
        Transform typeChild = text.transform.Find("Type");
        Transform speedChild = text.transform.Find("Speed");
        Transform damageChild = text.transform.Find("Damage");

        TextMeshProUGUI typeText = typeChild.GetComponent<TextMeshProUGUI>();
        if (typeText != null)
        {
            string monsterName = myMonster.name;
            var parts = Regex.Matches(monsterName, @"[A-Z][a-z]*|[0-9]+");
            string formatted = string.Join(" ", parts).ToUpper();
            formatted = Regex.Replace(formatted, @"(\d+)$", "TIER $1");
            typeText.text = $"TYPE: {formatted}";
        }

        TextMeshProUGUI speedText = speedChild.GetComponent<TextMeshProUGUI>();
        if (speedText != null)
        {
            speedText.text = $"SPEED: {GetMovementSpeed(myMonster.movementSpeed)}/5";
        }

        TextMeshProUGUI damageText = damageChild.GetComponent<TextMeshProUGUI>();
        if (damageText != null)
        {
            damageText.text = $"DAMAGE: {myMonster.damage}";
        }
    }

    public void ShowMonsterDisplay(Monster monster)
    {
        if (monster == null) return;

        focusedMonster = monster;
        isDisplayActive = true;
        monsterDisplayCanvasGroup.alpha = 1f;

        monsterFeedImage.sprite = GetMonsterIcon(monster.name);
        ChangeChildrenText(monster);
        UpdateHealthDisplay();

        moneyHandler.UpdateMoney();
        moneyHandler.MoveTransform(true);
    }

    public void HideMonsterDisplay()
    {
        isDisplayActive = false;
        monsterDisplayCanvasGroup.alpha = 0f;
        focusedMonster = null;
        moneyHandler.MoveTransform(false);
    }

    private Sprite GetMonsterIcon(string monsterName)
    {
        Sprite[] iconArray = GetIconArrayForMonsterType(monsterName);

        if (iconArray == null)
        {
            Debug.LogError($"No icon array found for monster: {monsterName}");
            return null;
        }

        if (int.TryParse(monsterName[^1].ToString(), out int tier))
        {
            tier = Mathf.Clamp(tier - 1, 0, iconArray.Length - 1);
            return iconArray[tier];
        }

        Debug.LogError($"Invalid monster name format: {monsterName}. Expected to extract a tier.");
        return null;
    }

    private Sprite[] GetIconArrayForMonsterType(string monsterName)
    {
        if (monsterName.Contains("Health")) return healthIcons;
        if (monsterName.Contains("Strength")) return strengthIcons;
        if (monsterName.Contains("Speed")) return speedIcons;
        if (monsterName.Contains("Invisible")) return invisibleIcons;
        return null;
    }
}