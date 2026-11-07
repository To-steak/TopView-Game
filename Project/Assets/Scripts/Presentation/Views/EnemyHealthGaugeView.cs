using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TopView.Presentation.Views
{
    // 적 머리 위에 뜨는 월드 공간 체력 게이지 프리팹의 참조와 연출 값 묶음
    public class EnemyHealthGaugeView : MonoBehaviour
    {
        // 게이지 이미지의 부모에 붙은 마스크. 오른쪽 여백으로 게이지를 가려 체력 비율만큼만 보인다. 게이지 이미지는 앵커를 Stretch로 둔다
        public RectMask2D healthMask;
        // 게이지가 새 체력 비율까지 부드럽게 움직이는 시간(틱). 0이면 바로 맞춘다
        public int tweenTicks = 15;
        // 체력을 "현재 / 최대"로 보여 주는 글자
        public TMP_Text healthText;
    }
}
