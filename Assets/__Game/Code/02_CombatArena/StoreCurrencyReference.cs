using UnityEngine;

public class StoreCurrencyReference : MonoBehaviour
{
    [Header("Currency Pools")]
    public ObjectPool blueBitsCurrencyPool;
    public ObjectPool pinkBitsCurrencyPool;
    public ObjectPool redBitsCurrencyPool;

    public ObjectPool GetCurrencyPool(EnumCurrency currencyType)
    {
        switch (currencyType)
        {
            case EnumCurrency.redBits:
                return redBitsCurrencyPool;
            case EnumCurrency.blueBits:
                return blueBitsCurrencyPool;
            case EnumCurrency.pinkBits:
                return pinkBitsCurrencyPool;
            default:
                return null;
        }
    }
}
