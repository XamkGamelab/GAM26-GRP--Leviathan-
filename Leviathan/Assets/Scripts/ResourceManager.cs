using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResourceManager : MonoBehaviour
{
    [SerializeField] private TMP_Text biomassText;
    [SerializeField] private TMP_Text oreText;
    [SerializeField] private TMP_Text energyText;
    [SerializeField] private TMP_Text airDomeQuestProgressText;
    [SerializeField, Min(1)] private int airDomeCost = 10;

    [SerializeField] private Button biomassUpgradeButton;
    [SerializeField] private TMP_Text biomassUpgradeButtonText;

    [SerializeField] private Button biomassCollectorButton;
    [SerializeField] private TMP_Text biomassCollectorButtonText;
    [SerializeField] private GameObject biomassCollectorPrefab;
    [SerializeField] private Transform biomassCollectorSpawnPoint;
    [SerializeField] private GameObject airDome;
    [SerializeField] private Button airDomeGoButton;
    [SerializeField] private TMP_Text workshopQuestProgressText;
    [SerializeField] private Button workshopGoButton;
    [SerializeField, Min(1)] private int workshopOreCost = 5;
    [SerializeField, Min(1)] private int workshopEnergyCost = 5;
    [SerializeField, Min(1)] private int energyPerSecond = 1;
    

    private bool airDomeBuilt;
    private const string AirDomeBuiltKey = "Leviathan_AirDomeBuilt";

    private bool workshopRepaired;
    private const string WorkshopRepairedKey = "Leviathan_WorkshopRepaired";
    private const string EnergyProductionTimerKey = "Leviathan_EnergyProductionTimer";

    private float energyProductionTimer;

    private int biomass;
    private int ore;
    private int energy;

    private int biomassPerClick = 1;
    private const int BiomassUpgradeCost = 5;
    private bool biomassUpgradePurchased;

    private const int BiomassCollectorCost = 10;
    private bool biomassCollectorBuilt;
    private GameObject biomassCollectorInstance;

    private const string SaveExistsKey = "Leviathan_SaveExists";
    private const string BiomassKey = "Leviathan_Biomass";
    private const string OreKey = "Leviathan_Ore";
    private const string EnergyKey = "Leviathan_Energy";
    private const string UpgradeKey = "Leviathan_BiomassUpgrade";
    private const string CollectorKey = "Leviathan_BiomassCollector";
    private const string BiomassTimerKey = "Leviathan_BiomassTimer";

    private void Start()
    {
        if (PlayerPrefs.GetInt(SaveExistsKey, 0) == 1)
        {
            LoadGame();
        }
        else
        {
            UpdateUI();
            UpdateUpgradeButton();
            UpdateBiomassCollectorButton();

            if (airDome != null)
            {
                airDome.SetActive(false);
            }
        }
    }

    public void CollectBiomass()
    {
        biomass += biomassPerClick;
        UpdateUI();
    }

    public void BuyBiomassUpgrade()
    {
        if (biomassUpgradePurchased || biomass < BiomassUpgradeCost)
        {
            return;
        }

        biomass -= BiomassUpgradeCost;
        biomassPerClick = 2;
        biomassUpgradePurchased = true;

        UpdateUI();
        UpdateUpgradeButton();
    }

    public void BuildBiomassCollector()
    {
        if (biomassCollectorBuilt || biomass < BiomassCollectorCost)
        {
            return;
        }

        if (biomassCollectorPrefab == null || biomassCollectorSpawnPoint == null)
        {
            Debug.LogError("Assign the biomass collector prefab and spawn point in ResourceManager.");
            return;
        }

        biomass -= BiomassCollectorCost;
        biomassCollectorBuilt = true;
        SpawnBiomassCollector();

        UpdateUI();
        UpdateBiomassCollectorButton();
    }

    public void BuildAirDome()
    {
        if (airDomeBuilt || biomass < airDomeCost)
        {
            return;
        }

        if (airDome == null)
        {
            Debug.LogError("Assign AirDome_Blockout in ResourceManager.");
            return;
        }

        biomass -= airDomeCost;
        airDomeBuilt = true;
        airDome.SetActive(true);
        UpdateUI();
    }

    public void RepairWorkshop()
    {
        if (!airDomeBuilt || workshopRepaired)
        {
            return;
        }

        if (ore < workshopOreCost || energy < workshopEnergyCost)
        {
            return;
        }

        ore -= workshopOreCost;
        energy -= workshopEnergyCost;
        workshopRepaired = true;
        energyProductionTimer = 0f;

        UpdateUI();
    }

    private void SpawnBiomassCollector()
    {
        if (biomassCollectorPrefab == null || biomassCollectorSpawnPoint == null)
        {
            Debug.LogError("Biomass collector prefab or spawn point is missing.");
            return;
        }

        biomassCollectorInstance = Instantiate(
            biomassCollectorPrefab,
            biomassCollectorSpawnPoint.position,
            biomassCollectorSpawnPoint.rotation
        );
    }

    public void SaveGame()
    {
        PlayerPrefs.SetInt(SaveExistsKey, 1);
        PlayerPrefs.SetInt(BiomassKey, biomass);
        PlayerPrefs.SetInt(OreKey, ore);
        PlayerPrefs.SetInt(EnergyKey, energy);
        PlayerPrefs.SetInt(UpgradeKey, biomassUpgradePurchased ? 1 : 0);
        PlayerPrefs.SetInt(CollectorKey, biomassCollectorBuilt ? 1 : 0);
        PlayerPrefs.SetInt(AirDomeBuiltKey, airDomeBuilt ? 1 : 0);
        PlayerPrefs.SetInt(WorkshopRepairedKey, workshopRepaired ? 1 : 0);
        PlayerPrefs.SetFloat(EnergyProductionTimerKey, energyProductionTimer);
        PlayerPrefs.Save();

        Debug.Log("Game saved.");
    }

    public void LoadGame()
    {
        if (PlayerPrefs.GetInt(SaveExistsKey, 0) == 0)
        {
            return;
        }

        biomass = PlayerPrefs.GetInt(BiomassKey, 0);
        ore = PlayerPrefs.GetInt(OreKey, 0);
        energy = PlayerPrefs.GetInt(EnergyKey, 0);

        biomassUpgradePurchased = PlayerPrefs.GetInt(UpgradeKey, 0) == 1;
        biomassPerClick = biomassUpgradePurchased ? 2 : 1;

        biomassCollectorBuilt = PlayerPrefs.GetInt(CollectorKey, 0) == 1;
        workshopRepaired = PlayerPrefs.GetInt(WorkshopRepairedKey, 0) == 1;

        energyProductionTimer = PlayerPrefs.GetFloat(EnergyProductionTimerKey, 0f);

        if (biomassCollectorInstance != null)
        {
            Destroy(biomassCollectorInstance);
            biomassCollectorInstance = null;
        }

        if (biomassCollectorBuilt)
        {
            SpawnBiomassCollector();
        }

        airDomeBuilt = PlayerPrefs.GetInt(AirDomeBuiltKey, 0) == 1;

        if (airDome != null)
        {
            airDome.SetActive(airDomeBuilt);
        }

        UpdateUI();
        UpdateUpgradeButton();
        UpdateBiomassCollectorButton();

        Debug.Log("Game loaded.");
    }

    public void AddOre(int amount)
    {
        ore += amount;
        UpdateUI();
    }

    public void AddEnergy(int amount)
    {
        energy += amount;
        UpdateUI();
    }

    public void SalvageOre()
    {
        if (!airDomeBuilt)
        {
            return;
        }

        AddOre(1);
    }

    public void RecoverEnergy()
    {
        if (!airDomeBuilt)
        {
            return;
        }

        AddEnergy(1);
    }

    private void UpdateUI()
    {
        biomassText.text = $"Biomass: {biomass}";
        oreText.text = $"Ore: {ore}";
        energyText.text = $"Energy: {energy}";

        if (airDomeQuestProgressText != null)
        {
            airDomeQuestProgressText.text = airDomeBuilt
                ? "Completed"
                : $"Biomass: {biomass} / {airDomeCost}";
        }

        if (airDomeGoButton != null)
        {
            airDomeGoButton.interactable = !airDomeBuilt && biomass >= airDomeCost;
        }

        if (workshopQuestProgressText != null)
        {
            if (workshopRepaired)
            {
                workshopQuestProgressText.text = "Completed";
            }
            else if (!airDomeBuilt)
            {
                workshopQuestProgressText.text = "Locked — build the Air Dome first";
            }
            else
            {
                workshopQuestProgressText.text =
                    $"Ore: {ore} / {workshopOreCost} | Energy: {energy} / {workshopEnergyCost}";
            }
        }

        if (workshopGoButton != null)
        {
            workshopGoButton.interactable =
                airDomeBuilt &&
                !workshopRepaired &&
                ore >= workshopOreCost &&
                energy >= workshopEnergyCost;
        }
    }

    private void UpdateUpgradeButton()
    {
        if (biomassUpgradeButton != null)
        {
            biomassUpgradeButton.interactable = !biomassUpgradePurchased;
        }

        if (biomassUpgradeButtonText != null)
        {
            biomassUpgradeButtonText.text = biomassUpgradePurchased
                ? "Upgrade Purchased"
                : "Upgrade Implant - 5 Biomass";
        }
    }

    private void UpdateBiomassCollectorButton()
    {
        if (biomassCollectorButton != null)
        {
            biomassCollectorButton.interactable = !biomassCollectorBuilt;
        }

        if (biomassCollectorButtonText != null)
        {
            biomassCollectorButtonText.text = biomassCollectorBuilt
                ? "Collector Purchased"
                : "Build Biomass Collector - 10 Biomass";
        }
    }

    private void Update()
    {
        if (!workshopRepaired)
        {
            return;
        }

        energyProductionTimer += Time.deltaTime;

        int elapsedSeconds = Mathf.FloorToInt(energyProductionTimer);

        if (elapsedSeconds > 0)
        {
            energyProductionTimer -= elapsedSeconds;
            AddEnergy(elapsedSeconds * energyPerSecond);
        }
    }
}