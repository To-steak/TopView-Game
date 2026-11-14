using TopView.Components;

namespace TopView.Utilities
{
    // 적 드롭 표에서 전리품을 뽑는 도구.
    public static class LootUtility
    {
        /// <summary>
        /// 드롭 표에서 가중치 비율로 항목 하나를 뽑는다. 가중치가 0 이하인 항목은 뽑히지 않는다.
        /// </summary>
        /// <param name="loots">드롭 표. 비었거나 가중치 합이 0이면 아무것도 뽑지 않는다(판을 준비할 때 경고를 남긴다)</param>
        /// <param name="level">적의 레벨</param>
        /// <param name="random">시드 고정 난수 생성기</param>
        /// <param name="kind">뽑힌 전리품 종류</param>
        /// <param name="amount">뽑힌 전리품의 양</param>
        /// <returns>떨어뜨릴 전리품이 있으면 true. None이 뽑혔거나 양이 0 이하면 false</returns>
        public static bool TryRoll(EnemyLootComponent[] loots, int level, System.Random random, out LootKind kind, out int amount)
        {
            kind = LootKind.None;
            amount = 0;

            int totalWeight = 0;
            for (int lootIndex = 0; lootIndex < loots.Length; lootIndex++)
            {
                if (loots[lootIndex].weight > 0)
                {
                    totalWeight += loots[lootIndex].weight;
                }
            }

            if (totalWeight <= 0)
            {
                return false;
            }

            int remainingWeight = random.Next(totalWeight);
            for (int lootIndex = 0; lootIndex < loots.Length; lootIndex++)
            {
                ref readonly EnemyLootComponent loot = ref loots[lootIndex];
                if (loot.weight <= 0)
                {
                    continue;
                }

                if (remainingWeight < loot.weight)
                {
                    kind = loot.kind;
                    amount = loot.baseAmount + level * loot.amountIncrement;
                    return kind != LootKind.None && amount > 0;
                }

                remainingWeight -= loot.weight;
            }

            return false;
        }
    }
}
