using System.Collections;
using UnityEngine;

public class ParasiteClickable : MonoBehaviour
{
    [SerializeField] private ResourceManager resourceManager;
    [SerializeField] private Transform spawnCenter;
    [SerializeField] private float spawnRadius = 8f;
    [SerializeField] private float surfaceHeight = 0.4f;
    [SerializeField] private float respawnDelay = 3f;

    private Renderer parasiteRenderer;
    private Collider parasiteCollider;
    private bool isAvailable = true;

    private void Awake()
    {
        parasiteRenderer = GetComponent<Renderer>();
        parasiteCollider = GetComponent<Collider>();
    }

    private void Start()
    {
        MoveToRandomPosition();
    }

    private void OnMouseDown()
    {
        if (!isAvailable || resourceManager == null)
            return;

        resourceManager.CollectBiomass();
        StartCoroutine(HideAndRespawn());
    }

    private IEnumerator HideAndRespawn()
    {
        isAvailable = false;
        parasiteRenderer.enabled = false;
        parasiteCollider.enabled = false;

        yield return new WaitForSeconds(respawnDelay);

        MoveToRandomPosition();

        parasiteRenderer.enabled = true;
        parasiteCollider.enabled = true;
        isAvailable = true;
    }

    private void MoveToRandomPosition()
    {
        Vector2 offset = Random.insideUnitCircle * spawnRadius;

        transform.position = new Vector3(
            spawnCenter.position.x + offset.x,
            spawnCenter.position.y + surfaceHeight,
            spawnCenter.position.z + offset.y
        );
    }
}