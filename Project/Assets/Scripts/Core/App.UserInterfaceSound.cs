using TopView.Definitions;
using TopView.Presentation.ViewContexts;
using UnityEngine;

namespace TopView.Core
{
    // UI 소리: 앱이 시작할 때 버튼 소리 AudioSource를 App 아래에 만들어 씬이 바뀌어도 이어 쓴다
    public partial class App
    {
        // 버튼 소리
        [SerializeField] private UserInterfaceSoundDefinition _userInterfaceSoundDefinition;

        // UI 소리 실행 상태. User Interface Sound Definition이 없으면 null이고, 이때는 버튼 소리를 내지 않는다
        private UserInterfaceSoundViewContext _userInterfaceSoundViewContext;

        private const string USER_INTERFACE_SOUND_OBJECT_NAME = "User Interface Sound";

        // App 아래에 UI 소리 오브젝트를 만든다. 일시정지 중에도 버튼 소리가 들리도록 ignoreListenerPause를 켠다.
        // User Interface Sound Definition이 없으면 null을 돌려준다.
        private UserInterfaceSoundViewContext CreateUserInterfaceSoundViewContext()
        {
            UserInterfaceSoundDefinition definition = _userInterfaceSoundDefinition;
            if (definition == null)
            {
                Debug.LogError("App >> User Interface Sound Definition이 연결되어 있지 않아 버튼 소리가 나지 않는다.");
                return null;
            }

            if (definition.outputGroup == null)
            {
                Debug.LogWarning("App >> User Interface Sound Definition에 Output Group이 연결되어 있지 않아 버튼 소리가 Audio Mixer를 거치지 않는다.");
            }

            GameObject soundObject = new GameObject(USER_INTERFACE_SOUND_OBJECT_NAME);
            soundObject.transform.SetParent(transform, false);
            AudioSource source = soundObject.AddComponent<AudioSource>();
            source.outputAudioMixerGroup = definition.outputGroup;
            source.playOnAwake = false;
            source.spatialBlend = 0f;
            source.ignoreListenerPause = true;
            return new UserInterfaceSoundViewContext
            {
                source = source,
                clickClips = definition.clickClips,
                closeClips = definition.closeClips,
                volume = definition.volume
            };
        }
    }
}
