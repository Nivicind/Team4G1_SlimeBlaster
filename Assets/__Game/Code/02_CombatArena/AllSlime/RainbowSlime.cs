using System.Collections.Generic;
using UnityEngine;

public class RainbowSlime : Enemy
{
    [System.Serializable]
    public struct RainbowCurrencyDrop
    {
        public EnumCurrency currencyType;
        public int baseAmount;
    }

    [Header("Rainbow Currency Drops")]
    public List<RainbowCurrencyDrop> possibleDrops = new List<RainbowCurrencyDrop>();

    protected override void Die()
    {
        slimeAnim.PlayDeathAnimation(() =>
        {
            GiveExpToPlayer();
            SpawnRandomCurrency();
            ReturnToPool();
        });

    }

    private void SpawnRandomCurrency()
    {
        if (currencyReference == null || possibleDrops.Count == 0)
            return;

        // Pick 1 random currency entry
        RainbowCurrencyDrop selectedDrop =
            possibleDrops[Random.Range(0, possibleDrops.Count)];

        ObjectPool selectedPool =
            currencyReference.GetCurrencyPool(selectedDrop.currencyType);

        if (selectedPool == null)
            return;

        int stage = Stage.Instance.GetStage();
        int totalAmount = selectedDrop.baseAmount * stage;

        // Get collider bounds
        Collider2D col = GetComponent<Collider2D>();

        for (int i = 0; i < totalAmount; i++)
        {
            Vector3 spawnPos;

            if (col != null)
            {
                // Spawn random position inside collider bounds
                Bounds bounds = col.bounds;
                spawnPos = new Vector3(
                    Random.Range(bounds.min.x, bounds.max.x),
                    Random.Range(bounds.min.y, bounds.max.y),
                    transform.position.z
                );
            }
            else
            {
                // Fallback if no collider
                spawnPos = transform.position + (Vector3)Random.insideUnitCircle * 0.5f;
            }

            GameObject currencyObj =
                selectedPool.Get(spawnPos, Quaternion.identity);

            CurrencyControl currencyControl =
                currencyObj.GetComponent<CurrencyControl>();

            if (currencyControl != null)
                currencyControl.pool = selectedPool;
        }
    }
}
