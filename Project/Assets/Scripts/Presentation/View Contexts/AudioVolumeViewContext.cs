using UnityEngine.Audio;

namespace TopView.Presentation.ViewContexts
{
    // 볼륨 반영 실행 상태. 볼륨을 반영하는 쪽(Runner, MenuFlow)이 씬마다 새로 만든다.
    public class AudioVolumeViewContext
    {
        // 볼륨이 노출된 Audio Mixer. 없으면 볼륨을 반영하지 않는다
        public AudioMixer mixer;
        // 반영 실패를 이미 경고했는지. 슬라이더를 움직이는 동안 같은 경고가 매 프레임 쌓이지 않게 한 번만 남긴다
        public bool hasReportedFailure;
    }
}
