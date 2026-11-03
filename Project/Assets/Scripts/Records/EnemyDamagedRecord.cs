using TopView.Entities;
using UnityEngine;

namespace TopView.Records
{
    // 적이 피해를 받았다. View가 피해량 글자를 띄운다
    public struct EnemyDamagedRecord
    {
        // 피해를 받은 적
        public EntityIdentifier enemy;
        // 피해를 받은 순간 적의 위치(월드). 적 View가 이미 없으면 이 위치에 글자를 띄운다
        public Vector3 position;
        // 요청된 피해량. 남은 체력보다 커도 그대로 담는다
        public float amount;
    }
}
