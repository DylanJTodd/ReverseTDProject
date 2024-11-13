using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeButtonHandler : MonoBehaviour
{
    public Button upgradeButton;
    public Button strengthUpgrade;
    public Button speedUpgrade;
    public Button healthUpgrade;

    public GameObject strengthProgress;
    public GameObject speedProgress;
    public GameObject healthProgress;
    public GameObject monsterHolder;

    public CanvasGroup upgradePanel;
    public CanvasGroup pricePanel;

    public TextMeshProUGUI amountText;

    public MoneyHandler moneyHandler;

    private bool isOpened = false;

    public int strengthUpgrades = 0;
    public int speedUpgrades = 0;
    public int healthUpgrades = 0;

    void Start()
    {
        upgradeButton.onClick.AddListener(OpenUpgrades);

        strengthUpgrade.onClick.AddListener(() => HandleUpgrades(strengthUpgrade));
        speedUpgrade.onClick.AddListener(() => HandleUpgrades(speedUpgrade));
        healthUpgrade.onClick.AddListener(() => HandleUpgrades(healthUpgrade));
    }

    private void HandleUpgrades(Button clickedButton)
    {
        if (clickedButton.name == "StrengthUpgrade")
        {
            if (strengthUpgrades == 3) return;

            bool money = BuyUpgrade(strengthUpgrades, strengthProgress);
            if (money)
            {
                strengthUpgrades += 1;
                ApplyUpgradeToAllMonsters("strength", strengthUpgrades);
            }

            if (strengthUpgrades == 3)
            {
                pricePanel.alpha = 0;
                strengthUpgrade.interactable = false;
            }
        }
        else if (clickedButton.name == "SpeedUpgrade")
        {
            if (speedUpgrades == 3) return;

            bool money = BuyUpgrade(speedUpgrades, speedProgress);
            if (money)
            {
                speedUpgrades += 1;
                ApplyUpgradeToAllMonsters("speed", speedUpgrades);
            }

            if (speedUpgrades == 3)
            {
                pricePanel.alpha = 0;
                speedUpgrade.interactable = false;
            }
        }
        else if (clickedButton.name == "HealthUpgrade")
        {
            if (healthUpgrades == 3) return;

            bool money = BuyUpgrade(healthUpgrades, healthProgress);
            if (money)
            {
                healthUpgrades += 1;
                ApplyUpgradeToAllMonsters("health", healthUpgrades);
            }

            if (healthUpgrades == 3)
            {
                pricePanel.alpha = 0;
                healthUpgrade.interactable = false;
            }
        }
    }

    private void ApplyUpgradeToAllMonsters(string upgradeType, int tier)
    {
        // Loop through all the monsters in the monsterHolder and apply the relevant upgrade
        foreach (Monster monster in MonsterManager.instance.GetMonsters())
        {
            monster.Upgrade(upgradeType, tier);
        }
    }

    private void OpenUpgrades()
    {
        if (isOpened)
        {
            upgradePanel.alpha = 0;
            isOpened = false;
        }
        else
        {
            upgradePanel.alpha = 1;
            isOpened = true;
        }
    }

    public void HandleStrengthHover()
    {
        if(strengthUpgrades == 3)
        {
            return;
        }
        int tierPrice = GetTierPrice(strengthUpgrades);
        amountText.text = tierPrice.ToString();
        pricePanel.alpha = 1;
    }

    public void HandleSpeedHover()
    {
        if (speedUpgrades == 3)
        {
            return;
        }
        int tierPrice = GetTierPrice(speedUpgrades);
        amountText.text = tierPrice.ToString();
        pricePanel.alpha = 1;
    }
    public void HandleHealthHover()
    {
        if (healthUpgrades == 3)
        {
            return;
        }
        int tierPrice = GetTierPrice(healthUpgrades);
        amountText.text = tierPrice.ToString();
        pricePanel.alpha = 1;
    }

    public bool BuyUpgrade(int tier, GameObject progressTracker)
    {
        int tierPrice = GetTierPrice(tier);
        int currentMoney = moneyHandler.GetMoney();

        if(tierPrice < currentMoney)
        {
            moneyHandler.RemoveMoney(tierPrice);

            progressTracker.transform.Find((tier + 1).ToString()).Find("Image").GetComponent<Image>().fillAmount = 1;
            amountText.text = tierPrice.ToString();
            return true;
        }
        return false;
    }

    public void HandleUnhover()
    {
        pricePanel.alpha = 0;
        amountText.text = "N/A";
    }

    private int GetTierPrice(int tier)
    {
        if (tier == 0)
        {
            return 100;
        }
        else if (tier == 1)
        {
            return 500;
        }
        else if (tier == 2)
        {
            return 1500;
        }
        return -1;
    }
}
