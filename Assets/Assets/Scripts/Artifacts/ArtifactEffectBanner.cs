using System.Collections;
using TMPro;
using UnityEngine;

public class ArtifactEffectBanner : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Text effectNameText;
    [SerializeField] private TMP_Text effectDescriptionText;

    [Header("Timing")]
    [SerializeField] private float displayDuration = 2.5f;

    private Coroutine currentRoutine;

    private void Awake()
    {
        if (panel != null)
        {
            panel.SetActive(false);
        }
    }

    public void Show(
        string effectName,
        string description)
    {
        if (currentRoutine != null)
        {
            StopCoroutine(currentRoutine);
        }

        currentRoutine =
            StartCoroutine(
                ShowRoutine(
                    effectName,
                    description
                )
            );
    }

    private IEnumerator ShowRoutine(
        string effectName,
        string description)
    {
        if (panel == null)
            yield break;

        if (effectNameText != null)
        {
            effectNameText.text =
                effectName;
        }

        if (effectDescriptionText != null)
        {
            effectDescriptionText.text =
                description;
        }

        panel.SetActive(true);

        yield return new WaitForSeconds(
            displayDuration
        );

        panel.SetActive(false);

        currentRoutine = null;
    }
}