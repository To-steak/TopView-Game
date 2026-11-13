using System.Collections.Generic;
using TopView.Components;
using TopView.Definitions;
using TopView.Presentation.Views;
using UnityEngine;

namespace TopView.Core
{
    // 투사체: 투사체 View 표와 피격 이펙트 표 만들기
    public partial class World
    {
        // 투사체 View 표와 피격 이펙트 표. 판을 준비할 때 Definition에서 모은다
        private GameObject[] _projectilePrefabs;
        private GameObject[] _hitEffectPrefabs;

        // 적 공격 목록에서 엔티티에 담을 공격 값을 꺼내고, 투사체마다 View와 피격 이펙트를 표에 올린다. 발 목록이 잘못된 투사체 공격은 빼고 담는다.
        // 공격의 애니메이션과 소리는 담은 공격과 같은 번호로 따로 돌려준다.
        private static EnemyAttackComponent[] ReadAttacks(string definitionName, EnemyAttackEntry[] entries, List<GameObject> projectilePrefabs, List<GameObject> hitEffectPrefabs, out EnemyAttackAnimation[] attackAnimations, out AudioClip[] attackClips)
        {
            List<EnemyAttackComponent> attacks = new List<EnemyAttackComponent>(entries.Length);
            List<EnemyAttackAnimation> animations = new List<EnemyAttackAnimation>(entries.Length);
            List<AudioClip> clips = new List<AudioClip>(entries.Length);
            for (int attackIndex = 0; attackIndex < entries.Length; attackIndex++)
            {
                ref readonly EnemyAttackEntry entry = ref entries[attackIndex];
                string ownerName = $"{definitionName} 공격 {attackIndex}번";
                bool hasProjectile = entry.attack.kind != EnemyAttackKind.Body;
                if (hasProjectile && TryFindShotsProblem(entry.attack.straight.shots, true, entry.attack.durationTicks, out string shotsProblem))
                {
                    Debug.LogWarning($"World >> {ownerName}의 {shotsProblem}. 이 공격을 건너뛰었다.");
                    continue;
                }

                EnemyAttackComponent attack = entry.attack;
                AssignProjectileViews(ownerName, hasProjectile, entry.projectilePrefab, entry.hitEffectPrefab, ref attack.projectileView, projectilePrefabs, hitEffectPrefabs);
                attacks.Add(attack);
                animations.Add(entry.animation);
                clips.Add(entry.attackClip);
            }

            attackAnimations = animations.ToArray();
            attackClips = clips.ToArray();
            return attacks.ToArray();
        }

        // 발 목록이 비었거나, Start Ticks가 오름차순이 아니거나, hasDuration이면 Duration Ticks보다 큰 칸이 있으면 문제를 적어 true를 돌려준다.
        private static bool TryFindShotsProblem(ProjectileShotComponent[] shots, bool hasDuration, int durationTicks, out string problem)
        {
            problem = null;
            if (shots == null || shots.Length == 0)
            {
                problem = "Shots가 비어 있다";
                return true;
            }

            for (int shotIndex = 0; shotIndex < shots.Length; shotIndex++)
            {
                int startTicks = shots[shotIndex].startTicks;
                if (shotIndex > 0 && startTicks < shots[shotIndex - 1].startTicks)
                {
                    problem = $"Shots {shotIndex}번 칸의 Start Ticks({startTicks})가 앞 칸({shots[shotIndex - 1].startTicks})보다 작다";
                    return true;
                }

                if (hasDuration && startTicks > durationTicks)
                {
                    problem = $"Shots {shotIndex}번 칸의 Start Ticks({startTicks})가 Duration Ticks({durationTicks})보다 크다";
                    return true;
                }
            }

            return false;
        }

        // 투사체 View 표와 피격 이펙트 표에 프리팹을 올리고(이미 있으면 그 번호를 쓰고) 번호를 투사체 View 값에 넣는다.
        // 투사체가 없거나 프리팹이 빠져 있으면 번호는 -1이고, 그 투사체나 이펙트는 보이지 않는다. 표시용이라 판은 시작한다.
        private static void AssignProjectileViews(string ownerName, bool hasProjectile, GameObject projectilePrefab, GameObject hitEffectPrefab, ref ProjectileViewComponent projectileView, List<GameObject> projectilePrefabs, List<GameObject> hitEffectPrefabs)
        {
            projectileView.viewIndex = -1;
            projectileView.hitEffectIndex = -1;
            if (!hasProjectile)
            {
                return;
            }

            if (projectilePrefab == null || projectilePrefab.GetComponent<ProjectileView>() == null)
            {
                Debug.LogWarning($"World >> {ownerName}의 투사체 프리팹이 없거나 ProjectileView가 붙어 있지 않아 이 투사체는 보이지 않게 진행한다.");
            }
            else
            {
                projectileView.viewIndex = projectilePrefabs.IndexOf(projectilePrefab);
                if (projectileView.viewIndex < 0)
                {
                    projectileView.viewIndex = projectilePrefabs.Count;
                    projectilePrefabs.Add(projectilePrefab);
                }
            }

            projectileView.hitEffectIndex = IndexHitEffect($"{ownerName}의 피격 이펙트", hitEffectPrefab, hitEffectPrefabs);
        }

        // 피격 이펙트 표에 프리팹을 올리고(이미 있으면 그 번호를 쓰고) 번호를 돌려준다.
        // 프리팹이 없으면 -1이다. ParticleSystem이 없으면 경고를 남기고 -1을 돌려주며, 그 이펙트는 나오지 않는다. 표시용이라 판은 시작한다.
        private static int IndexHitEffect(string effectName, GameObject effectPrefab, List<GameObject> hitEffectPrefabs)
        {
            if (effectPrefab == null)
            {
                return -1;
            }

            if (effectPrefab.GetComponent<ParticleSystem>() == null)
            {
                Debug.LogWarning($"World >> {effectName} 프리팹에 ParticleSystem이 없어 이 이펙트는 나오지 않게 진행한다.");
                return -1;
            }

            int effectIndex = hitEffectPrefabs.IndexOf(effectPrefab);
            if (effectIndex < 0)
            {
                effectIndex = hitEffectPrefabs.Count;
                hitEffectPrefabs.Add(effectPrefab);
            }

            return effectIndex;
        }
    }
}
