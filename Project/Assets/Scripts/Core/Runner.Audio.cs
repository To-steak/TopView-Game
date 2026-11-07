using TopView.Components;
using TopView.Definitions;
using TopView.Persistence;
using TopView.Presentation.ViewContexts;
using TopView.Presentation.ViewSystems;
using UnityEngine;

namespace TopView.Core
{
    // 볼륨: 판을 준비할 때 저장된 볼륨을 Audio Mixer에 반영한다
    public partial class Runner
    {
        // 볼륨을 조절할 Audio Mixer와 볼륨 시작값
        [SerializeField] private AudioSettingsDefinition _audioSettingsDefinition;

        // 메뉴를 거치지 않고 World 씬부터 실행해도 저장된 볼륨이 들리도록 판마다 반영한다.
        // 볼륨은 판의 결과와 상관없는 표시 설정이라, 없어도 판은 시작한다.
        private void PresentSavedAudioVolume()
        {
            if (_audioSettingsDefinition == null)
            {
                Debug.LogWarning("Runner >> Runner에 Audio Settings Definition이 연결되어 있지 않아 저장된 볼륨을 반영하지 않는다.");
                return;
            }

            AudioVolumeComponent volume = AudioVolumeStore.Load(in _audioSettingsDefinition.settings);
            // 판 시작 때 한 번만 반영하므로 실행 상태를 남겨 둘 필요가 없다
            AudioVolumeViewContext context = new AudioVolumeViewContext { mixer = _audioSettingsDefinition.mixer };
            AudioVolumeViewSystem.Present(in volume, context);
        }
    }
}
