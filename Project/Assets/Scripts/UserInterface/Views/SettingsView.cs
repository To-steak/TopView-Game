using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TopView.UserInterface.Views
{
    // 설정 화면. 배경음악·효과음 볼륨 슬라이더와 닫기 버튼 입력을 이벤트로 알린다.
    // 슬라이더의 Min Value는 0, Max Value는 1로 둔다.
    public class SettingsView : UserInterfaceView
    {
        // 배경음악 슬라이더 값이 바뀌었다. 인자는 볼륨(0~1)
        public event Action<float> MusicVolumeChanged;
        // 효과음 슬라이더 값이 바뀌었다. 인자는 볼륨(0~1)
        public event Action<float> SoundEffectVolumeChanged;
        // 닫기 버튼을 눌렀다
        public event Action CloseClicked;

        [SerializeField] private Slider _musicVolumeSlider;
        [SerializeField] private Slider _soundEffectVolumeSlider;
        // 배경음악 볼륨을 0~100 정수로 보여 준다
        [SerializeField] private TMP_Text _musicVolumeText;
        // 효과음 볼륨을 0~100 정수로 보여 준다
        [SerializeField] private TMP_Text _soundEffectVolumeText;
        [SerializeField] private Button _closeButton;

        /// <summary>
        /// 슬라이더와 값 글자를 볼륨 값으로 맞춘다. 값이 바뀌었다는 이벤트는 내지 않는다.
        /// </summary>
        /// <param name="musicVolume">배경음악 볼륨(0~1)</param>
        /// <param name="soundEffectVolume">효과음 볼륨(0~1)</param>
        public void SetVolumes(float musicVolume, float soundEffectVolume)
        {
            _musicVolumeSlider.SetValueWithoutNotify(musicVolume);
            _soundEffectVolumeSlider.SetValueWithoutNotify(soundEffectVolume);
            // SetText의 서식 인자는 문자열을 새로 만들지 않아 슬라이더를 움직이는 동안 매 프레임 불러도 할당이 없다
            _musicVolumeText.SetText("{0}", Mathf.RoundToInt(musicVolume * 100f));
            _soundEffectVolumeText.SetText("{0}", Mathf.RoundToInt(soundEffectVolume * 100f));
        }

        private void OnEnable()
        {
            _musicVolumeSlider.onValueChanged.AddListener(HandleMusicVolumeSliderChanged);
            _soundEffectVolumeSlider.onValueChanged.AddListener(HandleSoundEffectVolumeSliderChanged);
            _closeButton.onClick.AddListener(HandleCloseButtonClicked);
        }

        private void OnDisable()
        {
            _musicVolumeSlider.onValueChanged.RemoveListener(HandleMusicVolumeSliderChanged);
            _soundEffectVolumeSlider.onValueChanged.RemoveListener(HandleSoundEffectVolumeSliderChanged);
            _closeButton.onClick.RemoveListener(HandleCloseButtonClicked);
        }

        private void HandleMusicVolumeSliderChanged(float volume)
        {
            MusicVolumeChanged?.Invoke(volume);
        }

        private void HandleSoundEffectVolumeSliderChanged(float volume)
        {
            SoundEffectVolumeChanged?.Invoke(volume);
        }

        private void HandleCloseButtonClicked()
        {
            CloseClicked?.Invoke();
        }
    }
}
