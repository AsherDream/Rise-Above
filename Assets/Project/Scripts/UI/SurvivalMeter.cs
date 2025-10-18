using UnityEngine;
using UnityEngine.UI;

// Make sure the filename is SurvivalMeter.cs and the class name is SurvivalMeter
public class SurvivalMeter : MonoBehaviour
{
    [Header("UI Components")]
    public Slider hpSlider;   // drag the HP_Slider here
    public Text hpText;       // drag HP_Text here (use TextMeshProUGUI if using TMP - see note)

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
    }

    // PUBLIC method so it shows up in the Button -> OnClick dropdown
    public void OnGoodItemClick()
    {
        currentHP += goodItemHeal;
        if (currentHP > maxHP) currentHP = maxHP;
        UpdateUI();
        Debug.Log("Good item clicked. HP = " + currentHP);
    }

    // PUBLIC method so it shows up in the Button -> OnClick dropdown
    public void OnBadItemClick()
    {
        currentHP -= badItemDamage;
        if (currentHP < minHP) currentHP = minHP;
        UpdateUI();
        Debug.Log("Bad item clicked. HP = " + currentHP);
    }

    void UpdateUI()
    {
        if (hpSlider != null) hpSlider.value = (float)currentHP / (float)maxHP;
        if (hpText != null) hpText.text = currentHP + " / " + maxHP;
    }
}
