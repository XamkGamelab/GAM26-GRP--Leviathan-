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
    
    [Header("Robotic Mining")]
    [SerializeField] private TMP_Text miningQuestProgressText;
    [SerializeField] private Button miningGoButton;
    [SerializeField, Min(0)] private int miningOreCost = 5;
    [SerializeField, Min(0)] private int miningEnergyCost = 5;
    [SerializeField, Min(1)] private int orePerSecond = 2;
    [SerializeField] private GameObject miningRobot;

    [SerializeField] private TMP_Text miningUpgradeProgressText;
    [SerializeField] private Button miningUpgradeGoButton;

    [Header("Workshop Quest Guidance")]
    [SerializeField] private QuestMarker questMarker;
    [SerializeField] private Transform oreGuidanceTarget;
    [SerializeField] private Transform energyGuidanceTarget;

    private enum GuidedQuest
    {
        None,
        AirDome,
        Workshop,
        Mining,
        MiningUpgrade
    }

    private GuidedQuest selectedQuest;
    private float guidanceTimer;

    private int miningLevel = 1;
    private const int MaxMiningLevel = 100;
    private const string MiningLevelKey = "Leviathan_MiningLevel";

    private int MiningUpgradeCost =>
        Mathf.CeilToInt(20f * Mathf.Pow(1.25f, miningLevel - 1));

    private int MiningProduction => orePerSecond + miningLevel - 1;

    private bool miningStarted;
    private float oreProductionTimer;

    private const string MiningStartedKey = "Leviathan_MiningStarted";
    private const string OreProductionTimerKey = "Leviathan_OreProductionTimer";

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
        if (miningRobot != null)
        {
            miningRobot.SetActive(false);
        }

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
        
        if (questMarker != null)
        {
            questMarker.Hide();
        }

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
        PlayerPrefs.SetInt(MiningStartedKey, miningStarted ? 1 : 0);
        PlayerPrefs.SetFloat(OreProductionTimerKey, oreProductionTimer);
        PlayerPrefs.SetInt(MiningLevelKey, miningLevel);
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
        miningStarted = PlayerPrefs.GetInt(MiningStartedKey, 0) == 1;

        miningLevel = Mathf.Clamp(PlayerPrefs.GetInt(MiningLevelKey, 1),
         1,
         MaxMiningLevel
        );

        if (miningRobot != null)
        {
            miningRobot.SetActive(miningStarted);
        }

        oreProductionTimer = PlayerPrefs.GetFloat(OreProductionTimerKey, 0f);

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

        if (miningQuestProgressText != null)
        {
            if (miningStarted)
            {
                miningQuestProgressText.text = "Completed";
            }
            else if (!workshopRepaired)
            {
                miningQuestProgressText.text = "Repair the workshop first";
            }
            else
            {
                miningQuestProgressText.text =
                    $"Ore: {ore} / {miningOreCost}\n" +
                    $"Energy: {energy} / {miningEnergyCost}";
            }
        }

        if (miningGoButton != null)
        {
            miningGoButton.interactable =
                workshopRepaired &&
                !miningStarted &&
                ore >= miningOreCost &&
                energy >= miningEnergyCost;
        }

        if (miningUpgradeProgressText != null)
        {
            if (!miningStarted)
            {
                miningUpgradeProgressText.text = "Start robotic mining first";
            }
            else if (miningLevel >= MaxMiningLevel)
            {
                miningUpgradeProgressText.text =
                    $"Level {miningLevel} | {MiningProduction} Ore/s\nMax level";
            }
            else
            {
                miningUpgradeProgressText.text =
                    $"Level {miningLevel} | {MiningProduction} → {MiningProduction + 1} Ore/s\n" +
                    $"Cost: {MiningUpgradeCost} Ore";
            }
        }

        if (miningUpgradeGoButton != null)
        {
            miningUpgradeGoButton.interactable =
                miningStarted &&
                miningLevel < MaxMiningLevel &&
                ore >= MiningUpgradeCost;
        }

        UpdateQuestGuidance();
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
        if (workshopRepaired)
        {
            energyProductionTimer += Time.deltaTime;

            int elapsedSeconds = Mathf.FloorToInt(energyProductionTimer);

            if (elapsedSeconds > 0)
            {
                energyProductionTimer -= elapsedSeconds;
                AddEnergy(elapsedSeconds * energyPerSecond);
            }

            guidanceTimer += Time.unscaledDeltaTime;

            if (guidanceTimer >= 0.3f)
            {
                guidanceTimer = 0f;

                if (selectedQuest != GuidedQuest.None)
                {
                    UpdateQuestGuidance();
                }
            }
        }

        if (miningStarted)
        {
            oreProductionTimer += Time.deltaTime;

            int elapsedSeconds = Mathf.FloorToInt(oreProductionTimer);

            if (elapsedSeconds > 0)
            {
                oreProductionTimer -= elapsedSeconds;
                AddOre(elapsedSeconds * MiningProduction);
            }
        }
    }

    public void StartRoboticMining()
    {
        if (!workshopRepaired || miningStarted)
        {
            return;
        }

        if (ore < miningOreCost || energy < miningEnergyCost)
        {
            return;
        }

        ore -= miningOreCost;
        energy -= miningEnergyCost;

        miningStarted = true;
        if (miningRobot != null)
        {
            miningRobot.SetActive(true);
        }
        oreProductionTimer = 0f;

        UpdateUI();
    }

    public void UpgradeMining()
    {
        if (!miningStarted || miningLevel >= MaxMiningLevel)
        {
            return;
        }

        int cost = MiningUpgradeCost;

        if (ore < cost)
        {
            return;
        }

        ore -= cost;
        miningLevel++;

        UpdateUI();
    }

    public void SelectAirDomeQuest()
    {
        selectedQuest = GuidedQuest.AirDome;
        UpdateQuestGuidance();
    }

    public void SelectWorkshopQuest()
    {
        selectedQuest = GuidedQuest.Workshop;
        UpdateQuestGuidance();
    }

    public void SelectMiningQuest()
    {
        selectedQuest = GuidedQuest.Mining;
        UpdateQuestGuidance();
    }

    public void SelectMiningUpgradeQuest()
    {
        selectedQuest = GuidedQuest.MiningUpgrade;
        UpdateQuestGuidance();
    }

    public void ClearQuestGuidance()
    {
        selectedQuest = GuidedQuest.None;

        if (questMarker != null)
        {
            questMarker.Hide();
        }
    }

    private void UpdateQuestGuidance()
    {
        if (questMarker == null)
        {
            return;
        }

        switch (selectedQuest)
        {
            case GuidedQuest.AirDome:
                if (!airDomeBuilt && biomass < airDomeCost)
                {
                    ShowParasiteGuidance();
                }
                else
                {
                    questMarker.Hide();
                }
                break;

            case GuidedQuest.Workshop:
                if (airDomeBuilt && !workshopRepaired)
                {
                    ShowResourceGuidance(
                        workshopOreCost,
                        workshopEnergyCost
                    );
                }
                else
                {
                    questMarker.Hide();
                }
                break;

            case GuidedQuest.Mining:
                if (workshopRepaired && !miningStarted)
                {
                    ShowResourceGuidance(
                        miningOreCost,
                        miningEnergyCost
                    );
                }
                else
                {
                    questMarker.Hide();
                }
                break;

            case GuidedQuest.MiningUpgrade:
                if (miningStarted &&
                    miningLevel < MaxMiningLevel &&
                    ore < MiningUpgradeCost)
                {
                    questMarker.Show(oreGuidanceTarget);
                }
                else
                {
                    questMarker.Hide();
                }
                break;

            default:
                questMarker.Hide();
                break;
        }
    }

    private void ShowResourceGuidance(int requiredOre, int requiredEnergy)
    {
        if (ore < requiredOre)
        {
            questMarker.Show(oreGuidanceTarget);
        }
        else if (energy < requiredEnergy)
        {
            questMarker.Show(energyGuidanceTarget);
        }
        else
        {
            questMarker.Hide();
        }
    }

    private void ShowParasiteGuidance()
    {
        Camera camera = Camera.main;

        if (camera == null)
        {
            questMarker.Hide();
            return;
        }

        ParasiteClickable[] parasites =
            FindObjectsByType<ParasiteClickable>(FindObjectsSortMode.None);

        ParasiteClickable nearest = null;
        float nearestDistance = float.MaxValue;

        foreach (ParasiteClickable parasite in parasites)
        {
            if (!parasite.IsAvailable)
            {
                continue;
            }

            Vector3 position = parasite.transform.position;
            Vector3 viewport = camera.WorldToViewportPoint(position);

            
            if (viewport.z <= 0f ||
                viewport.x < 0.05f || viewport.x > 0.95f ||
                viewport.y < 0.05f || viewport.y > 0.95f)
            {
                continue;
            }

            float distance =
                (position - camera.transform.position).sqrMagnitude;

            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearest = parasite;
            }
        }

        if (nearest != null)
        {
            questMarker.Show(nearest.transform);
        }
        else
        {
            questMarker.Hide();
        }
    }


}