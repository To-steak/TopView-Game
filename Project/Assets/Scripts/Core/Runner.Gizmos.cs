#if UNITY_EDITOR
using TopView.Components;
using TopView.Entities;
using TopView.Utilities;
using UnityEngine;

namespace TopView.Core
{
    // 디버그 표시: 플레이 중 Logic 값을 그리는 요소별 Gizmo를 여기에 둔다. 배치할 때 봐야 하는 Gizmo는 Editor/에 둔다
    public partial class Runner
    {
        private const int CIRCLE_GIZMO_SEGMENTS = 32;
        private const float DROPPED_LOOT_GIZMO_RADIUS = 0.3f;

        private void OnDrawGizmos()
        {
            if (_world == null)
            {
                return;
            }

            DrawPlayerGizmos();
            DrawEnemyGizmos();
            DrawMeleeGizmos();
            DrawProjectileGizmos();
            DrawLootGizmos();
            DrawDroppedLootGizmos();
        }

        // 플레이어 이동 충돌에 쓰는 캡슐을 Logic 위치에 그린다. 착지 중이면 초록, 공중이면 노랑
        private void DrawPlayerGizmos()
        {
            PlayerEntity[] entities = _world.players.entities;
            for (int index = 0; index < entities.Length; index++)
            {
                ref readonly PlayerEntity entity = ref entities[index];
                if (!entity.isAlive)
                {
                    continue;
                }

                PlayerMovementUtility.GetCapsulePoints(in entity.movement, entity.rotation.current, entity.position.current, out Vector3 top, out Vector3 bottom);
                float radius = entity.movement.capsuleRadius;
                Gizmos.color = entity.motion.isGrounded ? Color.green : Color.yellow;
                Gizmos.DrawWireSphere(top, radius);
                Gizmos.DrawWireSphere(bottom, radius);
                Gizmos.DrawLine(top + Vector3.forward * radius, bottom + Vector3.forward * radius);
                Gizmos.DrawLine(top + Vector3.back * radius, bottom + Vector3.back * radius);
                Gizmos.DrawLine(top + Vector3.right * radius, bottom + Vector3.right * radius);
                Gizmos.DrawLine(top + Vector3.left * radius, bottom + Vector3.left * radius);
            }
        }

        // 살아 있는 적마다 감지 반경(노랑), 멈추는 거리(주황), 피격 판정 원(청록), Body 공격 판정 원(빨강)을 Logic 위치에 XZ 원으로 그린다.
        // 감지와 공격 판정은 여기에 플레이어 반지름을 더해 비교하므로, 원은 플레이어 중심이 아니라 가장자리가 닿는 기준이다
        private void DrawEnemyGizmos()
        {
            EnemyEntity[] entities = _world.enemies.entities;
            for (int index = 0; index < entities.Length; index++)
            {
                ref readonly EnemyEntity entity = ref entities[index];
                if (!entity.isAlive)
                {
                    continue;
                }

                Vector3 position = entity.position.current;
                Gizmos.color = Color.yellow;
                DrawCircleGizmo(position, entity.behavior.detectionRadius);
                Gizmos.color = new Color(1f, 0.5f, 0f);
                DrawCircleGizmo(position, entity.behavior.stopDistance);
                Gizmos.color = Color.cyan;
                DrawCircleGizmo(position, entity.stats.hitRadius);
                DrawBodyAttackGizmos(entity.attackList.attacks, position, entity.rotation.current);
            }
        }

        // 공격 목록의 Body 공격마다 공격이 나갈 때의 판정 원을 빨강으로 그린다.
        private static void DrawBodyAttackGizmos(EnemyAttackComponent[] attacks, Vector3 position, Quaternion rotation)
        {
            Gizmos.color = Color.red;
            for (int attackIndex = 0; attackIndex < attacks.Length; attackIndex++)
            {
                ref readonly EnemyAttackComponent attack = ref attacks[attackIndex];
                if (attack.kind == EnemyAttackKind.Body)
                {
                    DrawCircleGizmo(position + rotation * Vector3.forward * attack.bodyOffset, attack.bodyRadius);
                }
            }
        }

        // 지금 살아 있는 근접 판정마다 판정 원을 자홍으로 그린다. 플레이어와 적의 근접 공격이 모두 여기에 나온다.
        private void DrawMeleeGizmos()
        {
            MeleeEntity[] entities = _world.melees.entities;
            Gizmos.color = Color.magenta;
            for (int index = 0; index < entities.Length; index++)
            {
                if (entities[index].isAlive)
                {
                    DrawCircleGizmo(entities[index].position.current, entities[index].melee.radius);
                }
            }
        }

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

        // 살아 있는 플레이어마다 전리품 획득 반경을 Logic 위치에 초록색 XZ 원으로 그린다.
        private void DrawLootGizmos()
        {
            PlayerEntity[] players = _world.players.entities;
            Gizmos.color = Color.green;
            for (int playerIndex = 0; playerIndex < players.Length; playerIndex++)
            {
                if (players[playerIndex].isAlive)
                {
                    DrawCircleGizmo(players[playerIndex].position.current, _world.lootRule.pickupRadius);
                }
            }
        }

        // 떨어져 있는 전리품마다 Logic 위치를 파란색 XZ 원으로 그린다.
        private void DrawDroppedLootGizmos()
        {
            LootEntity[] entities = _world.loots.entities;
            Gizmos.color = Color.blue;
            for (int index = 0; index < entities.Length; index++)
            {
                if (entities[index].isAlive)
                {
                    DrawCircleGizmo(entities[index].position.current, DROPPED_LOOT_GIZMO_RADIUS);
                }
            }
        }

        // center 높이의 XZ 평면에 반지름 radius의 원을 지금 Gizmos 색으로 그린다.
        private static void DrawCircleGizmo(Vector3 center, float radius)
        {
            Vector3 previousPoint = center + new Vector3(radius, 0f, 0f);
            for (int segmentIndex = 1; segmentIndex <= CIRCLE_GIZMO_SEGMENTS; segmentIndex++)
            {
                float angle = segmentIndex * Mathf.PI * 2f / CIRCLE_GIZMO_SEGMENTS;
                Vector3 point = center + new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
                Gizmos.DrawLine(previousPoint, point);
                previousPoint = point;
            }
        }
    }
}
#endif
