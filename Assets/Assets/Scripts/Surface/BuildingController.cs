using UnityEngine;

// Visual state only. Progress, saving and gameplay effects belong to other systems.
[DisallowMultipleComponent]
public class BuildingController : MonoBehaviour
{
    [SerializeField] private GameObject unbuiltVisual;
    [SerializeField] private GameObject builtRoot;
    public bool IsBuilt { get; private set; }

    private void Awake()
    {
        // The progression owner supplies the actual state after initialization.
        SetBuilt(false);
    }

    public void SetBuilt(bool built)
    {
        IsBuilt = built;
        SetChildActive(unbuiltVisual, !built);
        SetChildActive(builtRoot, built);
    }

    private void SetChildActive(GameObject child, bool active)
    {
        if (child == null) return;
        if (child == gameObject || !child.transform.IsChildOf(transform))
        {
            Debug.LogError("BuildingController: Assign only children of this building.", this);
            return;
        }
        if (child.activeSelf != active) child.SetActive(active);
    }
}
