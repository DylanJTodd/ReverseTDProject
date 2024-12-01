using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatTracker : MonoBehaviour
{
    private int moneyGained;
    private int moneyUsed;
    private int highestHeldMoney = 0;
    private float averageSpending;
    private int wavesUsed = 0;
    private float longestWaveLength = 0f;
    private float shortestWaveLength = float.MaxValue;
    private float averageWaveLength;
    private float totalTimeTaken = 0f;
    private bool isFF = false;
    private bool isPaused = false;

    private float currentWaveTimer = 0f;
    private List<float> waveTimes = new List<float>();

    private void Update()
    {
        if (!isPaused)
        {
            float deltaTime = isFF ? Time.deltaTime * 2 : Time.deltaTime;
            totalTimeTaken += deltaTime;
        }
    }

    public void Start()
    {
        StartWave();
    }

    public void FastForwardToggle()
    {
        isFF = !isFF;
    }

    public void PauseToggle()
    {
        isPaused = !isPaused;
    }

    public void StartWave()
    {
        currentWaveTimer = totalTimeTaken;
    }

    public void EndWave()
    {
        if (!(wavesUsed < 1))
        {
            float currentTotalTime = totalTimeTaken -= currentWaveTimer;
            if (currentTotalTime < shortestWaveLength)
            {
                shortestWaveLength = currentTotalTime;
            }
            if (currentTotalTime > longestWaveLength)
            {
                longestWaveLength = currentTotalTime;
            }

            float totalWaveTime = 0f;
            waveTimes.Add(currentTotalTime);

            foreach (float waveTime in waveTimes)
            {
                totalWaveTime += waveTime;
            }
            averageWaveLength = totalWaveTime / waveTimes.Count;
        }
    }

    public void AddTotalMoney(int amount)
    {
        moneyGained += amount;
    }

    public void AddSpentMoney(int amount)
    {
        moneyUsed += amount;
    }

    public void NextWave()
    {
        EndWave();
        StartWave();
        wavesUsed++;
    }

    public void UpdateWaveMoney(int amount)
    {
        if (amount > highestHeldMoney)
        {
            highestHeldMoney = amount;
        }
    }

    public List<string> ReturnStats()
    {
        averageSpending = wavesUsed > 0 ? (float)moneyUsed / (wavesUsed - 1) : 0;
        NextWave();
        float tmp = 0f;
        foreach (float waveTime  in waveTimes)
        {
            tmp += waveTime;
        }
        totalTimeTaken = tmp;

        List<string> stats = new List<string>
        {
            moneyGained.ToString(),
            moneyUsed.ToString(),
            highestHeldMoney.ToString(),
            averageSpending.ToString("F2"),
            (wavesUsed-1).ToString(),
            longestWaveLength.ToString("F2"),
            shortestWaveLength == float.MaxValue ? "0.00" : shortestWaveLength.ToString("F2"),
            averageWaveLength.ToString("F2"),
            totalTimeTaken.ToString("F2")
        };
        return stats;
    }
}