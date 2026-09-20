using TMPro;
using UnityEngine;

public class HUDController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private LevelSystem levelSystem;
    [SerializeField] private MineGenerator mineGenerator;

    [Header("HUD Texts")]
    [SerializeField] private TMP_Text energyText;
    [SerializeField] private TMP_Text runGoldText;
    [SerializeField] private TMP_Text bankGoldText;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text xpText;
    [SerializeField] private TMP_Text luckText;
    [SerializeField] private TMP_Text floorText;

    private void Update()
    {
        UpdatePlayerStats();
        UpdateProgression();
        UpdateCurrency();
        UpdateFloor();
    }

    // =========================================================
    // PLAYER STATS
    // =========================================================

    private void UpdatePlayerStats()
    {
        if (playerStats == null)
            return;

        if (energyText != null)
        {
            energyText.text =
                $"Energy: {playerStats.CurrentEnergy:0} / {playerStats.MaxEnergy:0}";
        }

        if (luckText != null)
        {
            luckText.text =
                $"Luck: {playerStats.Luck:0}";
        }
    }

    // =========================================================
    // LEVEL / XP
    // =========================================================

    private void UpdateProgression()
    {
        if (levelSystem == null)
            return;

        if (levelText != null)
        {
            levelText.text =
                $"Level: {levelSystem.CurrentLevel}";
        }

        if (xpText != null)
        {
            xpText.text =
                $"XP: {levelSystem.CurrentXP} / {levelSystem.XPToNextLevel}";
        }
    }

    // =========================================================
    // GOLD
    // =========================================================

    private void UpdateCurrency()
    {
        if (RunManager.Instance == null)
            return;

        if (runGoldText != null)
        {
            runGoldText.text =
                $"Run Gold: {RunManager.Instance.RunGold}";
        }

        if (bankGoldText != null)
        {
            bankGoldText.text =
                $"Bank: {RunManager.Instance.BankGold}";
        }
    }

    // =========================================================
    // FLOOR
    // =========================================================

    private void UpdateFloor()
    {
        if (mineGenerator == null)
            return;

        if (floorText != null)
        {
            floorText.text =
                $"Floor: {mineGenerator.CurrentFloor}";
        }
    }
}