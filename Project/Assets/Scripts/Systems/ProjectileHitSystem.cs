using System.Collections.Generic;
using TopView.Components;
using TopView.Entities;
using TopView.Records;
using TopView.Requests;
using TopView.Utilities;
using UnityEngine;

namespace TopView.Systems
{
    // 투사체 명중과 폭발을 XZ 평면의 원으로 판정해 피해 요청을 넣는다. Straight 투사체는 대상의 높이 범위도 본다. 공격의 진영이 맞힐 쪽의 목록과 피해 요청 큐를 정한다.
    public static class ProjectileHitSystem
    {
        /// <summary>
        /// Straight 투사체는 이번 틱의 이동 선분에서 가장 먼저 닿은 대상 하나에, 멈춘(착탄점에 도착했거나 땅이나 벽에 닿은) Grenade는 폭발 반경 안의 대상 모두에게 피해 요청을 넣고 소멸을 요청한다. 벽에 막혀 멈춘 Straight 투사체는 맞은 대상이 없으면 닿은 자리에 명중 기록을 남기고 소멸을 요청한다.
        /// 폭발 반지름이 0보다 큰 Straight 투사체는 대상이나 벽에 닿은 자리에서 폭발해 반경 안의 대상 모두에게 피해 요청을 넣고 소멸을 요청한다.
        /// </summary>
        /// <param name="projectiles">투사체 저장소</param>
        /// <param name="playerHitTargets">플레이어 피격 대상 목록</param>
        /// <param name="playerHitTargetCount">플레이어 피격 대상 목록에서 이번 틱에 채운 대상 수</param>
        /// <param name="enemyHitTargets">적 피격 대상 목록</param>
        /// <param name="enemyHitTargetCount">적 피격 대상 목록에서 이번 틱에 채운 대상 수</param>
        /// <param name="playerDamageRequests">플레이어 피해 요청 큐</param>
        /// <param name="enemyDamageRequests">적 피해 요청 큐</param>
        /// <param name="hitRecords">명중·폭발 기록</param>
        /// <param name="despawnRequests">투사체 소멸 요청 큐</param>
        public static void Tick(ProjectileStorage projectiles, HitTargetComponent[] playerHitTargets, int playerHitTargetCount, HitTargetComponent[] enemyHitTargets, int enemyHitTargetCount, Queue<DamageRequest> playerDamageRequests, Queue<DamageRequest> enemyDamageRequests, List<ProjectileHitRecord> hitRecords, Queue<ProjectileDespawnRequest> despawnRequests)
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
                EntityIdentifier identifier = new EntityIdentifier { index = index, generation = entity.generation };

                if (entity.projectile.kind == ProjectileKind.Grenade)
                {
                    if (entity.motion.isResting)
                    {
                        Explode(in entity.projectile, entity.projectileView.hitEffectIndex, entity.position.current, in entity.payload, hitTargets, hitTargetCount, damageRequests, hitRecords);
                        despawnRequests.Enqueue(new ProjectileDespawnRequest { target = identifier });
                    }
                    continue;
                }

                bool hasTarget = TryFindNearest(in entity.projectile, in entity.position, hitTargets, hitTargetCount, out int nearestTargetIndex, out float nearestFraction);
                // 대상에도 벽에도 닿지 않았으면 계속 날아간다
                if (!hasTarget && !entity.motion.isResting)
                {
                    continue;
                }

                // 대상에 닿았으면 닿은 지점, 아니면 벽에 막혀 멈춘 자리
                Vector3 impactPosition = hasTarget ? Vector3.Lerp(entity.position.previous, entity.position.current, nearestFraction) : entity.position.current;
                if (entity.projectile.explosionRadius > 0f)
                {
                    Explode(in entity.projectile, entity.projectileView.hitEffectIndex, impactPosition, in entity.payload, hitTargets, hitTargetCount, damageRequests, hitRecords);
                }
                else if (hasTarget)
                {
                    HitTarget(entity.projectileView.hitEffectIndex, impactPosition, in entity.payload, in hitTargets[nearestTargetIndex], damageRequests, hitRecords);
                }
                else
                {
                    // 맞은 대상 없이 벽에 막혀 멈췄으면 닿은 자리에 피격 이펙트만 남긴다
                    hitRecords.Add(new ProjectileHitRecord { position = impactPosition, hitEffectIndex = entity.projectileView.hitEffectIndex });
                }
                despawnRequests.Enqueue(new ProjectileDespawnRequest { target = identifier });
            }
        }

        // 대상 하나에게 피해를 주고 닿은 자리에 명중 기록을 남긴다.
        private static void HitTarget(int hitEffectIndex, Vector3 impactPosition, in AttackPayloadComponent payload, in HitTargetComponent hitTarget, Queue<DamageRequest> damageRequests, List<ProjectileHitRecord> hitRecords)
        {
            damageRequests.Enqueue(new DamageRequest { target = hitTarget.target, amount = payload.damage });
            hitRecords.Add(new ProjectileHitRecord { position = impactPosition, hitEffectIndex = hitEffectIndex });
        }

        // 이동 선분에서 가장 먼저 닿는 대상과 그 지점의 비율(0~1)을 찾는다. 같은 비율이면 목록에서 앞선 대상을 고른다. 없으면 false
        private static bool TryFindNearest(in ProjectileComponent projectile, in PositionComponent position, HitTargetComponent[] hitTargets, int hitTargetCount, out int nearestTargetIndex, out float nearestFraction)
        {
            nearestFraction = float.MaxValue;
            nearestTargetIndex = -1;

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

            return nearestTargetIndex >= 0;
        }

        // center에서 폭발해 반지름 안(XZ)의 대상 모두에게 피해를 준다.
        private static void Explode(in ProjectileComponent projectile, int hitEffectIndex, Vector3 center, in AttackPayloadComponent payload, HitTargetComponent[] hitTargets, int hitTargetCount, Queue<DamageRequest> damageRequests, List<ProjectileHitRecord> hitRecords)
        {
            for (int targetIndex = 0; targetIndex < hitTargetCount; targetIndex++)
            {
                ref readonly HitTargetComponent hitTarget = ref hitTargets[targetIndex];
                if (TargetingUtility.IsWithin(center, hitTarget.position, projectile.explosionRadius + hitTarget.radius))
                {
                    damageRequests.Enqueue(new DamageRequest { target = hitTarget.target, amount = payload.damage });
                }
            }

            hitRecords.Add(new ProjectileHitRecord { position = center, hitEffectIndex = hitEffectIndex });
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
