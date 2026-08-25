using UnityEngine;
using UnityEngine.UI;

public class GaugeWorldView : MonoBehaviour
{
    [SerializeField] private Image gaugeFront;

    private Transform _cameraTransform;
    private Transform _target;
    private Vector3 _offset;

    void Start()
    {
        if (Camera.main != null)
        {
            _cameraTransform = Camera.main.transform;
        }
    }

    void LateUpdate()
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

    public void SetTarget(Transform target, Vector3 offset)
    {
        _target = target;
        _offset = offset;
    }

    public void SetColor(Color color)
    {
        gaugeFront.color = color;
    }

    public void UpdateGauge(float current, float max)
    {
        if (gaugeFront != null && max > 0)
        {
            gaugeFront.fillAmount = current / max;
        }
    }
}