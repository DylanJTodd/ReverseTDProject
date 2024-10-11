using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MoneyHandler : MonoBehaviour
{
    public MonsterSpawningHandler monsterSpawningHandler;

    private int playerMoney = 5000;

    public void Start()
    {
        UpdateMoney();
    }
    public void UpdateMoney()
    {
        Transform textChild = transform.Find("Text");
        TextMeshProUGUI textMeshPro = textChild.GetComponent<TextMeshProUGUI>();
        textMeshPro.text = $"MONEY: {playerMoney}";
    }

    public void MoveTransform(bool direction)
    {
        RectTransform rectTransform = GetComponent<RectTransform>();
        rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, direction ? -200 : 0);
    }

    public void AddMoney(int amount)
    {
        playerMoney += amount;
        UpdateMoney();
    }

    public void RemoveMoney(int amount)
    {
        playerMoney -= amount;
        UpdateMoney();
    }

    public int GetMoney()
    {
        return playerMoney;
    }



}
