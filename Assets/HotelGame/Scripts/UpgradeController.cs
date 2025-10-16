using UnityEngine;
using System;
using System.Collections.Generic;
using TMPro;

[Serializable]
public class UpgradeLevel
{
    public int level;
    public List<GameObject> objectsToActivate;
}

public class UpgradeController : MonoBehaviour
{
    public static UpgradeController Instance;

    public GameObject haveNoMoneyPanel;
    public GameObject maxLevelPanel;
    
    [Header("Casino Upgrades")]
    public List<UpgradeLevel> casinoUpgrades;
    public TextMeshProUGUI casinoLevelText;
    
    [Header("Hotel Upgrades")]
    public List<UpgradeLevel> hotelUpgrades;
    public TextMeshProUGUI hotelLevelText;
    
    [Header("Settings")]
    public int upgradeCost = 100;
    
    private int currentCasinoLevel = 1;
    private int currentHotelLevel = 1;

    void Awake()
    {
        Instance = this;
        LoadUpgrades();
    }

    void Start()
    {
        ApplyCasinoUpgrades();
        ApplyHotelUpgrades();
        UpdateUI();
    }

    public void UpgradeCasino()
    {
        if (WalletController.Instance.Money < upgradeCost)
        {
            Debug.Log("Not enough money to upgrade casino!");
            haveNoMoneyPanel.SetActive(true);
            return;
        }

        if (currentCasinoLevel >= casinoUpgrades.Count + 1)
        {
            Debug.Log("Casino is already at max level!");
            maxLevelPanel.SetActive(true);
            return;
        }

        WalletController.Instance.Money -= upgradeCost;
        currentCasinoLevel++;
        SaveCasinoLevel();
        ApplyCasinoUpgrades();
        UpdateUI();
        
        Debug.Log($"Casino upgraded to level {currentCasinoLevel}");
    }

    public void UpgradeHotel()
    {
        if (WalletController.Instance.Money < upgradeCost)
        {
            Debug.Log("Not enough money to upgrade hotel!");
            haveNoMoneyPanel.SetActive(true);
            return;
        }

        if (currentHotelLevel >= hotelUpgrades.Count + 1)
        {
            Debug.Log("Hotel is already at max level!");
            maxLevelPanel.SetActive(true);
            return;
        }

        WalletController.Instance.Money -= upgradeCost;
        currentHotelLevel++;
        SaveHotelLevel();
        ApplyHotelUpgrades();
        UpdateUI();
        
        Debug.Log($"Hotel upgraded to level {currentHotelLevel}");
    }

    private void ApplyCasinoUpgrades()
    {
        for (int i = 0; i < casinoUpgrades.Count; i++)
        {
            bool shouldActivate = casinoUpgrades[i].level <= currentCasinoLevel;
            
            foreach (GameObject obj in casinoUpgrades[i].objectsToActivate)
            {
                if (obj != null)
                {
                    obj.SetActive(shouldActivate);
                }
            }
        }
    }

    private void ApplyHotelUpgrades()
    {
        for (int i = 0; i < hotelUpgrades.Count; i++)
        {
            bool shouldActivate = hotelUpgrades[i].level <= currentHotelLevel;
            
            foreach (GameObject obj in hotelUpgrades[i].objectsToActivate)
            {
                if (obj != null)
                {
                    obj.SetActive(shouldActivate);
                }
            }
        }
    }

    private void UpdateUI()
    {
        if (casinoLevelText != null)
        {
            casinoLevelText.text = $"Level {currentCasinoLevel}";
        }
        
        if (hotelLevelText != null)
        {
            hotelLevelText.text = $"Level {currentHotelLevel}";
        }
    }

    private void LoadUpgrades()
    {
        currentCasinoLevel = PlayerPrefs.GetInt("casinoLevel", 1);
        currentHotelLevel = PlayerPrefs.GetInt("hotelLevel", 1);
    }

    private void SaveCasinoLevel()
    {
        PlayerPrefs.SetInt("casinoLevel", currentCasinoLevel);
        PlayerPrefs.Save();
    }

    private void SaveHotelLevel()
    {
        PlayerPrefs.SetInt("hotelLevel", currentHotelLevel);
        PlayerPrefs.Save();
    }

    public int GetCasinoLevel()
    {
        return currentCasinoLevel;
    }

    public int GetHotelLevel()
    {
        return currentHotelLevel;
    }

    public bool CanUpgradeCasino()
    {
        return WalletController.Instance.Money >= upgradeCost && currentCasinoLevel < casinoUpgrades.Count;
    }

    public bool CanUpgradeHotel()
    {
        return WalletController.Instance.Money >= upgradeCost && currentHotelLevel < hotelUpgrades.Count;
    }
}