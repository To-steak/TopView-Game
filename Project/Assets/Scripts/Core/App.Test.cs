#if UNITY_INCLUDE_TESTS
using System.Collections.Generic;
using TopView.Commands;
using TopView.Components;
using TopView.Persistence;

namespace TopView.Core
{
    // PlayMode 테스트가 App의 데이터를 읽고 쓰는 통로. 테스트가 설정 명령을 넣고 저장된 볼륨을 본다.
    // 테스트 어셈블리가 있을 때만 컴파일되어 빌드에는 들어가지 않는다.
    public partial class App
    {
        // 설정 명령 목록. App이 Update에서 소비한다
        public List<SettingsCommand> TestSettingsCommands => _settingsCommands;
        // 설정 화면이 열려 있는지
        public bool IsTestSettingsOpen => _isSettingsOpen;
        // 볼륨을 저장할 수 있는지. Audio Settings Definition이 없으면 저장하지 않는다
        public bool IsTestVolumeSavable => _audioVolumeViewContext != null;

        // 저장소에 저장된 볼륨. 저장된 값이 없으면 시작값이다. 쓰면 그 값을 저장소에 저장한다(테스트가 바꾼 값을 되돌릴 때 쓴다).
        // IsTestVolumeSavable이 false면 쓰지 않는다
        public AudioVolumeComponent TestStoredVolume
        {
            get => AudioVolumeStore.Load(in _audioSettingsDefinition.settings);
            set => AudioVolumeStore.Save(in value);
        }
    }
}
#endif
