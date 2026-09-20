using UnityEngine;

public class LevelSystem : MonoBehaviour
{
    [Header("Level")]
    [SerializeField] private int currentLevel = 1;

    [Header("XP")]
    [SerializeField] private int currentXP = 0;
    [SerializeField] private int xpToNextLevel = 10;

    [Header("References")]
    [SerializeField] private PerkSelectionUI perkSelectionUI;

    public int CurrentLevel => currentLevel;
    public int CurrentXP => currentXP;
    public int XPToNextLevel => xpToNextLevel;

    public void AddXP(int amount)
    {
        currentXP += amount;

        while (currentXP >= xpToNextLevel)
        {
            LevelUp();
        }
    }

    private void LevelUp()
    {
        currentXP -= xpToNextLevel;

        currentLevel++;
        MiningAudio.Current?.Play(MiningAudio.Cue.LevelUp);

        xpToNextLevel = CalculateXPForNextLevel();

        if (perkSelectionUI != null)
        {
            perkSelectionUI.ShowPerks();
        }
    }

    private int CalculateXPForNextLevel()
    {
        return Mathf.RoundToInt(
            10 * Mathf.Pow(1.35f, currentLevel - 1)
        );
    }
}