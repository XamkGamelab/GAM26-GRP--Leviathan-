using UnityEngine;
using UnityEngine.EventSystems;

public class ResourceSourceClickable : MonoBehaviour
{
    public enum ResourceType
    {
        Ore,
        Energy
    }

    [SerializeField] private ResourceManager resourceManager;
    [SerializeField] private ResourceType resourceType;

    private void OnMouseDown()
    {
        if (resourceManager == null || Time.timeScale == 0f)
        {
            return;
        }

        if (EventSystem.current != null &&
            EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        switch (resourceType)
        {
            case ResourceType.Ore:
                resourceManager.SalvageOre();
                break;

            case ResourceType.Energy:
                resourceManager.RecoverEnergy();
                break;
        }
    }
}