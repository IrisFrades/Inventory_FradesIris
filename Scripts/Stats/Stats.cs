using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Stats : MonoBehaviour
{
    public TextMeshProUGUI levelText;      // ← Arrastra LevelText aquí
    public TextMeshProUGUI strengthText;   // ← Arrastra StrengthText aquí

    private int currentUserID = -1;

    public void SetUserID(int userID)
    {
        currentUserID = userID;
        UpdateStatsUI();
    }

    public void UpdateStatsUI()
    {
        if (currentUserID == -1) return;

        var (strength, level) = DBManager.Instance.LoadUserStats(currentUserID);
        strengthText.text = $"Fuerza: {strength}";
        levelText.text = $"Nivel: {level}";
    }
}
