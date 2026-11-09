using TopView.Components;
using UnityEngine;

namespace TopView.Definitions
{
    // 무기 하나의 종류와 값
    [CreateAssetMenu(fileName = "Weapon Definition", menuName = "Definitions/Weapon Definition")]
    public class WeaponDefinition : ScriptableObject
    {
        // 무기 값
        public WeaponComponent weapon;
        // 플레이어 무기 소켓에 붙일 모델 프리팹
        public GameObject modelPrefab;
        // ProjectileView가 붙은 투사체 프리팹. Ranged와 Throw가 쓴다
        public GameObject projectilePrefab;
        // 투사체가 맞거나 폭발할 때 재생할 ParticleSystem 프리팹. 없으면 null. 루트에 AudioSource가 있으면 그 소리도 함께 재생한다
        public GameObject hitEffectPrefab;
        // 근접 판정이 시작될 때 판정 원의 중심에서 재생할 ParticleSystem 프리팹. Melee만 쓰며, 없으면 null. 루트에 AudioSource나 CinemachineImpulseSource가 있으면 소리와 화면 흔들림도 함께 낸다
        public GameObject impactEffectPrefab;
    }
}
