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
    public MoneyHandler moneyHandler;

    private Monster focusedMonster;

    private void Start()
    {
        HideMonsterDisplay();
    }

    private int GetMovementSpeed(float movementSpeed)
    {
        return Mathf.RoundToInt(movementSpeed * 5);
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
        monsterFeedImage.sprite = GetMonsterIcon(monster.name);
        ChangeChildrenText(monster);
        moneyHandler.UpdateMoney();
        moneyHandler.MoveTransform(true);

        monsterDisplayCanvasGroup.alpha = 1f;
        focusedMonster = monster;
    }

    public void HideMonsterDisplay()
    {
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

        int tier;
        if (int.TryParse(monsterName.Substring(monsterName.Length - 1), out tier))
        {
            tier = Mathf.Clamp(tier - 1, 0, iconArray.Length - 1);
            return iconArray[tier];
        }
        else
        {
            Debug.LogError($"Invalid monster name format: {monsterName}. Expected to extract a tier.");
            return null;
        }
    }

    private Sprite[] GetIconArrayForMonsterType(string monsterName)
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
}