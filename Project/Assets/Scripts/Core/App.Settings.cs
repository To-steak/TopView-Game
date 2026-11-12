using TopView.Commands;
using TopView.UserInterface.Presenters;
using TopView.Utilities;
using UnityEngine;

namespace TopView.Core
{
    // 설정: 씬마다 설정 화면 Presenter를 찾아 연결하고, 설정 명령을 소비한다. 볼륨은 판의 결과와 상관없는 표시 설정이라 Tick이 아닌 여기서 처리한다
    public partial class App
    {
        // 지금 씬의 설정 화면. 씬을 등록할 때 찾는다. 없으면 null
        private SettingsPresenter _settingsPresenter;
        // 설정 화면이 열려 있는지
        private bool _isSettingsOpen;
        // 슬라이더 값이 0~1 밖이라는 경고를 이미 남겼는지. 슬라이더를 움직이는 동안 매 프레임 쌓이지 않게 한 번만 남긴다
        private bool _hasReportedVolumeOutOfRange;

        private void BindSettingsPresenter()
        {
            _isSettingsOpen = false;
            _settingsPresenter = FindAnyObjectByType<SettingsPresenter>();
            if (_settingsPresenter == null)
            {
                Debug.LogError("App >> 씬에 SettingsPresenter가 없다.");
                return;
            }

            _settingsPresenter.Initialize(_settingsCommands, _userInterfaceSoundRequests);
        }

        private void HandleSettingsCommands(bool canOpenSettings)
        {
            for (int commandIndex = 0; commandIndex < _settingsCommands.Count; commandIndex++)
            {
                HandleSettingsCommand(_settingsCommands[commandIndex], canOpenSettings);
            }
            _settingsCommands.Clear();
        }

        private void HandleSettingsCommand(in SettingsCommand command, bool canOpenSettings)
        {
            if (command.type == SettingsCommandType.Open)
            {
                // 설정 버튼은 열 수 있을 때만 보여, 그 뒤에 남은 명령(일시정지가 풀린 뒤, 로딩을 요청한 뒤)은 정상적으로 무시한다
                if (canOpenSettings)
                {
                    _isSettingsOpen = true;
                }
                return;
            }

            // 설정 화면이 닫힌 뒤에 남은 명령은 정상적으로 무시한다
            if (!_isSettingsOpen)
            {
                return;
            }

            if (command.type == SettingsCommandType.Close)
            {
                _isSettingsOpen = false;
                SaveVolume();
                return;
            }

            if (!AudioVolumeUtility.Apply(ref _volume, in command) && !_hasReportedVolumeOutOfRange)
            {
                Debug.LogWarning($"App >> {command.type} 볼륨 {command.value}이 0~1 밖이라 범위 안으로 맞췄다. 슬라이더의 Min/Max Value를 확인한다. 이 경고는 한 번만 남긴다.");
                _hasReportedVolumeOutOfRange = true;
            }
            PresentVolume();
        }
    }
}
