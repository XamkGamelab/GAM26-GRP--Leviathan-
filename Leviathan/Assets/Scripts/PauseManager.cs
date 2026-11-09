using UnityEngine;

public class PauseManager : MonoBehaviour
{
    [SerializeField] private GameObject saveButton;
    [SerializeField] private GameObject loadButton;

    private bool isPaused;

    private void Start()
    {
        SetPaused(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SetPaused(!isPaused);
        }
    }

    private void SetPaused(bool paused)
    {
        isPaused = paused;
        Time.timeScale = isPaused ? 0f : 1f;

        saveButton.SetActive(isPaused);
        loadButton.SetActive(isPaused);
    }

    private void OnDestroy()
    {
        Time.timeScale = 1f;
    }
}