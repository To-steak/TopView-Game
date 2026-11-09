using UnityEngine;

namespace TopView.Presentation.Views
{
    // 전리품 프리팹의 참조 묶음. Runner가 프리팹을 확인하고 LootViewSystem이 풀에서 다룰 때 쓴다.
    public class LootView : MonoBehaviour
    {
        // 회전시킬 모델 Transform. 회전하지 않으면 null
        public Transform rotationPivot;
        // 모델의 로컬 X축 각속도(도/초). 0이면 이 축으로 돌지 않는다
        public float pitchSpeed;
        // 모델의 로컬 Y축 각속도(도/초). 0이면 이 축으로 돌지 않는다
        public float yawSpeed;
        // 모델의 로컬 Z축 각속도(도/초). 0이면 이 축으로 돌지 않는다
        public float rollSpeed;
    }
}
