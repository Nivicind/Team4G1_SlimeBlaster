using UnityEngine;

[CreateAssetMenu(fileName = "NodeData", menuName = "Game/Node Data")]
public class SONodeData : ScriptableObject
{
    [Header("Display Info")]
    public string upgradeName;
    [TextArea] public string description;
    public Sprite icon;

    [Header("Stats")]
    public EnumStat stat;          // Type-safe dropdown for stats
    public int perUpgradeValue;    // How much this upgrade adds per level
    public int maxLevel = 10;      // Max threshold

    [Header("Cost Formula")]
    [Tooltip("Write formula using n for level, e.g. '1+1*(n-1)'")]
    public string costFormula = "1+1*(n-1)";
    public EnumCurrency costUnit;  // Type-safe dropdown for currency


    [Header("Upgrade Information")]
    public string description1;
    public string description2;
}
