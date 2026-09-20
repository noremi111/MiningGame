using UnityEngine;

// Auf einem aktiven Elternobjekt platzieren, außerhalb von towerRoot.
public class MageTowerUnlock : MonoBehaviour
{
    [SerializeField, Tooltip("Kompletter Turm inklusive Grafik, Collider und Interaktions-Script.")]
    private GameObject towerRoot;
    [SerializeField, Tooltip("Optional: Baustelle, solange der Turm gesperrt ist.")]
    private GameObject constructionVisual;

    private void Awake()
    {
        Refresh();
    }

    private void Update()
    {
        Refresh();
    }

    private void Refresh()
    {
        bool unlocked = RunManager.Instance != null && RunManager.Instance.MageTowerUnlocked;
        SetVisible(towerRoot, unlocked);
        SetVisible(constructionVisual, !unlocked);
    }

    private void SetVisible(GameObject target, bool visible)
    {
        // Controller und seine Eltern dürfen sich nicht selbst deaktivieren.
        if (target == null || transform.IsChildOf(target.transform)) return;
        if (target.activeSelf != visible) target.SetActive(visible);
    }
}
