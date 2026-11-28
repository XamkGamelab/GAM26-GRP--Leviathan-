using System;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

public class ExperimentManager : MonoBehaviour
{
    public static ExperimentManager Instance { get; private set; }

    public const string ExperimentId = "mining_upgrade_cost_v1";

    private const string PlayerIdKey = "Leviathan_PlayerId";
    private const string GroupKey = "Leviathan_MiningExperimentGroup";

    public string PlayerId { get; private set; }
    public string Group { get; private set; }

    public bool IsNewPlayer { get; private set; }
    public bool IsNewAssignment { get; private set; }

    public float MiningCostMultiplier => Group == "B" ? 1.15f : 1.25f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitializePlayer();
        InitializeExperiment();

        PlayerPrefs.Save();
        PrintStatus();
    }

    private void InitializePlayer()
    {
        PlayerId = PlayerPrefs.GetString(PlayerIdKey, "");

        IsNewPlayer = string.IsNullOrEmpty(PlayerId);

        if (IsNewPlayer)
        {
            PlayerId = Guid.NewGuid().ToString("N");
            PlayerPrefs.SetString(PlayerIdKey, PlayerId);
        }
    }

    private void InitializeExperiment()
    {
        Group = PlayerPrefs.GetString(GroupKey, "");

        IsNewAssignment = Group != "A" && Group != "B";

        if (IsNewAssignment)
        {
            Group = AssignGroup(PlayerId);
            PlayerPrefs.SetString(GroupKey, Group);
        }
    }

    private string AssignGroup(string playerId)
    {
        using (SHA256 sha = SHA256.Create())
        {
            byte[] input = Encoding.UTF8.GetBytes(
                ExperimentId + ":" + playerId
            );

            byte[] hash = sha.ComputeHash(input);

            return (hash[0] & 1) == 0 ? "A" : "B";
        }
    }

    [ContextMenu("Debug/Print experiment status")]
    public void PrintStatus()
    {
        Debug.Log(
            $"Player ID: {PlayerId}\n" +
            $"Experiment: {ExperimentId}\n" +
            $"Group: {Group}\n" +
            $"Assignment: {(IsNewAssignment ? "NEW" : "LOADED")}\n" +
            $"Mining cost multiplier: {MiningCostMultiplier}",
            this
        );
    }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    [ContextMenu("Debug/Use Group A on next launch")]
    private void UseGroupA()
    {
        SaveDebugGroup("A");
    }

    [ContextMenu("Debug/Use Group B on next launch")]
    private void UseGroupB()
    {
        SaveDebugGroup("B");
    }

    private void SaveDebugGroup(string group)
    {
        PlayerPrefs.SetString(GroupKey, group);
        PlayerPrefs.Save();

        Debug.Log($"Group {group} saved. Restart Play Mode.");
    }

    [ContextMenu("Debug/Reset assignment for next launch")]
    private void ResetAssignment()
    {
        PlayerPrefs.DeleteKey(GroupKey);
        PlayerPrefs.Save();

        Debug.Log(
            "Assignment reset. Restart Play Mode. " +
            "The group will be calculated from the existing Player ID."
        );
    }
#endif

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}