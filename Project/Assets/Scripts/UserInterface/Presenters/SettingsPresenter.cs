using System.Collections.Generic;
using TopView.Commands;
using TopView.Components;
using TopView.UserInterface.Views;
using UnityEngine;

namespace TopView.UserInterface.Presenters
{
    // 설정이 열려 있으면 SettingsView를 보이고 볼륨을 슬라이더에 맞춘다. 슬라이더와 닫기 입력을 메뉴 명령으로 넣는다.
    public class SettingsPresenter : MonoBehaviour
    {
        [SerializeField] private SettingsView _view;

        private List<MenuCommand> _commands;
        private bool _isShown;
        // 같은 값을 매 프레임 다시 쓰지 않도록 마지막으로 보여 준 값을 기억한다
        private float _presentedMusicVolume;
        private float _presentedSoundEffectVolume;

        /// <summary>
        /// 명령을 넣을 목록을 받고 View를 숨긴다. MenuFlow가 시작할 때 부른다.
        /// </summary>
        /// <param name="commands">메뉴 명령 목록</param>
        public void Initialize(List<MenuCommand> commands)
        {
            _commands = commands;
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
            AddCommand(MenuCommandType.SetMusicVolume, volume);
        }

        private void HandleSoundEffectVolumeChanged(float volume)
        {
            AddCommand(MenuCommandType.SetSoundEffectVolume, volume);
        }

        private void HandleCloseClicked()
        {
            AddCommand(MenuCommandType.CloseSettings, 0f);
        }

        private void AddCommand(MenuCommandType type, float value)
        {
            if (_commands == null)
            {
                Debug.LogWarning($"SettingsPresenter >> 명령 목록이 연결되지 않아 {type} 명령을 버렸다.");
                return;
            }

            _commands.Add(new MenuCommand { type = type, value = value });
        }
    }
}
