using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using TopView.Commands;
using TopView.Components;
using TopView.Core;
using TopView.Definitions;
using TopView.Persistence;
using TopView.Presentation.ViewContexts;
using UnityEngine;
using UnityEngine.TestTools;

namespace TopView.Tests.PlayMode
{
    // App의 앱 수명 일(설정 화면, 볼륨 저장)을 실제 App으로 확인한다. App은 World 씬을 불러올 때 에디터 부트스트랩이 만든다.
    // 볼륨은 에디터와 같은 저장소(PlayerPrefs)에 저장되므로, 테스트 전의 값을 기억했다가 끝나면 되돌린다.
    public class AppTests
    {
        private WorldTestScene _scene;
        private App _app;
        private bool _hasStoredVolume;
        private AudioVolumeComponent _storedVolume;

        // App의 설정 명령 목록. App이 Update에서 소비한다
        private List<SettingsCommand> SettingsCommands => PrivateFieldAccess.Read<List<SettingsCommand>>(_app, "_settingsCommands");
        // 설정 화면이 열려 있는지
        private bool IsSettingsOpen => PrivateFieldAccess.Read<bool>(_app, "_isSettingsOpen");
        // App이 볼륨을 저장할 수 있는지. Audio Settings Definition이 없으면 저장하지 않는다
        private bool IsVolumeSavable => PrivateFieldAccess.Read<AudioVolumeViewContext>(_app, "_audioVolumeViewContext") != null;

        // 저장소에 저장된 볼륨. 저장된 값이 없으면 시작값이다. 쓰면 그 값을 저장소에 저장한다(테스트가 바꾼 값을 되돌릴 때 쓴다)
        private AudioVolumeComponent StoredVolume
        {
            get => AudioVolumeStore.Load(in PrivateFieldAccess.Read<AudioSettingsDefinition>(_app, "_audioSettingsDefinition").settings);
            set => AudioVolumeStore.Save(in value);
        }

        // 볼륨 비교 허용 오차
        private const float VOLUME_TOLERANCE = 0.001f;

        /// <summary>
        /// 틱을 멈춘 채 World 씬을 불러오고, 저장된 볼륨을 기억한다.
        /// </summary>
        /// <returns>판이 준비되면 끝나는 코루틴</returns>
        [UnitySetUp]
        public IEnumerator SetUp()
        {
            _scene = new WorldTestScene();
            yield return _scene.Load();
            _app = App.Instance;
            Assert.IsNotNull(_app, "World 씬을 불러온 뒤 App이 있어야 한다.");
            Assert.IsTrue(IsVolumeSavable, "App 프리팹에 Audio Settings Definition이 연결되어 있어야 볼륨을 저장해 이 테스트가 뜻이 있다.");
            _storedVolume = StoredVolume;
            _hasStoredVolume = true;
        }

        /// <summary>
        /// 저장된 볼륨과 멈춘 틱을 되돌린다.
        /// </summary>
        [TearDown]
        public void TearDown()
        {
            if (_hasStoredVolume)
            {
                StoredVolume = _storedVolume;
                _hasStoredVolume = false;
            }
            _scene.Unload();
        }

        /// <summary>
        /// 설정 화면을 연 채로 앱이 끝나도(창 닫기, Alt+F4) 바꾼 볼륨이 저장된다. 앱 종료는 Unity가 보내는 OnApplicationQuit 메시지로 흉내 낸다.
        /// </summary>
        /// <returns>테스트 코루틴</returns>
        [UnityTest]
        public IEnumerator VolumeIsSavedWhenAppQuitsWithSettingsOpen()
        {
            float changedVolume = GetChangedVolume();
            yield return OpenSettingsAndSetMusicVolume(changedVolume);

            _app.gameObject.SendMessage("OnApplicationQuit", SendMessageOptions.DontRequireReceiver);

            Assert.AreEqual(changedVolume, StoredVolume.musicVolume, VOLUME_TOLERANCE, "설정 화면을 연 채 앱이 끝났는데 바꾼 배경음악 볼륨이 저장되지 않았다.");
        }

        /// <summary>
        /// 설정 화면을 닫으면 바꾼 볼륨이 저장된다.
        /// </summary>
        /// <returns>테스트 코루틴</returns>
        [UnityTest]
        public IEnumerator VolumeIsSavedWhenSettingsClose()
        {
            float changedVolume = GetChangedVolume();
            yield return OpenSettingsAndSetMusicVolume(changedVolume);

            SettingsCommands.Add(new SettingsCommand { type = SettingsCommandType.Close });
            yield return null;

            Assert.AreEqual(changedVolume, StoredVolume.musicVolume, VOLUME_TOLERANCE);
        }

        // 저장된 값과 확실히 다른 볼륨을 고른다.
        private float GetChangedVolume()
        {
            return _storedVolume.musicVolume > 0.5f ? 0.1f : 0.9f;
        }

        // 설정 화면은 일시정지 중에만 열리므로 일시정지를 걸고 연 뒤, 배경음악 볼륨을 바꾼다. App이 Update에서 명령을 소비하도록 프레임을 넘긴다.
        private IEnumerator OpenSettingsAndSetMusicVolume(float musicVolume)
        {
            GameProgressComponent progress = _scene.Progress;
            progress.isPaused = true;
            _scene.Progress = progress;
            SettingsCommands.Add(new SettingsCommand { type = SettingsCommandType.Open });
            yield return null;
            Assert.IsTrue(IsSettingsOpen, "설정 화면이 열려야 이 테스트가 뜻이 있다.");

            SettingsCommands.Add(new SettingsCommand { type = SettingsCommandType.SetMusicVolume, value = musicVolume });
            yield return null;
            Assert.AreNotEqual(musicVolume, StoredVolume.musicVolume, "볼륨을 바꾼 것만으로는 아직 저장되지 않아야 이 테스트가 뜻이 있다.");
        }
    }
}
