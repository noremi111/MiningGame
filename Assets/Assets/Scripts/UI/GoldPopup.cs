using TMPro;
using UnityEngine;

public class GoldPopup : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TMP_Text popupText;

    [Header("Animation")]
    [SerializeField] private float lifetime = 0.9f;
    [SerializeField] private float moveSpeed = 0.7f;
    [SerializeField] private float startScale = 0.8f;
    [SerializeField] private float peakScale = 1.15f;

    private float timer;
    private Color startColor;

    private void Awake()
    {
        if (popupText == null)
            popupText = GetComponent<TMP_Text>();

        startColor = popupText.color;

        transform.localScale = Vector3.one * startScale;
    }

    private void Update()
    {
        timer += Time.deltaTime;

        float t = timer / lifetime;

        // Nach oben schweben
        transform.position += Vector3.up * moveSpeed * Time.deltaTime;

        // Kurzer Scale-Punch
        float scale;

        if (t < 0.25f)
        {
            float scaleT = t / 0.25f;
            scale = Mathf.Lerp(startScale, peakScale, scaleT);
        }
        else
        {
            float scaleT = (t - 0.25f) / 0.75f;
            scale = Mathf.Lerp(peakScale, 1f, scaleT);
        }

        transform.localScale = Vector3.one * scale;

        // Ausblenden
        Color color = startColor;
        color.a = 1f - t;

        popupText.color = color;

        if (timer >= lifetime)
        {
            Destroy(gameObject);
        }
    }

    public void Setup(int goldAmount)
    {
        popupText.text = "+" + goldAmount;

        if (goldAmount >= 100)
        {
            peakScale = 1.5f;
        }
        else if (goldAmount >= 25)
        {
            peakScale = 1.3f;
        }
    }
}