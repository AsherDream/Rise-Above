using UnityEngine;
using UnityEngine.UI;
using TMPro; // ✅ Add this for TextMeshPro

public class SurvivalMeter : MonoBehaviour
{
    [Header("UI Components")]
    public Slider hpSlider;
    public TMP_Text hpText; // ✅ Changed from Text → TMP_Text

    [Header("HP Settings")]
    public int maxHP = 100;
    public int minHP = 0;
    public int goodItemHeal = 10;
    public int badItemDamage = 15;

    private int currentHP;

    void Start()
    {
        currentHP = maxHP;
        UpdateUI();
        Debug.Log("SurvivalMeter started. CurrentHP = " + currentHP);
    }

    public void OnGoodItemClick()
    {
        currentHP += goodItemHeal;
        if (currentHP > maxHP)
            currentHP = maxHP;

        UpdateUI();
        Debug.Log("Good item clicked! HP: " + currentHP);
    }

    public void OnBadItemClick()
    {
        currentHP -= badItemDamage;
        if (currentHP < minHP)
            currentHP = minHP;

        UpdateUI();
        Debug.Log("Bad item clicked! HP: " + currentHP);
    }

    void UpdateUI()
    {
        hpSlider.value = (float)currentHP / maxHP;

        if (hpText != null)
            hpText.text = $"{currentHP} / {maxHP}";
    }
}
