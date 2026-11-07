using TMPro;
using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    [SerializeField] private TMP_Text biomassText;
    [SerializeField] private TMP_Text oreText;
    [SerializeField] private TMP_Text energyText;

    private int biomass;
    private int ore;
    private int energy;

    private void Start()
    {
        UpdateUI();
    }

    public void CollectBiomass()
    {
        biomass++;
        UpdateUI();
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

    private void UpdateUI()
    {
        biomassText.text = $"Biomass: {biomass}";
        oreText.text = $"Ore: {ore}";
        energyText.text = $"Energy: {energy}";
    }
}