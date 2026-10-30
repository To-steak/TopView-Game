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
        // 투사체가 맞거나 폭발할 때 재생할 ParticleSystem 프리팹. 없으면 null
        public GameObject hitEffectPrefab;
    }
}
