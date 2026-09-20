using TMPro;
using UnityEngine;

public class EffectPopup : MonoBehaviour
{
    [SerializeField] private TMP_Text popupText;

    [Header("Animation")]
    [SerializeField] private float lifetime = 1.1f;
    [SerializeField] private float moveSpeed = 0.8f;

    [SerializeField] private float startScale = 0.8f;
    [SerializeField] private float peakScale = 1.25f;

    private float timer;
    private Color startColor;

    private void Awake()
    {
        if (popupText != null)
        {
            startColor = popupText.color;
        }
    }

    public void Setup(string message)
    {
        if (popupText != null)
        {
            popupText.text = message;
        }

        timer = 0f;

        transform.localScale =
            Vector3.one * startScale;
    }

    private void Update()
    {
        timer += Time.deltaTime;

        transform.position +=
            Vector3.up *
            moveSpeed *
            Time.deltaTime;

        float normalized =
            Mathf.Clamp01(
                timer / lifetime
            );

        // Scale Punch
        float scale;

        if (normalized < 0.25f)
        {
            scale =
                Mathf.Lerp(
                    startScale,
                    peakScale,
                    normalized / 0.25f
                );
        }
        else
        {
            scale =
                Mathf.Lerp(
                    peakScale,
                    1f,
                    (normalized - 0.25f) / 0.75f
                );
        }

        transform.localScale =
            Vector3.one * scale;

        // Fade
        if (popupText != null)
        {
            Color color =
                startColor;

            color.a =
                1f - normalized;

            popupText.color =
                color;
        }

        if (timer >= lifetime)
        {
            Destroy(gameObject);
        }
    }
}