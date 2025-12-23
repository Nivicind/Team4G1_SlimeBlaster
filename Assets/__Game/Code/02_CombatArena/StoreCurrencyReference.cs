using UnityEngine;

public class StoreCurrencyReference : MonoBehaviour
{
    [Header("Currency Pools")]
    public ObjectPool blueBitsCurrencyPool;
    public ObjectPool pinkBitsCurrencyPool;
    public ObjectPool yellowBitsCurrencyPool;
    public ObjectPool greenBitsCurrencyPool;

    public ObjectPool GetCurrencyPool(EnumCurrency currencyType)
    {
        switch (currencyType)
        {
            case EnumCurrency.yellowBits:
                return yellowBitsCurrencyPool;
            case EnumCurrency.blueBits:
                return blueBitsCurrencyPool;
            case EnumCurrency.pinkBits:
                return pinkBitsCurrencyPool;
            case EnumCurrency.greenBits:
                return greenBitsCurrencyPool;
            default:
                return null;
        }
    }
}
