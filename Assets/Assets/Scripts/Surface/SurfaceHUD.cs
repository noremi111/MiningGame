using TMPro;
using UnityEngine;

public class SurfaceHUD : MonoBehaviour
{
    [SerializeField] private TMP_Text bankGoldText;

    private void Update()
    {
        if (bankGoldText == null)
            return;

        if (RunManager.Instance == null)
        {
            bankGoldText.text = "Gold: 0";
            return;
        }

        bankGoldText.text =
            $"Gold: {RunManager.Instance.BankGold}";
    }
}