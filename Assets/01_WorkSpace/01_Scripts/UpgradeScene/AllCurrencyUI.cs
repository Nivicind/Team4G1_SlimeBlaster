using UnityEngine;
using TMPro;

public class AllCurrencyUI : MonoBehaviour
{
    [Header("References")]
    public PlayerStats playerStats;

    [Header("TextMeshPro UI")]
    public TextMeshProUGUI blueBitsText;
    public TextMeshProUGUI pinkBitsText;
    public TextMeshProUGUI redBitsText;


    void Update()
    {
        if (playerStats == null) return;

        // Update all three currency text fields
        if (redBitsText != null)
            redBitsText.text = playerStats.redBits.ToString();
        
        if (blueBitsText != null)
            blueBitsText.text = playerStats.blueBits.ToString();
        
        if (pinkBitsText != null)
            pinkBitsText.text = playerStats.pinkBits.ToString();
    }
}
