using System.Collections.Generic;
using TopView.Components;
using TopView.Entities;
using TopView.Records;
using TopView.Requests;
using TopView.Utilities;
using UnityEngine;

namespace TopView.Systems
{
    // 투사체 명중과 폭발을 XZ 평면의 원으로 판정해 피해 요청을 넣는다. 직선 투사체는 대상의 높이 범위도 본다. 공격의 진영이 맞힐 쪽의 목록과 피해 요청 큐를 정한다.
    public static class ProjectileHitSystem
    {
        // 직선 투사체가 가진 컴포넌트
        private const ComponentMask STRAIGHT_MASK = ComponentMask.Position | ComponentMask.StraightFlight | ComponentMask.StraightMotion | ComponentMask.ProjectileHit | ComponentMask.Explosion | ComponentMask.ProjectileView | ComponentMask.AttackPayload;
        // 포물선 투사체가 가진 컴포넌트
        private const ComponentMask ARC_MASK = ComponentMask.Position | ComponentMask.ArcMotion | ComponentMask.Explosion | ComponentMask.ProjectileView | ComponentMask.AttackPayload;

        /// <summary>
        /// 직선 투사체는 이번 틱의 이동 선분에서 가장 먼저 닿은 대상 하나에 피해 요청을 넣고 소멸을 요청한다. 벽에 막혀 멈춘 투사체는 맞은 대상이 없으면 닿은 자리에 명중 기록을 남기고 소멸을 요청한다. 아무것도 맞히지 않고 수명이 다한 투사체는 소멸만 요청한다.
        /// 폭발 반지름이 0보다 큰 직선 투사체는 대상이나 벽에 닿은 자리에서 폭발해 반경 안의 대상 모두에게 피해 요청을 넣고 소멸을 요청한다.
        /// 멈춘(착탄점에 도착했거나 땅이나 벽에 닿은) 포물선 투사체는 폭발시켜 반경 안의 대상 모두에게 피해 요청을 넣고 소멸을 요청한다.
        /// </summary>
        /// <param name="storage">엔티티 저장소. 직선 투사체(Position, StraightFlight, StraightMotion, ProjectileHit, Explosion, ProjectileView, AttackPayload)와 포물선 투사체(Position, ArcMotion, Explosion, ProjectileView, AttackPayload)를 읽기만 한다</param>
        /// <param name="playerHitTargets">플레이어 피격 대상 목록</param>
        /// <param name="playerHitTargetCount">플레이어 피격 대상 목록에서 이번 틱에 채운 대상 수</param>
        /// <param name="enemyHitTargets">적 피격 대상 목록</param>
        /// <param name="enemyHitTargetCount">적 피격 대상 목록에서 이번 틱에 채운 대상 수</param>
        /// <param name="playerDamageRequests">플레이어 피해 요청 큐</param>
        /// <param name="enemyDamageRequests">적 피해 요청 큐</param>
        /// <param name="hitRecords">명중·폭발 기록</param>
        /// <param name="despawnRequests">투사체 소멸 요청 큐</param>
        public static void Tick(EntityStorage storage, HitTargetComponent[] playerHitTargets, int playerHitTargetCount, HitTargetComponent[] enemyHitTargets, int enemyHitTargetCount, Queue<DamageRequest> playerDamageRequests, Queue<DamageRequest> enemyDamageRequests, List<ProjectileHitRecord> hitRecords, Queue<ProjectileDespawnRequest> despawnRequests)
        {
            for (int index = 0; index < storage.entities.Length; index++)
            {
                ref readonly Entity entity = ref storage.entities[index];
                bool isStraight = (entity.mask & STRAIGHT_MASK) == STRAIGHT_MASK;
                bool isRestingArc = (entity.mask & ARC_MASK) == ARC_MASK && entity.arcMotion.isResting;
                if (!isStraight && !isRestingArc)
                {
                    continue;
                }

                bool isPlayerAttack = entity.payload.faction == Faction.Player;
                HitTargetComponent[] hitTargets = isPlayerAttack ? enemyHitTargets : playerHitTargets;
                int hitTargetCount = isPlayerAttack ? enemyHitTargetCount : playerHitTargetCount;
                Queue<DamageRequest> damageRequests = isPlayerAttack ? enemyDamageRequests : playerDamageRequests;

                if (isRestingArc)
                {
                    Explode(in entity.explosion, entity.projectileView.hitEffectIndex, entity.position.current, in entity.payload, hitTargets, hitTargetCount, -1, damageRequests, hitRecords);
                    despawnRequests.Enqueue(new ProjectileDespawnRequest { target = new EntityIdentifier { index = index, generation = entity.generation } });
                    continue;
                }

                bool hasTarget = TryFindNearest(in entity.projectileHit, in entity.position, hitTargets, hitTargetCount, out int nearestTargetIndex, out float nearestFraction);
                // 대상에도 벽에도 닿지 않았으면 계속 날아간다. 수명이 다했으면 이번 틱의 이동 구간까지 판정한 뒤 여기서 소멸한다
                if (!hasTarget && !entity.straightMotion.isResting)
                {
                    if (entity.straightMotion.elapsedTicks >= entity.straightFlight.lifetimeTicks)
                    {
                        despawnRequests.Enqueue(new ProjectileDespawnRequest { target = new EntityIdentifier { index = index, generation = entity.generation } });
                    }
                    continue;
                }

                // 대상에 닿았으면 닿은 지점, 아니면 벽에 막혀 멈춘 자리
                Vector3 impactPosition = hasTarget ? Vector3.Lerp(entity.position.previous, entity.position.current, nearestFraction) : entity.position.current;
                if (entity.explosion.radius > 0f)
                {
                    // 맞힌 대상은 폭발 반지름이 판정 반지름보다 작아 폭발 원 밖에 있어도 피해를 받는다
                    Explode(in entity.explosion, entity.projectileView.hitEffectIndex, impactPosition, in entity.payload, hitTargets, hitTargetCount, hasTarget ? nearestTargetIndex : -1, damageRequests, hitRecords);
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
                despawnRequests.Enqueue(new ProjectileDespawnRequest { target = new EntityIdentifier { index = index, generation = entity.generation } });
            }
        }

        // 대상 하나에게 피해를 주고 닿은 자리에 명중 기록을 남긴다.
        private static void HitTarget(int hitEffectIndex, Vector3 impactPosition, in AttackPayloadComponent payload, in HitTargetComponent hitTarget, Queue<DamageRequest> damageRequests, List<ProjectileHitRecord> hitRecords)
        {
            damageRequests.Enqueue(new DamageRequest { target = hitTarget.target, amount = payload.damage });
            hitRecords.Add(new ProjectileHitRecord { position = impactPosition, hitEffectIndex = hitEffectIndex });
        }

        // 이동 선분에서 가장 먼저 닿는 대상과 그 지점의 비율(0~1)을 찾는다. 같은 비율이면 목록에서 앞선 대상을 고른다. 없으면 false
        private static bool TryFindNearest(in ProjectileHitComponent hit, in PositionComponent position, HitTargetComponent[] hitTargets, int hitTargetCount, out int nearestTargetIndex, out float nearestFraction)
        {
            nearestFraction = float.MaxValue;
            nearestTargetIndex = -1;

            for (int targetIndex = 0; targetIndex < hitTargetCount; targetIndex++)
            {
                ref readonly HitTargetComponent hitTarget = ref hitTargets[targetIndex];
                if (TryHitSegment(position.previous, position.current, hitTarget.position, hit.radius + hitTarget.radius, out float hitFraction)
                    && IsWithinHeight(in hit, in position, in hitTarget, hitFraction)
                    && hitFraction < nearestFraction)
                {
                    nearestFraction = hitFraction;
                    nearestTargetIndex = targetIndex;
                }
            }

            return nearestTargetIndex >= 0;
        }

        // center에서 폭발해 반지름 안(XZ)의 대상 모두에게 피해를 준다. directTargetIndex의 대상(투사체가 직접 맞힌 대상, 없으면 -1)은 반지름과 상관없이 피해를 준다.
        private static void Explode(in ExplosionComponent explosion, int hitEffectIndex, Vector3 center, in AttackPayloadComponent payload, HitTargetComponent[] hitTargets, int hitTargetCount, int directTargetIndex, Queue<DamageRequest> damageRequests, List<ProjectileHitRecord> hitRecords)
        {
            for (int targetIndex = 0; targetIndex < hitTargetCount; targetIndex++)
            {
                ref readonly HitTargetComponent hitTarget = ref hitTargets[targetIndex];
                if (targetIndex == directTargetIndex || TargetingUtility.IsWithin(center, hitTarget.position, explosion.radius + hitTarget.radius))
                {
                    damageRequests.Enqueue(new DamageRequest { target = hitTarget.target, amount = payload.damage });
                }
            }

            hitRecords.Add(new ProjectileHitRecord { position = center, hitEffectIndex = hitEffectIndex });
        }

        // 이동 선분에서 hitFraction 지점의 투사체 구가 대상의 높이 범위에 닿는지.
        private static bool IsWithinHeight(in ProjectileHitComponent hit, in PositionComponent position, in HitTargetComponent hitTarget, float hitFraction)
        {
            float height = Mathf.Lerp(position.previous.y, position.current.y, hitFraction);
            return height + hit.radius >= hitTarget.bottomHeight
                && height - hit.radius <= hitTarget.topHeight;
        }

        // 선분 from→to(XZ)가 center를 중심으로 한 반지름 radius의 원에 닿는지. 닿으면 원에 처음 들어가는 점의 비율(0~1)을 돌려준다.
        // 가장 가까운 점의 비율로 고르면, 큰 원에 먼저 들어가도 가장 가까운 점이 뒤쪽이라 뒤의 작은 원을 먼저 맞힌 것으로 본다.
        private static bool TryHitSegment(Vector3 from, Vector3 to, Vector3 center, float radius, out float hitFraction)
        {
            Vector2 start = new Vector2(from.x, from.z);
            Vector2 segment = new Vector2(to.x, to.z) - start;
            Vector2 toCenter = new Vector2(center.x, center.z) - start;
            hitFraction = 0f;

            // 시작점이 이미 원 안이면 시작점에서 닿는다
            float startOutside = toCenter.sqrMagnitude - radius * radius;
            if (startOutside <= 0f)
            {
                return true;
            }

            float segmentLengthSquared = segment.sqrMagnitude;
            if (segmentLengthSquared <= 0f)
            {
                return false;
            }

            // |start + segment × t - center|² = radius²의 작은 근이 원에 들어가는 비율이다
            float projection = Vector2.Dot(toCenter, segment);
            float discriminant = projection * projection - segmentLengthSquared * startOutside;
            if (discriminant < 0f)
            {
                return false;
            }

            float entryFraction = (projection - Mathf.Sqrt(discriminant)) / segmentLengthSquared;
            if (entryFraction < 0f || entryFraction > 1f)
            {
                return false;
            }

            hitFraction = entryFraction;
            return true;
        }
    }
}
