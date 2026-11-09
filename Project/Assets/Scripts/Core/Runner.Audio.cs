using TopView.Components;
using TopView.Definitions;
using TopView.Persistence;
using TopView.Presentation.ViewContexts;
using TopView.Presentation.ViewSystems;
using UnityEngine;

namespace TopView.Core
{
    // 볼륨: 판을 준비할 때 저장된 볼륨을 Audio Mixer에 반영하고, 설정 화면에서 바꾼 볼륨을 반영하고 저장한다
    public partial class Runner
    {
        // 볼륨을 조절할 Audio Mixer와 볼륨 시작값
        [SerializeField] private AudioSettingsDefinition _audioSettingsDefinition;

        // 지금 적용 중인 볼륨. 설정 화면이 보여 주고 바꾼다
        private AudioVolumeComponent _volume;
        // 볼륨 반영 실행 상태. Audio Settings Definition이 없으면 null이고, 이때는 볼륨을 반영하지도 저장하지도 않는다
        private AudioVolumeViewContext _audioVolumeViewContext;

        // 메뉴를 거치지 않고 World 씬부터 실행해도 저장된 볼륨이 들리도록 판마다 반영한다.
        // 볼륨은 판의 결과와 상관없는 표시 설정이라, 없어도 판은 시작한다.
        private void PresentSavedAudioVolume()
        {
            if (_audioSettingsDefinition == null)
            {
                // 저장된 볼륨을 1로 덮어쓰지 않도록 저장도 하지 않는다
                Debug.LogWarning("Runner >> Runner에 Audio Settings Definition이 연결되어 있지 않아 저장된 볼륨을 반영하지 않고, 설정 화면의 볼륨을 1로 보이며 저장하지 않는다.");
                _volume = new AudioVolumeComponent { musicVolume = 1f, soundEffectVolume = 1f };
                return;
            }

            _volume = AudioVolumeStore.Load(in _audioSettingsDefinition.settings);
            _audioVolumeViewContext = new AudioVolumeViewContext { mixer = _audioSettingsDefinition.mixer };
            PresentVolume();
        }

        // Audio Settings Definition이 없으면 Start에서 경고했으므로 로그 없이 건너뛴다
        private void PresentVolume()
        {
            if (_audioVolumeViewContext != null)
            {
                AudioVolumeViewSystem.Present(in _volume, _audioVolumeViewContext);
            }
        }

        // Audio Settings Definition이 없으면 저장된 볼륨을 시작값 1로 덮어쓰지 않도록 저장하지 않는다. Start에서 경고했다
        private void SaveVolume()
        {
            if (_audioVolumeViewContext != null)
            {
                AudioVolumeStore.Save(in _volume);
            }
        }
    }
}
