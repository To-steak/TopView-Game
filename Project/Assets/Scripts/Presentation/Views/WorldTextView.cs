using TMPro;
using UnityEngine;

namespace TopView.Presentation.Views
{
    // 적이 받은 피해량을 보여 주는 월드 공간 글자 프리팹의 참조 묶음
    public class WorldTextView : MonoBehaviour
    {
        // 피해량 글자. 레이아웃 그룹이 위치를 덮어쓰지 않도록 Body 안쪽에 둔다
        public TMP_Text amountText;
        // 글자를 서서히 사라지게 하는 CanvasGroup
        public CanvasGroup canvasGroup;
    }
}
