using System.Collections.Generic;
using TopView.Components;
using TopView.Definitions;
using TopView.Entities;
using TopView.Presentation.Projectile.Views;
using UnityEngine;

namespace TopView.Core
{
    // 투사체: 투사체 View 표와 피격 이펙트 표 만들기
    public partial class Runner
    {
        // 투사체 View 표와 피격 이펙트 표. 판을 준비할 때 Definition에서 모은다
        private GameObject[] _projectilePrefabs;
        private GameObject[] _hitEffectPrefabs;

        // 적·보스 공격 목록에서 엔티티에 담을 공격 값을 꺼내고, 투사체마다 View와 피격 이펙트를 표에 올린다.
        private static EnemyAttackComponent[] ReadAttacks(string definitionName, EnemyAttackEntry[] entries, List<GameObject> projectilePrefabs, List<GameObject> hitEffectPrefabs)
        {
            EnemyAttackComponent[] attacks = new EnemyAttackComponent[entries.Length];
            for (int attackIndex = 0; attackIndex < entries.Length; attackIndex++)
            {
                ref readonly EnemyAttackEntry entry = ref entries[attackIndex];
                attacks[attackIndex] = entry.attack;
                bool hasProjectile = entry.attack.kind != EnemyAttackKind.Body;
                AssignProjectileViews($"{definitionName} 공격 {attackIndex}번", hasProjectile, entry.projectilePrefab, entry.hitEffectPrefab, ref attacks[attackIndex].projectile, projectilePrefabs, hitEffectPrefabs);
            }

            return attacks;
        }

        // 투사체 View 표와 피격 이펙트 표에 프리팹을 올리고(이미 있으면 그 번호를 쓰고) 번호를 투사체 값에 넣는다.
        // 투사체가 없거나 프리팹이 빠져 있으면 번호는 -1이고, 그 투사체나 이펙트는 보이지 않는다. 표시용이라 판은 시작한다.
        private static void AssignProjectileViews(string ownerName, bool hasProjectile, GameObject projectilePrefab, GameObject hitEffectPrefab, ref ProjectileComponent projectile, List<GameObject> projectilePrefabs, List<GameObject> hitEffectPrefabs)
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

#if UNITY_EDITOR
        // 살아 있는 투사체마다 판정 구를 Logic 위치에 흰색으로 그린다.
        private void DrawProjectileGizmos()
        {
            ProjectileEntity[] entities = _world.projectiles.entities;
            Gizmos.color = Color.white;
            for (int index = 0; index < entities.Length; index++)
            {
                if (entities[index].isAlive)
                {
                    Gizmos.DrawWireSphere(entities[index].position.current, entities[index].projectile.radius);
                }
            }
        }
#endif
    }
}
