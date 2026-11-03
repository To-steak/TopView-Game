using System.Collections.Generic;
using TopView.Components;
using TopView.Entities;
using TopView.Records;
using TopView.Requests;
using UnityEngine;

namespace TopView.Systems
{
    // 투사체 명중과 수류탄 폭발을 XZ 평면의 원으로 판정해 피해 요청을 넣는다. Straight 투사체는 대상의 높이 범위도 본다. 공격의 진영이 맞힐 쪽의 목록과 피해 요청 큐를 정한다.
    public static class ProjectileHitSystem
    {
        /// <summary>
        /// Straight 투사체는 이번 틱의 이동 선분에서 가장 먼저 닿은 대상 하나에, 시간이 된 Grenade는 폭발 반경 안의 대상 모두에게 피해 요청을 넣고 사라진다.
        /// </summary>
        /// <param name="projectiles">투사체 저장소</param>
        /// <param name="playerHitTargets">플레이어 피격 대상 목록</param>
        /// <param name="playerHitTargetCount">플레이어 피격 대상 목록에서 이번 틱에 채운 대상 수</param>
        /// <param name="enemyHitTargets">적 피격 대상 목록</param>
        /// <param name="enemyHitTargetCount">적 피격 대상 목록에서 이번 틱에 채운 대상 수</param>
        /// <param name="playerDamageRequests">플레이어 피해 요청 큐</param>
        /// <param name="enemyDamageRequests">적 피해 요청 큐</param>
        /// <param name="hitRecords">명중·폭발 기록</param>
        public static void Tick(ProjectileStorage projectiles, HitTargetComponent[] playerHitTargets, int playerHitTargetCount, HitTargetComponent[] enemyHitTargets, int enemyHitTargetCount, Queue<DamageRequest> playerDamageRequests, Queue<DamageRequest> enemyDamageRequests, List<ProjectileHitRecord> hitRecords)
        {
            for (int index = 0; index < projectiles.entities.Length; index++)
            {
                ref readonly ProjectileEntity entity = ref projectiles.entities[index];
                if (!entity.isAlive)
                {
                    continue;
                }

                bool isPlayerAttack = entity.payload.faction == Faction.Player;
                HitTargetComponent[] hitTargets = isPlayerAttack ? enemyHitTargets : playerHitTargets;
                int hitTargetCount = isPlayerAttack ? enemyHitTargetCount : playerHitTargetCount;
                Queue<DamageRequest> damageRequests = isPlayerAttack ? enemyDamageRequests : playerDamageRequests;

                if (entity.projectile.kind == ProjectileKind.Grenade)
                {
                    if (entity.motion.elapsedTicks >= entity.projectile.fuseTicks)
                    {
                        Explode(in entity.projectile, in entity.position, in entity.payload, hitTargets, hitTargetCount, damageRequests, hitRecords);
                        ProjectileSystem.Despawn(projectiles, index);
                    }
                    continue;
                }

                if (HitNearest(in entity.projectile, in entity.position, in entity.payload, hitTargets, hitTargetCount, damageRequests, hitRecords))
                {
                    ProjectileSystem.Despawn(projectiles, index);
                }
            }
        }

        // 이동 선분에서 가장 먼저 닿는 대상 하나에게 피해를 준다. 같은 비율이면 목록에서 앞선 대상이 맞는다.
        private static bool HitNearest(in ProjectileComponent projectile, in PositionComponent position, in AttackPayloadComponent payload, HitTargetComponent[] hitTargets, int hitTargetCount, Queue<DamageRequest> damageRequests, List<ProjectileHitRecord> hitRecords)
        {
            float nearestFraction = float.MaxValue;
            int nearestTargetIndex = -1;

            for (int targetIndex = 0; targetIndex < hitTargetCount; targetIndex++)
            {
                ref readonly HitTargetComponent hitTarget = ref hitTargets[targetIndex];
                if (TryHitSegment(position.previous, position.current, hitTarget.position, projectile.radius + hitTarget.radius, out float hitFraction)
                    && IsWithinHeight(in projectile, in position, in hitTarget, hitFraction)
                    && hitFraction < nearestFraction)
                {
                    nearestFraction = hitFraction;
                    nearestTargetIndex = targetIndex;
                }
            }

            if (nearestTargetIndex < 0)
            {
                return false;
            }

            damageRequests.Enqueue(new DamageRequest { target = hitTargets[nearestTargetIndex].target, amount = payload.damage });
            hitRecords.Add(new ProjectileHitRecord
            {
                position = Vector3.Lerp(position.previous, position.current, nearestFraction),
                hitEffectIndex = projectile.hitEffectIndex
            });
            return true;
        }

        private static void Explode(in ProjectileComponent projectile, in PositionComponent position, in AttackPayloadComponent payload, HitTargetComponent[] hitTargets, int hitTargetCount, Queue<DamageRequest> damageRequests, List<ProjectileHitRecord> hitRecords)
        {
            for (int targetIndex = 0; targetIndex < hitTargetCount; targetIndex++)
            {
                ref readonly HitTargetComponent hitTarget = ref hitTargets[targetIndex];
                if (TargetingUtility.IsWithin(position.current, hitTarget.position, projectile.explosionRadius + hitTarget.radius))
                {
                    damageRequests.Enqueue(new DamageRequest { target = hitTarget.target, amount = payload.damage });
                }
            }

            hitRecords.Add(new ProjectileHitRecord { position = position.current, hitEffectIndex = projectile.hitEffectIndex });
        }

        // 이동 선분에서 hitFraction 지점의 투사체 구가 대상의 높이 범위에 닿는지.
        private static bool IsWithinHeight(in ProjectileComponent projectile, in PositionComponent position, in HitTargetComponent hitTarget, float hitFraction)
        {
            float height = Mathf.Lerp(position.previous.y, position.current.y, hitFraction);
            return height + projectile.radius >= hitTarget.bottomHeight
                && height - projectile.radius <= hitTarget.topHeight;
        }

        // 선분 from→to(XZ)가 center를 중심으로 한 반지름 radius의 원에 닿는지. 닿으면 선분에서 가장 가까운 점의 비율(0~1)을 돌려준다.
        private static bool TryHitSegment(Vector3 from, Vector3 to, Vector3 center, float radius, out float hitFraction)
        {
            Vector2 start = new Vector2(from.x, from.z);
            Vector2 segment = new Vector2(to.x, to.z) - start;
            Vector2 toCenter = new Vector2(center.x, center.z) - start;
            float segmentLengthSquared = segment.sqrMagnitude;

            hitFraction = segmentLengthSquared > 0f ? Mathf.Clamp01(Vector2.Dot(toCenter, segment) / segmentLengthSquared) : 0f;
            Vector2 closestOffset = toCenter - segment * hitFraction;
            return closestOffset.sqrMagnitude <= radius * radius;
        }
    }
}
