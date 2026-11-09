using UnityEngine;

public class BiomassCollectorRobot : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float collectionDistance = 0.8f;
    [SerializeField] private float targetSearchInterval = 0.3f;

    private ParasiteClickable target;
    private float searchTimer;

    private void Update()
    {
        if (target == null || !target.IsAvailable)
        {
            searchTimer -= Time.deltaTime;

            if (searchTimer <= 0f)
            {
                FindNearestParasite();
                searchTimer = targetSearchInterval;
            }
        }

        if (target == null)
        {
            return;
        }

        Vector3 targetPosition = target.transform.position;
        targetPosition.y = transform.position.y;

        Vector3 direction = targetPosition - transform.position;

        if (direction.sqrMagnitude <= collectionDistance * collectionDistance)
        {
            target.TryCollect();
            target = null;
            return;
        }

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            moveSpeed * Time.deltaTime
        );

        if (direction.sqrMagnitude > 0.001f)
        {
            transform.forward = direction.normalized;
        }
    }

    private void FindNearestParasite()
    {
        ParasiteClickable[] parasites =
            FindObjectsByType<ParasiteClickable>(FindObjectsSortMode.None);

        float nearestDistance = float.MaxValue;
        target = null;

        foreach (ParasiteClickable parasite in parasites)
        {
            if (!parasite.IsAvailable)
            {
                continue;
            }

            Vector3 difference = parasite.transform.position - transform.position;
            difference.y = 0f;
            float distance = difference.sqrMagnitude;

            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                target = parasite;
            }
        }
    }
}