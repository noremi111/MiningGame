using TMPro;
using UnityEngine;

// Auf ein aktives HUD-Objekt setzen. Textfelder dürfen Kinder sein.
public class RunTimerUI : MonoBehaviour
{
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text warningText;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color warningColor = new Color(1f, 0.7f, 0.15f);
    [SerializeField] private Color urgentColor = new Color(1f, 0.25f, 0.2f);

    private void LateUpdate()
    {
        RunManager run = RunManager.Instance;
        bool visible = run != null && run.RunActive && run.TimeLimitEnabled;
        if (timerText != null) timerText.enabled = visible;
        if (warningText != null) warningText.enabled = visible && run.RemainingSeconds <= 60f;
        if (!visible) return;

        int seconds = Mathf.CeilToInt(run.RemainingSeconds);
        Color color = run.RemainingSeconds <= 30f ? urgentColor :
            run.RemainingSeconds <= 60f ? warningColor : normalColor;
        if (timerText != null)
        {
            timerText.text = $"{seconds / 60:00}:{seconds % 60:00}";
            timerText.color = color;
        }
        if (warningText != null)
        {
            warningText.text = run.RemainingSeconds <= 30f ?
                "Time is running out! Return to the surface!" :
                "Less than a minute left!";
            warningText.color = color;
        }
    }
}
