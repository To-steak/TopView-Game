using UnityEngine;

namespace TopView.Presentation.ViewContexts
{
    // 배경음악 실행 상태. 판마다 새로 만든다.
    public class MusicViewContext
    {
        // 평소 곡을 재생하는 AudioSource
        public AudioSource normalSource;
        // 보스 곡을 재생하는 AudioSource. 보스 곡이 없으면 null
        public AudioSource bossSource;
        // 곡이 다 들렸을 때의 볼륨(0~1)
        public float volume;
        // 곡을 페이드 인하거나 크로스페이드하는 데 걸리는 틱 수
        public int fadeTicks;
        // 이번 판에 곡을 틀었는지
        public bool hasStarted;
        // 이번 판에 보스 곡으로 바꿨는지
        public bool hasSwitchedToBoss;
    }
}
