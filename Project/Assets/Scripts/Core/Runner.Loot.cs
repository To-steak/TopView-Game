using TopView.Components;
using TopView.Definitions;
using TopView.Presentation.Views;
using UnityEngine;

namespace TopView.Core
{
    // 전리품: 종류별 View 표 만들기와 드롭 표 확인
    public partial class Runner
    {
        // 전리품 종류별 View 프리팹. 번호가 LootKind 값이고, 없는 종류는 null이다
        private GameObject[] _lootPrefabs;

        // Loot Definition에서 종류별 View 표를 만든다.
        private static GameObject[] ReadLootPrefabs(LootDefinition definition)
        {
            GameObject[] prefabs = new GameObject[System.Enum.GetValues(typeof(LootKind)).Length];
            AssignLootPrefab(prefabs, LootKind.Gold, definition.goldViewPrefab);
            AssignLootPrefab(prefabs, LootKind.Health, definition.healthViewPrefab);
            AssignLootPrefab(prefabs, LootKind.Ammo, definition.ammoViewPrefab);
            return prefabs;
        }

        // View는 표시용이라 빠져 있어도 판은 시작하고, 그 종류는 보이지 않게 둔다.
        private static void AssignLootPrefab(GameObject[] prefabs, LootKind kind, GameObject prefab)
        {
            if (prefab == null || prefab.GetComponent<LootView>() == null)
            {
                Debug.LogWarning($"Runner >> Loot Definition의 {kind} 프리팹이 없거나 LootView가 붙어 있지 않아 이 전리품은 보이지 않게 진행한다.");
                return;
            }

            prefabs[(int)kind] = prefab;
        }

        // 적 종류의 드롭 표를 확인해 잘못된 값에 경고를 남긴다. 값이 없으면 빈 표를 돌려준다.
        private static EnemyLootComponent[] ReadLoots(string definitionName, EnemyLootComponent[] loots)
        {
            if (loots == null)
            {
                loots = new EnemyLootComponent[0];
            }

            int totalWeight = 0;
            for (int lootIndex = 0; lootIndex < loots.Length; lootIndex++)
            {
                ref readonly EnemyLootComponent loot = ref loots[lootIndex];
                if (loot.weight > 0)
                {
                    totalWeight += loot.weight;
                }
                else
                {
                    Debug.LogWarning($"Runner >> {definitionName} 드롭 표 {lootIndex}번의 Weight가 {loot.weight}라 이 항목은 뽑히지 않는다.");
                }

                if (loot.kind != LootKind.None && loot.baseAmount <= 0)
                {
                    Debug.LogWarning($"Runner >> {definitionName} 드롭 표 {lootIndex}번({loot.kind})의 Base Amount가 {loot.baseAmount}라 얻는 양이 없을 수 있다.");
                }
            }

            if (totalWeight <= 0)
            {
                Debug.LogWarning($"Runner >> {definitionName}의 드롭 표가 비었거나 가중치 합이 0이라 이 적 종류는 전리품을 떨어뜨리지 않는다.");
            }

            return loots;
        }
    }
}
