using TMPro;
using UnityEngine;

namespace TopView.Presentation.Views
{
    // 게임 세계의 한 자리에 잠깐 띄우는 월드 공간 글자 프리팹의 참조와 연출 값 묶음
    public class WorldTextView : MonoBehaviour
    {
        // 표시할 글자. 레이아웃 그룹이 위치를 덮어쓰지 않도록 Body 안쪽에 둔다
        public TMP_Text amountText;
        // 글자를 서서히 사라지게 하는 CanvasGroup
        public CanvasGroup canvasGroup;
        // 글자가 나타나서 사라질 때까지의 시간(틱)
        public int lifetimeTicks = 40;
        // 사라지는 동안 글자가 위로 올라가는 거리(글자 RectTransform의 앵커 단위)
        public float riseDistance = 100f;
        // 같은 자리에서 겹치지 않도록 피해량 글자의 시작 위치를 어긋나게 하는 반지름(월드 유닛)
        public float spreadRadius = 0.2f;
        // 피해량 글자 색
        public Color damageColor = Color.white;
        // 획득 글자가 전리품 위치에서 떠 있는 높이(월드 유닛)
        public float lootHeightOffset = 1f;
        // 골드 획득 글자 색
        public Color goldColor = Color.yellow;
        // 골드 획득 글자 형식. {0}이 획득한 양이다
        public string goldFormat = "+{0} G";
        // 체력 획득 글자 색
        public Color healthColor = Color.green;
        // 체력 획득 글자 형식. {0}이 획득한 양이다
        public string healthFormat = "+{0} HP";
        // 탄약 획득 글자 색
        public Color ammoColor = Color.cyan;
        // 탄약 획득 글자 형식. {0}이 획득한 양이다
        public string ammoFormat = "+{0}";
    }
}
