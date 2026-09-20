using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ArtifactLoadoutHUD : MonoBehaviour
{
    [Header("Slot Images")]
    [SerializeField] private Image[] artifactImages;

    [Header("Artifact Sprites")]
    [SerializeField]
    private List<ArtifactSpriteEntry> artifactSprites =
        new List<ArtifactSpriteEntry>();

    private void Start()
    {
        RefreshHUD();
    }

    public void RefreshHUD()
    {
        if (ArtifactManager.Instance == null)
        {
            ClearAllSlots();
            return;
        }

        IReadOnlyList<ArtifactType> equipped =
            ArtifactManager.Instance.GetEquippedArtifacts();

        for (int i = 0; i < artifactImages.Length; i++)
        {
            Image image =
                artifactImages[i];

            if (image == null)
                continue;

            ArtifactType type =
                ArtifactType.None;

            if (i < equipped.Count)
            {
                type =
                    equipped[i];
            }

            if (type == ArtifactType.None)
            {
                image.sprite = null;
                image.enabled = false;
                continue;
            }

            Sprite sprite =
                GetArtifactSprite(
                    type
                );

            image.sprite =
                sprite;

            image.enabled =
                sprite != null;

            image.preserveAspect =
                true;
        }
    }

    private Sprite GetArtifactSprite(
        ArtifactType type)
    {
        foreach (
            ArtifactSpriteEntry entry
            in artifactSprites)
        {
            if (entry != null &&
                entry.type == type)
            {
                return entry.sprite;
            }
        }

        return null;
    }

    private void ClearAllSlots()
    {
        foreach (Image image in artifactImages)
        {
            if (image == null)
                continue;

            image.sprite = null;
            image.enabled = false;
        }
    }
}