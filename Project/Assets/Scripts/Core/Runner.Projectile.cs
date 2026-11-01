using System.Collections.Generic;
using TopView.Components;
using TopView.Presentation.Projectile.Views;
using UnityEngine;

namespace TopView.Core
{
    // 투사체: 투사체 View 표와 피격 이펙트 표 만들기
    public partial class Runner
    {
        // 투사체 View 표와 피격 이펙트 표. 판을 준비할 때 Definition에서 모은다
        private GameObject[] _projectilePrefabs;
        private float[] _projectilePitchSpeeds;
        private GameObject[] _hitEffectPrefabs;

        // 적·보스 공격 목록의 투사체마다 View와 피격 이펙트를 표에 올린다.
        private static void AssignAttackViews(string definitionName, EnemyAttackComponent[] attacks, GameObject[] attackProjectilePrefabs, GameObject[] attackHitEffectPrefabs, float[] attackPitchSpeeds, List<GameObject> projectilePrefabs, List<float> projectilePitchSpeeds, List<GameObject> hitEffectPrefabs)
        {
            for (int attackIndex = 0; attackIndex < attacks.Length; attackIndex++)
            {
                bool hasProjectile = attacks[attackIndex].kind != EnemyAttackKind.Body;
                GameObject projectilePrefab = GetOrNull(attackProjectilePrefabs, attackIndex);
                GameObject hitEffectPrefab = GetOrNull(attackHitEffectPrefabs, attackIndex);
                float pitchSpeed = attackPitchSpeeds != null && attackIndex < attackPitchSpeeds.Length ? attackPitchSpeeds[attackIndex] : 0f;
                AssignProjectileViews($"{definitionName} 공격 {attackIndex}번", hasProjectile, projectilePrefab, hitEffectPrefab, pitchSpeed, ref attacks[attackIndex].projectile, projectilePrefabs, projectilePitchSpeeds, hitEffectPrefabs);
            }
        }

        // 투사체 View 표와 피격 이펙트 표에 프리팹을 올리고(이미 있으면 그 번호를 쓰고) 번호를 투사체 값에 넣는다.
        // 투사체가 없거나 프리팹이 빠져 있으면 번호는 -1이고, 그 투사체나 이펙트는 보이지 않는다. 표시용이라 판은 시작한다.
        private static void AssignProjectileViews(string ownerName, bool hasProjectile, GameObject projectilePrefab, GameObject hitEffectPrefab, float pitchSpeed, ref ProjectileComponent projectile, List<GameObject> projectilePrefabs, List<float> projectilePitchSpeeds, List<GameObject> hitEffectPrefabs)
        {
            projectile.viewIndex = -1;
            projectile.hitEffectIndex = -1;
            if (!hasProjectile)
            {
                return;
            }

            if (projectilePrefab == null || projectilePrefab.GetComponent<ProjectileView>() == null)
            {
                Debug.LogWarning($"Runner >> {ownerName}의 투사체 프리팹이 없거나 ProjectileView가 붙어 있지 않아 이 투사체는 보이지 않게 진행한다.");
            }
            else
            {
                projectile.viewIndex = projectilePrefabs.IndexOf(projectilePrefab);
                if (projectile.viewIndex < 0)
                {
                    projectile.viewIndex = projectilePrefabs.Count;
                    projectilePrefabs.Add(projectilePrefab);
                    projectilePitchSpeeds.Add(pitchSpeed);
                }
            }

            if (hitEffectPrefab == null)
            {
                return;
            }

            if (hitEffectPrefab.GetComponent<ParticleSystem>() == null)
            {
                Debug.LogWarning($"Runner >> {ownerName}의 피격 이펙트 프리팹에 ParticleSystem이 없어 이 피격 이펙트는 나오지 않게 진행한다.");
                return;
            }

            projectile.hitEffectIndex = hitEffectPrefabs.IndexOf(hitEffectPrefab);
            if (projectile.hitEffectIndex < 0)
            {
                projectile.hitEffectIndex = hitEffectPrefabs.Count;
                hitEffectPrefabs.Add(hitEffectPrefab);
            }
        }

        private static GameObject GetOrNull(GameObject[] prefabs, int index)
        {
            return prefabs != null && index < prefabs.Length ? prefabs[index] : null;
        }
    }
}
