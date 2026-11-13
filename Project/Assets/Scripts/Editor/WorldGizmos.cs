using TopView.Components;
using TopView.Core;
using TopView.Entities;
using TopView.Utilities;
using UnityEditor;
using UnityEngine;

namespace TopView.Editor
{
    // 플레이 중 World의 Logic 값을 씬 뷰에 그린다. 배치할 때 봐야 하는 값은 AuthoringGizmos가 그린다.
    public static class WorldGizmos
    {
        private const float DROPPED_LOOT_RADIUS = 0.3f;

        [DrawGizmo(GizmoType.Selected | GizmoType.NonSelected)]
        private static void DrawWorld(World world, GizmoType gizmoType)
        {
            if (!world.EditorIsRoundReady)
            {
                return;
            }

            DrawPlayers(world.EditorPlayers);
            DrawEnemies(world.EditorEnemies);
            DrawMelees(world.EditorMelees);
            DrawProjectiles(world.EditorProjectiles);
            DrawLootPickupRanges(world.EditorPlayers, world.EditorLootPickupRadius);
            DrawDroppedLoots(world.EditorLoots);
        }

        // 플레이어 이동 충돌에 쓰는 캡슐을 Logic 위치에 그린다. 착지 중이면 초록, 공중이면 노랑
        private static void DrawPlayers(PlayerEntity[] entities)
        {
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

        // 살아 있는 적마다 멈추는 거리(주황), 피격 판정 원(청록), Body 공격 판정 원(빨강)을 Logic 위치에 XZ 원으로 그린다.
        // 공격 판정은 여기에 플레이어 반지름을 더해 비교하므로, 원은 플레이어 중심이 아니라 가장자리가 닿는 기준이다
        private static void DrawEnemies(EnemyEntity[] entities)
        {
            for (int index = 0; index < entities.Length; index++)
            {
                ref readonly EnemyEntity entity = ref entities[index];
                if (!entity.isAlive)
                {
                    continue;
                }

                Vector3 position = entity.position.current;
                Gizmos.color = new Color(1f, 0.5f, 0f);
                GizmoShapes.DrawCircle(position, entity.movement.stopDistance);
                Gizmos.color = Color.cyan;
                GizmoShapes.DrawCircle(position, entity.body.radius);
                DrawBodyAttacks(entity.attackList.attacks, position, entity.rotation.current);
            }
        }

        // 공격 목록의 Body 공격마다 공격이 나갈 때의 판정 원을 빨강으로 그린다.
        private static void DrawBodyAttacks(EnemyAttackComponent[] attacks, Vector3 position, Quaternion rotation)
        {
            Gizmos.color = Color.red;
            for (int attackIndex = 0; attackIndex < attacks.Length; attackIndex++)
            {
                ref readonly EnemyAttackComponent attack = ref attacks[attackIndex];
                if (attack.kind == EnemyAttackKind.Body)
                {
                    GizmoShapes.DrawCircle(MeleeShapeUtility.GetCenter(position, rotation, attack.meleeShape.offset), attack.meleeShape.radius);
                }
            }
        }

        // 지금 살아 있는 근접 판정마다 판정 원을 자홍으로 그린다. 플레이어와 적의 근접 공격이 모두 여기에 나온다.
        private static void DrawMelees(MeleeEntity[] entities)
        {
            Gizmos.color = Color.magenta;
            for (int index = 0; index < entities.Length; index++)
            {
                if (entities[index].isAlive)
                {
                    GizmoShapes.DrawCircle(entities[index].position.current, entities[index].melee.radius);
                }
            }
        }

        // 살아 있는 투사체마다 판정 구를 Logic 위치에 흰색으로 그린다. 폭발 반지름이 있는 Straight는 폭발 범위를 빨간색 XZ 원으로 함께 그린다.
        // 폭발 판정은 여기에 대상 반지름을 더해 비교하므로, 원은 대상 중심이 아니라 가장자리가 닿는 기준이다
        private static void DrawProjectiles(ProjectileEntity[] entities)
        {
            for (int index = 0; index < entities.Length; index++)
            {
                ref readonly ProjectileEntity entity = ref entities[index];
                if (!entity.isAlive)
                {
                    continue;
                }

                Gizmos.color = Color.white;
                Gizmos.DrawWireSphere(entity.position.current, entity.projectile.radius);
                if (entity.projectile.kind == ProjectileKind.Straight && entity.projectile.explosionRadius > 0f)
                {
                    Gizmos.color = Color.red;
                    GizmoShapes.DrawCircle(entity.position.current, entity.projectile.explosionRadius);
                }
            }
        }

        // 살아 있는 플레이어마다 전리품 획득 반경을 Logic 위치에 초록색 XZ 원으로 그린다.
        private static void DrawLootPickupRanges(PlayerEntity[] players, float pickupRadius)
        {
            Gizmos.color = Color.green;
            for (int playerIndex = 0; playerIndex < players.Length; playerIndex++)
            {
                if (players[playerIndex].isAlive)
                {
                    GizmoShapes.DrawCircle(players[playerIndex].position.current, pickupRadius);
                }
            }
        }

        // 떨어져 있는 전리품마다 Logic 위치를 파란색 XZ 원으로 그린다.
        private static void DrawDroppedLoots(LootEntity[] entities)
        {
            Gizmos.color = Color.blue;
            for (int index = 0; index < entities.Length; index++)
            {
                if (entities[index].isAlive)
                {
                    GizmoShapes.DrawCircle(entities[index].position.current, DROPPED_LOOT_RADIUS);
                }
            }
        }
    }
}
