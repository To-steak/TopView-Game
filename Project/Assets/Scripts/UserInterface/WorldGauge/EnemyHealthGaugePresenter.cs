using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace TopView.UserInterface.WorldGauge
{
    // 살아 있는 적마다 체력 게이지를 풀에서 꺼내 붙이고, Runner가 매 프레임 넘기는 체력을 보여 준다.
    // 같은 모양의 GaugeWorldView 여러 개를 Presenter 하나가 맡는 것은 사용자가 정한 규칙 예외다.
    public class EnemyHealthGaugePresenter : MonoBehaviour
    {
        [SerializeField] private GaugeWorldView _gaugePrefab;
        [SerializeField] private Transform _gaugeParent;

        private readonly Stack<GaugeWorldView> _pool = new Stack<GaugeWorldView>();
        // 적 저장소 칸별 게이지. 빈 칸은 null
        private GaugeWorldView[] _gauges;
        // 적 저장소 칸별로 게이지가 따라가는 대상
        private Transform[] _targets;
        private float[] _presentedHealths;
        private float[] _presentedMaxHealths;
        private Vector3[] _offsetsByKind;
        private Color[] _colorsByKind;

        /// <summary>
        /// 이전 판의 게이지를 모두 풀에 돌려놓고, 칸 수와 종류별 게이지 위치·색을 받는다. 판을 준비할 때 Runner가 부른다.
        /// </summary>
        /// <param name="slotCount">적 저장소 칸 수</param>
        /// <param name="offsetsByKind">적 종류별로 게이지를 적 위치에서 띄울 거리(월드)</param>
        /// <param name="colorsByKind">적 종류별 게이지 색</param>
        public void Initialize(int slotCount, Vector3[] offsetsByKind, Color[] colorsByKind)
        {
            if (_gauges != null)
            {
                for (int slotIndex = 0; slotIndex < _gauges.Length; slotIndex++)
                {
                    Release(slotIndex);
                }
            }

            _gauges = new GaugeWorldView[slotCount];
            _targets = new Transform[slotCount];
            _presentedHealths = new float[slotCount];
            _presentedMaxHealths = new float[slotCount];
            _offsetsByKind = offsetsByKind;
            _colorsByKind = colorsByKind;
        }

        /// <summary>
        /// 칸에 대상이 있으면 게이지를 붙여 체력을 보여 주고, 없으면 게이지를 풀에 돌려놓는다.
        /// </summary>
        /// <param name="slotIndex">적 저장소 칸 번호</param>
        /// <param name="kind">적 종류 번호</param>
        /// <param name="target">적 View의 Transform. 없으면 null</param>
        /// <param name="health">현재 체력</param>
        /// <param name="maxHealth">최대 체력</param>
        public void Present(int slotIndex, int kind, Transform target, float health, float maxHealth)
        {
            if (target == null)
            {
                Release(slotIndex);
                return;
            }

            GaugeWorldView gauge = _gauges[slotIndex];
            if (gauge == null || _targets[slotIndex] != target)
            {
                Release(slotIndex);
                gauge = _pool.Count > 0 ? _pool.Pop() : Instantiate(_gaugePrefab, _gaugeParent);
                gauge.SetTarget(target, _offsetsByKind[kind]);
                gauge.SetColor(_colorsByKind[kind]);
                gauge.gameObject.SetActive(true);
                _gauges[slotIndex] = gauge;
                _targets[slotIndex] = target;
                gauge.UpdateGauge(health, maxHealth);
                _presentedHealths[slotIndex] = health;
                _presentedMaxHealths[slotIndex] = maxHealth;
                return;
            }

            if (health != _presentedHealths[slotIndex] || maxHealth != _presentedMaxHealths[slotIndex])
            {
                gauge.UpdateGauge(health, maxHealth);
                _presentedHealths[slotIndex] = health;
                _presentedMaxHealths[slotIndex] = maxHealth;
            }
        }

        private void Release(int slotIndex)
        {
            GaugeWorldView gauge = _gauges[slotIndex];
            if (gauge == null)
            {
                return;
            }

            gauge.SetTarget(null, Vector3.zero);
            gauge.gameObject.SetActive(false);
            _pool.Push(gauge);
            _gauges[slotIndex] = null;
            _targets[slotIndex] = null;
        }
    }
}
