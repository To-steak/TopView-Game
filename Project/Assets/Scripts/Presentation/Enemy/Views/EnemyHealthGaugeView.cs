using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TopView.Presentation.Enemy.Views
{
    // 적 머리 위에 뜨는 월드 공간 체력 게이지 프리팹의 참조 묶음
    public class EnemyHealthGaugeView : MonoBehaviour
    {
        // 체력 비율만큼 너비가 바뀌는 게이지 이미지. 앵커를 Stretch로 둔다
        public Image gaugeFront;
        // 체력을 "현재 / 최대"로 보여 주는 글자
        public TMP_Text healthText;
    }
}
