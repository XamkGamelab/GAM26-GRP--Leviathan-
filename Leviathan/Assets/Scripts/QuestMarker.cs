using UnityEngine;

public class QuestMarker : MonoBehaviour
{
    [SerializeField] private Camera worldCamera;
    [SerializeField] private float worldHeight = 2f;
    [SerializeField] private float uiHeight = 45f;
    [SerializeField] private float bounceHeight = 12f;
    [SerializeField] private float bounceSpeed = 4f;

    private RectTransform markerRect;
    private RectTransform parentRect;
    private Canvas rootCanvas;

    private Transform worldTarget;
    private RectTransform uiTarget;
    private bool initialized;

    private void Awake()
    {
        Initialize();

        if (worldTarget == null && uiTarget == null)
        {
            Hide();
        }
    }

    private void Initialize()
    {
        if (initialized)
        {
            return;
        }

        markerRect = GetComponent<RectTransform>();
        parentRect = transform.parent as RectTransform;
        rootCanvas = GetComponentInParent<Canvas>().rootCanvas;

        if (worldCamera == null)
        {
            worldCamera = Camera.main;
        }

        initialized = true;
    }

    public void Show(Transform newTarget)
    {
        Initialize();

        if (newTarget == null || worldCamera == null)
        {
            Hide();
            return;
        }

        worldTarget = newTarget;
        uiTarget = null;

        gameObject.SetActive(true);
        UpdatePosition();
    }

    public void ShowUI(RectTransform newTarget)
    {
        Initialize();

        if (newTarget == null)
        {
            Hide();
            return;
        }

        worldTarget = null;
        uiTarget = newTarget;

        gameObject.SetActive(true);
        UpdatePosition();
    }

    public void Hide()
    {
        worldTarget = null;
        uiTarget = null;
        gameObject.SetActive(false);
    }

    private void LateUpdate()
    {
        if (worldTarget == null && uiTarget == null)
        {
            Hide();
            return;
        }

        UpdatePosition();
    }

    private void UpdatePosition()
    {
        Camera markerCamera =
            rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay
                ? null
                : rootCanvas.worldCamera;

        Vector2 screenPosition;
        float extraHeight = 0f;

        if (uiTarget != null)
        {
            Canvas targetCanvas =
                uiTarget.GetComponentInParent<Canvas>().rootCanvas;

            Camera targetCamera =
                targetCanvas.renderMode == RenderMode.ScreenSpaceOverlay
                    ? null
                    : targetCanvas.worldCamera;

            Vector3 targetCenter =
                uiTarget.TransformPoint(uiTarget.rect.center);

            screenPosition = RectTransformUtility.WorldToScreenPoint(
                targetCamera,
                targetCenter
            );

            SetArrowVisible(uiTarget.gameObject.activeInHierarchy);
            extraHeight = uiHeight;
        }
        else
        {
            Vector3 position = worldCamera.WorldToScreenPoint(
                GetWorldTargetPosition()
            );

            SetArrowVisible(position.z > 0f);

            if (position.z <= 0f)
            {
                return;
            }

            screenPosition = new Vector2(position.x, position.y);
            extraHeight = uiHeight;
        }

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentRect,
            screenPosition,
            markerCamera,
            out Vector2 localPosition))
        {
            localPosition.y += extraHeight +
                Mathf.Sin(Time.unscaledTime * bounceSpeed) * bounceHeight;

            markerRect.localPosition = new Vector3(
                localPosition.x,
                localPosition.y,
                0f
            );
        }
    }

    private void SetArrowVisible(bool visible)
    {
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(visible);
        }
    }

    private Vector3 GetWorldTargetPosition()
    {
        Renderer[] renderers =
            worldTarget.GetComponentsInChildren<Renderer>();

        if (renderers.Length == 0)
        {
            return worldTarget.position;
        }

        Bounds bounds = renderers[0].bounds;

        for (int i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }

        return new Vector3(
            bounds.center.x,
            bounds.max.y,
            bounds.center.z
        );
    }
}