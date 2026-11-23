using UnityEngine;

public class QuestMarker : MonoBehaviour
{
    [SerializeField] private Camera worldCamera;
    [SerializeField] private float worldHeight = 2f;
    [SerializeField] private float bounceHeight = 12f;
    [SerializeField] private float bounceSpeed = 4f;

    private RectTransform markerRect;
    private RectTransform parentRect;
    private Canvas rootCanvas;
    private Transform target;

    private void Awake()
    {
        markerRect = GetComponent<RectTransform>();
        parentRect = transform.parent as RectTransform;
        rootCanvas = GetComponentInParent<Canvas>().rootCanvas;

        if (worldCamera == null)
        {
            worldCamera = Camera.main;
        }

        Hide();
    }

    public void Show(Transform newTarget)
    {
        if (newTarget == null || worldCamera == null)
        {
            Hide();
            return;
        }

        target = newTarget;
        gameObject.SetActive(true);
        UpdatePosition();
    }

    public void Hide()
    {
        target = null;
        gameObject.SetActive(false);
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            Hide();
            return;
        }

        UpdatePosition();
    }

    private void UpdatePosition()
    {
        Vector3 worldPosition =
            target.position + Vector3.up * worldHeight;

        Vector3 screenPosition =
            worldCamera.WorldToScreenPoint(worldPosition);

        bool visible = screenPosition.z > 0f;

        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(visible);
        }

        if (!visible)
        {
            return;
        }

        Camera uiCamera =
            rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay
                ? null
                : rootCanvas.worldCamera;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentRect,
            screenPosition,
            uiCamera,
            out Vector2 localPosition))
        {
            localPosition.y +=
                Mathf.Sin(Time.unscaledTime * bounceSpeed) * bounceHeight;

            markerRect.localPosition = new Vector3(
                localPosition.x,
                localPosition.y,
                0f
            );
        }
    }
}