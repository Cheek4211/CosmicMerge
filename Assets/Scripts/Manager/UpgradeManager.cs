using UnityEngine;

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance { get; private set; }

    [Header("Currency (Debug View)")]
    [SerializeField] private int totalPoints = 0;
    
    public int TotalPoints => totalPoints; 

    [Header("Passive Skills (Debug View)")]
    [SerializeField] private int enginePowerLevel = 1;
    [SerializeField] private int universeSizeLevel = 1;
    [SerializeField] private int techLevel = 1;

    public int EnginePowerLevel => enginePowerLevel;
    public int UniverseSizeLevel => universeSizeLevel;
    public int TechLevel => techLevel;

    [Header("Active Skills (Debug View)")]
    [SerializeField] private bool isCosmicWarUnlocked = false;
    [SerializeField] private int ammoCosmicWar = 0;

    [SerializeField] private bool isAlienAbductionUnlocked = false;
    [SerializeField] private int ammoAlienAbduction = 0;

    [SerializeField] private bool isEvolutionLightUnlocked = false;
    [SerializeField] private int ammoEvolutionLight = 0;

    public bool IsCosmicWarUnlocked => isCosmicWarUnlocked;
    public int AmmoCosmicWar => ammoCosmicWar;
    public bool IsAlienAbductionUnlocked => isAlienAbductionUnlocked;
    public int AmmoAlienAbduction => ammoAlienAbduction;
    public bool IsEvolutionLightUnlocked => isEvolutionLightUnlocked;
    public int AmmoEvolutionLight => ammoEvolutionLight;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadData();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SaveData()
    {
        PlayerPrefs.SetInt("TotalPoints", totalPoints);

        PlayerPrefs.SetInt("Passive_Engine", enginePowerLevel);
        PlayerPrefs.SetInt("Passive_Universe", universeSizeLevel);
        PlayerPrefs.SetInt("Passive_Tech", techLevel);

        PlayerPrefs.SetInt("Active_CosmicWar_Unlock", isCosmicWarUnlocked ? 1 : 0); // bool은 1과 0으로 저장
        PlayerPrefs.SetInt("Active_CosmicWar_Ammo", ammoCosmicWar);

        PlayerPrefs.SetInt("Active_Alien_Unlock", isAlienAbductionUnlocked ? 1 : 0);
        PlayerPrefs.SetInt("Active_Alien_Ammo", ammoAlienAbduction);

        PlayerPrefs.SetInt("Active_Evolution_Unlock", isEvolutionLightUnlocked ? 1 : 0);
        PlayerPrefs.SetInt("Active_Evolution_Ammo", ammoEvolutionLight);

        PlayerPrefs.Save();
    }

    private void LoadData()
    {
        totalPoints = PlayerPrefs.GetInt("TotalPoints", 0);

        enginePowerLevel = PlayerPrefs.GetInt("Passive_Engine", 1);
        universeSizeLevel = PlayerPrefs.GetInt("Passive_Universe", 1);
        techLevel = PlayerPrefs.GetInt("Passive_Tech", 1);

        isCosmicWarUnlocked = PlayerPrefs.GetInt("Active_CosmicWar_Unlock", 0) == 1;
        ammoCosmicWar = PlayerPrefs.GetInt("Active_CosmicWar_Ammo", 0);

        isAlienAbductionUnlocked = PlayerPrefs.GetInt("Active_Alien_Unlock", 0) == 1;
        ammoAlienAbduction = PlayerPrefs.GetInt("Active_Alien_Ammo", 0);

        isEvolutionLightUnlocked = PlayerPrefs.GetInt("Active_Evolution_Unlock", 0) == 1;
        ammoEvolutionLight = PlayerPrefs.GetInt("Active_Evolution_Ammo", 0);
    }

    public void AddPoints(int amount)
    {
        totalPoints += amount;
        SaveData();
    }

    public bool TrySpendPoints(int amount)
    {
        if (totalPoints >= amount)
        {
            totalPoints -= amount;
            SaveData();
            return true;
        }
        return false;
    }

    public void UpgradePassive(string skillName)
    {
        if (skillName == "Engine") enginePowerLevel++;
        else if (skillName == "Universe") universeSizeLevel++;
        else if (skillName == "Tech") techLevel++;
        SaveData();
    }

    public void PurchaseActive(string skillName, bool isFirstUnlock)
    {
        if (skillName == "CosmicWar")
        {
            if (isFirstUnlock) isCosmicWarUnlocked = true;
            ammoCosmicWar = 1; // 최대 1발 장전
        }
        else if (skillName == "Alien")
        {
            if (isFirstUnlock) isAlienAbductionUnlocked = true;
            ammoAlienAbduction = 1;
        }
        else if (skillName == "Evolution")
        {
            if (isFirstUnlock) isEvolutionLightUnlocked = true;
            ammoEvolutionLight = 1;
        }
        SaveData();
    }

    public void ConsumeActiveAmmo(string skillName)
    {
        if (skillName == "CosmicWar" && ammoCosmicWar > 0) ammoCosmicWar--;
        else if (skillName == "Alien" && ammoAlienAbduction > 0) ammoAlienAbduction--;
        else if (skillName == "Evolution" && ammoEvolutionLight > 0) ammoEvolutionLight--;
        SaveData();
    }
}