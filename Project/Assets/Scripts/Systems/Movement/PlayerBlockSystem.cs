using TopView.Components;
using TopView.Entities;
using TopView.Utilities;
using UnityEngine;

namespace TopView.Systems
{
    // 플레이어가 적의 피격 원 쪽으로 파고들지 않게 막는다.
    public static class PlayerBlockSystem
    {
        // 플레이어를 막는 적이 가진 컴포넌트
        private const ComponentMask ENEMY_MASK = ComponentMask.EnemyKind | ComponentMask.Position | ComponentMask.Body;

        // 플레이어와 적의 중심이 이보다 가까우면 밀어낼 방향을 정할 수 없어 건너뛴다(유닛)
        private const float MIN_BLOCK_DISTANCE = 0.00001f;

        // 처리할 플레이어가 가진 컴포넌트
        private const ComponentMask PLAYER_MASK = ComponentMask.PlayerActionState | ComponentMask.Position | ComponentMask.Rotation | ComponentMask.PlayerMovement;

        /// <summary>
        /// 이번 틱에 적의 피격 원 쪽으로 가까워졌다면 원 밖(XZ)으로 되돌린다. 적이 다가와 겹친 경우는 밀어내지 않는다.
        /// 밀어낸 자리에서 캡슐이 지형과 겹치면 밀어내지 않고 지난 틱 위치(XZ)로 되돌린다.
        /// </summary>
        /// <param name="storage">엔티티 저장소. 플레이어(PlayerActionState, Position, Rotation, PlayerMovement)를 처리해 position을 고치고, 적(EnemyKind, Position, Body)을 읽는다</param>
        public static void Tick(EntityStorage storage)
        {
            for (int index = 0; index < storage.entities.Length; index++)
            {
                ref Entity entity = ref storage.entities[index];
                if ((entity.mask & PLAYER_MASK) == PLAYER_MASK)
                {
                    BlockByEnemies(ref entity.position, in entity.playerMovement, entity.rotation.current, storage.entities);
                }
            }
        }

        private static void BlockByEnemies(ref PositionComponent position, in PlayerMovementComponent movement, Quaternion rotation, Entity[] entities)
        {
            for (int index = 0; index < entities.Length; index++)
            {
                ref readonly Entity enemy = ref entities[index];
                if ((enemy.mask & ENEMY_MASK) == ENEMY_MASK)
                {
                    BlockByCircle(ref position, in movement, rotation, enemy.position.current, movement.capsuleRadius + enemy.body.radius);
                }
            }
        }

        private static void BlockByCircle(ref PositionComponent position, in PlayerMovementComponent movement, Quaternion rotation, Vector3 center, float minimumDistance)
        {
            Vector3 offset = position.current - center;
            offset.y = 0f;
            float distance = offset.magnitude;
            if (distance >= minimumDistance || distance < MIN_BLOCK_DISTANCE)
            {
                return;
            }

            Vector3 previousOffset = position.previous - center;
            previousOffset.y = 0f;
            if (distance * distance >= previousOffset.sqrMagnitude)
            {
                return;
            }

            Vector3 blockedOffset = offset / distance * minimumDistance;
            Vector3 blockedPosition = new Vector3(center.x + blockedOffset.x, position.current.y, center.z + blockedOffset.z);
            // 밀어낸 자리가 벽과 겹치면 다음 이동의 CapsuleCast가 이미 겹친 벽을 보지 못해 벽을 통과하므로, 지난 틱 자리로 되돌린다
            if (IsOverlappingTerrain(in movement, rotation, blockedPosition))
            {
                blockedPosition = new Vector3(position.previous.x, position.current.y, position.previous.z);
            }
            position.current = blockedPosition;
        }

        // 그 자리의 캡슐이 지형(collisionLayer)과 겹치는지. 피부 두께만큼 줄여 벽면에 닿은 것은 겹친 것으로 보지 않는다.
        private static bool IsOverlappingTerrain(in PlayerMovementComponent movement, Quaternion rotation, Vector3 position)
        {
            PlayerMovementUtility.GetCapsulePoints(in movement, rotation, position, out Vector3 top, out Vector3 bottom);
            float radius = Mathf.Max(movement.capsuleRadius - movement.skinWidth, 0f);
            return Physics.CheckCapsule(top, bottom, radius, movement.collisionLayer, QueryTriggerInteraction.Ignore);
        }
    }
}
