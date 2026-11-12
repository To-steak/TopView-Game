using System.Collections.Generic;
using TopView.Commands;
using TopView.Components;
using TopView.UserInterface.Views;
using UnityEngine;

namespace TopView.UserInterface.Presenters
{
    // 설정이 열려 있으면 SettingsView를 보이고 볼륨을 슬라이더에 맞춘다. 슬라이더와 닫기 입력을 설정 명령으로 넣는다. Menu와 World 씬이 함께 쓴다.
    public class SettingsPresenter : MonoBehaviour
    {
        [SerializeField] private SettingsView _view;

        private List<SettingsCommand> _commands;
        // 버튼 소리를 넣을 목록
        private List<UserInterfaceSoundKind> _soundRequests;
        private bool _isShown;
        // 같은 값을 매 프레임 다시 쓰지 않도록 마지막으로 보여 준 값을 기억한다
        private float _presentedMusicVolume;
        private float _presentedSoundEffectVolume;

        /// <summary>
        /// 명령을 넣을 목록을 받고 View를 숨긴다. 그 씬의 흐름을 맡은 쪽(MenuFlow, World)이 시작할 때 부른다.
        /// </summary>
        /// <param name="commands">설정 명령 목록</param>
        /// <param name="soundRequests">UI 소리 요청 목록</param>
        public void Initialize(List<SettingsCommand> commands, List<UserInterfaceSoundKind> soundRequests)
        {
            _commands = commands;
            _soundRequests = soundRequests;
            _view.SetVisible(false);
            _isShown = false;
        }

        /// <summary>
        /// 열려 있으면 View를 보이고 볼륨이 바뀌었을 때만 슬라이더를 맞춘다. 닫혀 있으면 숨긴다.
        /// </summary>
        /// <param name="isOpen">설정이 열려 있는지</param>
        /// <param name="volume">지금 적용 중인 볼륨</param>
        public void Present(bool isOpen, in AudioVolumeComponent volume)
        {
            if (!isOpen)
            {
                if (_isShown)
                {
                    _view.SetVisible(false);
                    _isShown = false;
                }
                return;
            }

            bool isFirstFrame = !_isShown;
            if (isFirstFrame)
            {
                _view.SetVisible(true);
                _isShown = true;
            }

            if (isFirstFrame || volume.musicVolume != _presentedMusicVolume || volume.soundEffectVolume != _presentedSoundEffectVolume)
            {
                _view.SetVolumes(volume.musicVolume, volume.soundEffectVolume);
                _presentedMusicVolume = volume.musicVolume;
                _presentedSoundEffectVolume = volume.soundEffectVolume;
            }
        }

        private void OnEnable()
        {
            _view.MusicVolumeChanged += HandleMusicVolumeChanged;
            _view.SoundEffectVolumeChanged += HandleSoundEffectVolumeChanged;
            _view.CloseClicked += HandleCloseClicked;
        }

        private void OnDisable()
        {
            _view.MusicVolumeChanged -= HandleMusicVolumeChanged;
            _view.SoundEffectVolumeChanged -= HandleSoundEffectVolumeChanged;
            _view.CloseClicked -= HandleCloseClicked;
        }

        private void HandleMusicVolumeChanged(float volume)
        {
            AddCommand(SettingsCommandType.SetMusicVolume, volume);
        }

        private void HandleSoundEffectVolumeChanged(float volume)
        {
            AddCommand(SettingsCommandType.SetSoundEffectVolume, volume);
        }

        private void HandleCloseClicked()
        {
            AddSound(UserInterfaceSoundKind.Close);
            AddCommand(SettingsCommandType.Close, 0f);
        }

        private void AddCommand(SettingsCommandType type, float value)
        {
            if (_commands == null)
            {
                Debug.LogWarning($"SettingsPresenter >> 명령 목록이 연결되지 않아 {type} 명령을 버렸다.");
                return;
            }

            _commands.Add(new SettingsCommand { type = type, value = value });
        }

        private void AddSound(UserInterfaceSoundKind kind)
        {
            if (_soundRequests == null)
            {
                Debug.LogWarning($"SettingsPresenter >> 소리 요청 목록이 연결되지 않아 {kind} 소리를 내지 않았다.");
                return;
            }

            _soundRequests.Add(kind);
        }
    }
}
