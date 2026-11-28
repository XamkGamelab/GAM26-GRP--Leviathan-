using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class AnalyticsManager : MonoBehaviour
{
    public static AnalyticsManager Instance { get; private set; }

    private const float EarningsInterval = 10f;

    private ExperimentManager experiment;
    private string sessionId;
    private string filePath;

    private bool initialized;
    private bool sessionEnded;
    private float sessionStartedAt;
    private float earningsTimer;

    private readonly Dictionary<string, int> pendingEarnings =
        new Dictionary<string, int>();

    [Serializable]
    private class AnalyticsEvent
    {
        public string event_name;
        public string player_id;
        public string session_id;
        public string timestamp;
        public string experiment_id;
        public string experiment_group;

        public string upgrade_id;
        public string currency_type;
        public int cost;
        public int level;
        public int production_after;

        public string milestone_id;

        public string source;
        public int amount;
        public float interval_seconds;

        public float session_duration_seconds;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        experiment = ExperimentManager.Instance;

        if (experiment == null)
        {
            Debug.LogError(
                "Analytics requires ExperimentManager.",
                this
            );
            return;
        }

        sessionId = Guid.NewGuid().ToString("N");
        sessionStartedAt = Time.realtimeSinceStartup;

        filePath = Path.Combine(
            Application.persistentDataPath,
            "analytics.jsonl"
        );

        initialized = true;

        if (experiment.IsNewPlayer)
        {
            RecordEvent("player_created");
        }

        if (experiment.IsNewAssignment)
        {
            RecordEvent("experiment_started");
        }

        RecordEvent("session_started");

        Debug.Log($"Analytics file: {filePath}", this);
    }

    public void RecordEvent(
        string eventName,
        string upgradeId = "",
        string currencyType = "",
        int cost = 0,
        int level = 0,
        int productionAfter = 0,
        string milestoneId = "",
        string source = "",
        int amount = 0,
        float intervalSeconds = 0f,
        float sessionDurationSeconds = 0f)
    {
        if (!initialized)
        {
            return;
        }

        AnalyticsEvent entry = new AnalyticsEvent
        {
            event_name = eventName,
            player_id = experiment.PlayerId,
            session_id = sessionId,
            timestamp = DateTime.UtcNow.ToString("o"),
            experiment_id = ExperimentManager.ExperimentId,
            experiment_group = experiment.Group,

            upgrade_id = upgradeId,
            currency_type = currencyType,
            cost = cost,
            level = level,
            production_after = productionAfter,

            milestone_id = milestoneId,

            source = source,
            amount = amount,
            interval_seconds = intervalSeconds,

            session_duration_seconds = sessionDurationSeconds
        };

        string json = JsonUtility.ToJson(entry);

        try
        {
            File.AppendAllText(
                filePath,
                json + Environment.NewLine
            );
        }
        catch (Exception exception)
        {
            Debug.LogError(
                $"Could not write analytics: {exception.Message}",
                this
            );
        }
    }

    public void TrackCurrencyEarned(
        string currencyType,
        int amount,
        string source)
    {
        if (!initialized || sessionEnded || amount <= 0)
        {
            return;
        }

        string key = currencyType + "|" + source;

        if (!pendingEarnings.ContainsKey(key))
        {
            pendingEarnings[key] = 0;
        }

        pendingEarnings[key] += amount;
    }

    private void Update()
    {
        if (!initialized || sessionEnded)
        {
            return;
        }

        earningsTimer += Time.unscaledDeltaTime;

        if (earningsTimer >= EarningsInterval)
        {
            FlushEarnings();
        }
    }

    private void FlushEarnings()
    {
        if (!initialized)
        {
            return;
        }

        foreach (KeyValuePair<string, int> entry in pendingEarnings)
        {
            string[] parts = entry.Key.Split('|');

            RecordEvent(
                eventName: "currency_earned",
                currencyType: parts[0],
                source: parts[1],
                amount: entry.Value,
                intervalSeconds: earningsTimer
            );
        }

        pendingEarnings.Clear();
        earningsTimer = 0f;
    }

    private void OnApplicationPause(bool paused)
    {
        if (paused && !sessionEnded)
        {
            FlushEarnings();
        }
    }

    private void OnApplicationQuit()
    {
        if (!initialized || sessionEnded)
        {
            return;
        }

        sessionEnded = true;

        FlushEarnings();

        RecordEvent(
            eventName: "session_ended",
            sessionDurationSeconds:
                Time.realtimeSinceStartup - sessionStartedAt
        );
    }

    [ContextMenu("Debug/Print recorded events")]
    private void PrintRecordedEvents()
    {
        if (!initialized || !File.Exists(filePath))
        {
            Debug.Log(
                "No analytics file available yet.",
                this
            );
            return;
        }

        try
        {
            Debug.Log(File.ReadAllText(filePath), this);
        }
        catch (Exception exception)
        {
            Debug.LogError(
                $"Could not read analytics: {exception.Message}",
                this
            );
        }
    }

    [ContextMenu("Debug/Print analytics file path")]
    private void PrintFilePath()
    {
        Debug.Log(
            Path.Combine(
                Application.persistentDataPath,
                "analytics.jsonl"
            ),
            this
        );
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}