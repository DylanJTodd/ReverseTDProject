using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WaveManager : MonoBehaviour
{
    public MoneyHandler moneyHandler;
    public Button skipButton;
    public TextMeshProUGUI waveText;
    public GameObject monsterHandler;
    public GameOver gameOver;

    private int waveNumber = 0;
    void Start()
    {
        waveNumber += 1;
        NextWave(waveNumber);

        skipButton.onClick.AddListener(HandleSkip);
    }
    void Update()
    {
        CheckAvailableMoney();
    }
    private void CheckAvailableMoney()
    {
        if (moneyHandler.GetMoney() < 50)
        {
            if (CheckMonsters())
            {
                waveNumber += 1;
                NextWave(waveNumber);
            }
        }
    }

    private bool CheckMonsters()
    {
        return monsterHandler.transform.childCount == 0;
    }

    private void NextWave(int number)
    {
        if (number >= 16)
        {
            gameOver.GameLost();
        }
        waveText.text = $"WAVE: {waveNumber} / 15";
        WaveMoney();
    }

    private void HandleSkip()
    {
        waveNumber += 1;
        NextWave(waveNumber);
    }

    private void WaveMoney()
    {
        if(waveNumber <= 5)
        {
            moneyHandler.AddMoney(500);
        }
        else if (waveNumber <= 10)
        {
            moneyHandler.AddMoney(750);
        }
        else
        {
            moneyHandler.AddMoney(1000);
        }
    }
}
