namespace TopView.Components
{
    // 런타임 값. 지금 적용 중인 볼륨. 저장소에서 불러오고, 설정 화면에서 바꾼다.
    public struct AudioVolumeComponent
    {
        // 배경음악 볼륨(0~1)
        public float musicVolume;
        // 효과음 볼륨(0~1)
        public float soundEffectVolume;
    }
}
