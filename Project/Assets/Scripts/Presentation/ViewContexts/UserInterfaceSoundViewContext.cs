using UnityEngine;

namespace TopView.Presentation.ViewContexts
{
    // UI 소리 실행 상태. 씬마다 새로 만든다.
    public class UserInterfaceSoundViewContext
    {
        // UI 소리를 재생하는 AudioSource. 일시정지 중에도 들리도록 ignoreListenerPause를 켠다
        public AudioSource source;
        // 일반 버튼 소리들
        public AudioClip[] clickClips;
        // 종료·닫기·나가기 버튼 소리들
        public AudioClip[] closeClips;
        // 소리 볼륨(0~1)
        public float volume;
    }
}
