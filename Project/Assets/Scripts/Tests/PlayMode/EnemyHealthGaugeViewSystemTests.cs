using System.Collections.Generic;
using NUnit.Framework;
using TMPro;
using TopView.Components;
using TopView.Entities;
using TopView.Presentation.ViewContexts;
using TopView.Presentation.ViewSystems;
using TopView.Presentation.Views;
using UnityEngine;
using UnityEngine.UI;

namespace TopView.Tests.PlayMode
{
    // 적 머리 위 체력 게이지가 적의 체력을 글자로 맞게 보여 주는지 확인한다.
    public class EnemyHealthGaugeViewSystemTests
    {
        private Entity[] _entities;
        private EnemyViewContext _context;
        private GameObject[] _gaugePrefabs;
        private EnemyView _enemyView;

        private const float MAX_HEALTH = 10f;

        /// <summary>
        /// 적 칸 하나와 그 칸의 적 View, 게이지 프리팹을 만든다.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            _entities = new Entity[1];
            _enemyView = new GameObject("Test Enemy View").AddComponent<EnemyView>();
            _enemyView.healthGaugeAnchor = _enemyView.transform;
            _context = new EnemyViewContext
            {
                views = new EnemyView[] { _enemyView },
                viewGenerations = new int[] { 1 },
                healthGaugeViews = new EnemyHealthGaugeView[1],
                healthGaugeGenerations = new int[1],
                healthGaugeRatios = new float[1],
                healthGaugePools = new Stack<EnemyHealthGaugeView>[] { new Stack<EnemyHealthGaugeView>() }
            };
            _gaugePrefabs = new GameObject[] { CreateGaugePrefab() };
        }

        /// <summary>
        /// 만든 View와 게이지를 지운다.
        /// </summary>
        [TearDown]
        public void TearDown()
        {
            Object.Destroy(_enemyView.gameObject);
            Object.Destroy(_gaugePrefabs[0]);
            if (_context.healthGaugeViews[0] != null)
            {
                Object.Destroy(_context.healthGaugeViews[0].gameObject);
            }
        }

        /// <summary>
        /// 체력이 0보다 크면 적은 살아 있으므로, 1 미만으로 남아도 게이지 글자가 0으로 보이지 않는다.
        /// </summary>
        [Test]
        public void AliveHealthBelowOneIsNotShownAsZero()
        {
            PlaceEnemy(0.35f);

            EnemyHealthGaugeViewSystem.Present(_entities, _gaugePrefabs, null, _context);

            Assert.AreNotEqual($"0 / {MAX_HEALTH:F0}", _context.healthGaugeViews[0].healthText.text, "체력 0.35가 남아 살아 있는 적의 체력이 0으로 보인다.");
        }

        /// <summary>
        /// 정수 체력은 그대로 보인다.
        /// </summary>
        [Test]
        public void WholeHealthIsShownAsIs()
        {
            PlaceEnemy(4f);

            EnemyHealthGaugeViewSystem.Present(_entities, _gaugePrefabs, null, _context);

            Assert.AreEqual($"4 / {MAX_HEALTH:F0}", _context.healthGaugeViews[0].healthText.text);
        }

        private void PlaceEnemy(float health)
        {
            _entities[0] = new Entity
            {
                mask = ComponentMask.EnemyKind | ComponentMask.Health | ComponentMask.Combat,
                generation = 1,
                enemyKind = new EnemyKindComponent { kind = 0 },
                health = new HealthComponent { current = health, max = MAX_HEALTH },
                combat = new CombatComponent { level = 1 }
            };
        }

        // 꺼 둔 채로 만들어, 게이지가 풀에서 꺼낼 때 켜는 프리팹처럼 쓴다.
        private static GameObject CreateGaugePrefab()
        {
            GameObject prefab = new GameObject("Test Enemy Health Gauge", typeof(RectTransform));
            prefab.SetActive(false);
            EnemyHealthGaugeView gauge = prefab.AddComponent<EnemyHealthGaugeView>();
            gauge.gaugeFront = new GameObject("Gauge Front", typeof(RectTransform)).AddComponent<Image>();
            gauge.gaugeFront.transform.SetParent(prefab.transform, false);
            gauge.healthText = new GameObject("Health Text", typeof(RectTransform)).AddComponent<TextMeshProUGUI>();
            gauge.healthText.transform.SetParent(prefab.transform, false);
            gauge.levelText = new GameObject("Level Text", typeof(RectTransform)).AddComponent<TextMeshProUGUI>();
            gauge.levelText.transform.SetParent(prefab.transform, false);
            return prefab;
        }
    }
}
