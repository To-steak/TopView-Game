using System.Collections.Generic;
using TopView.Commands;
using TopView.Utilities;
using UnityEngine;

namespace TopView.Core
{
    // 설정: 일시정지 화면에서 연 설정 화면의 명령을 소비한다. 볼륨은 판의 결과와 상관없는 표시 설정이라 Tick이 아닌 여기서 처리한다
    public partial class Runner
    {
        // 설정 화면이 열려 있는지. 일시정지 중에만 열린다
        private bool _isSettingsOpen;
        // 슬라이더 값이 0~1 밖이라는 경고를 이미 남겼는지. 슬라이더를 움직이는 동안 매 프레임 쌓이지 않게 한 번만 남긴다
        private bool _hasReportedVolumeOutOfRange;

        private void HandleSettingsCommands(List<SettingsCommand> settingsCommands)
        {
            for (int commandIndex = 0; commandIndex < settingsCommands.Count; commandIndex++)
            {
                HandleSettingsCommand(settingsCommands[commandIndex]);
            }
            settingsCommands.Clear();
        }

        private void HandleSettingsCommand(in SettingsCommand command)
        {
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
                Debug.LogWarning($"Runner >> {command.type} 볼륨 {command.value}이 0~1 밖이라 범위 안으로 맞췄다. 슬라이더의 Min/Max Value를 확인한다. 이 경고는 한 번만 남긴다.");
                _hasReportedVolumeOutOfRange = true;
            }
            PresentVolume();
        }
    }
}
