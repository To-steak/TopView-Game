using UnityEngine;
using UnityEngine.UI;

namespace TopView.UserInterface.WorldGauge
{
    // 월드 공간에 뜨는 게이지. 대상을 따라다니며 카메라를 바라본다.
    public class GaugeWorldView : MonoBehaviour
    {
        [SerializeField] private Image gaugeFront;

        private Transform _cameraTransform;
        private Transform _target;
        private Vector3 _offset;

        /// <summary>
        /// 따라다닐 대상과 대상 위치에서 띄울 거리를 정한다.
        /// </summary>
        /// <param name="target">따라다닐 대상. null이면 멈춘다</param>
        /// <param name="offset">대상 위치에서 띄울 거리(월드)</param>
        public void SetTarget(Transform target, Vector3 offset)
        {
            _target = target;
            _offset = offset;
        }

        /// <summary>
        /// 게이지 색을 정한다.
        /// </summary>
        /// <param name="color">게이지 색</param>
        public void SetColor(Color color)
        {
            gaugeFront.color = color;
        }

        /// <summary>
        /// 게이지를 현재 값 / 최대 값만큼 채운다.
        /// </summary>
        /// <param name="current">현재 값</param>
        /// <param name="max">최대 값. 0 이하면 바꾸지 않는다</param>
        public void UpdateGauge(float current, float max)
        {
            if (gaugeFront != null && max > 0)
            {
                gaugeFront.fillAmount = current / max;
            }
        }

        private void Start()
        {
            if (UnityEngine.Camera.main != null)
            {
                _cameraTransform = UnityEngine.Camera.main.transform;
            }
        }

        private void LateUpdate()
        {
            if (_target != null)
            {
                transform.position = _target.position + _offset;
            }

            if (_cameraTransform != null)
            {
                transform.LookAt(_cameraTransform);
                transform.Rotate(0, 180, 0);
            }
        }
    }
}
