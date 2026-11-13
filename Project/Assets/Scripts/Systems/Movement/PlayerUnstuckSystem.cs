using System.Collections.Generic;
using TopView.Commands;
using TopView.Components;
using TopView.Entities;
using TopView.Utilities;
using UnityEngine;
using UnityEngine.AI;

namespace TopView.Systems
{
    // 탈출 명령을 처리한다. 땅 아래로 빠진 플레이어를 가까운 NavMesh 위로 옮긴다.
    public static class PlayerUnstuckSystem
    {
        // 처리할 플레이어가 가진 컴포넌트
        private const ComponentMask PLAYER_MASK = ComponentMask.PlayerActionState | ComponentMask.Position | ComponentMask.Rotation | ComponentMask.PlayerMovement | ComponentMask.PlayerMotion;

        /// <summary>
        /// 탈출 명령을 모두 소비해 살아 있는 플레이어를 가까운 NavMesh 위로 옮긴다. 진행 중이 아니면 명령만 버린다.
        /// 일시정지 화면에서 들어온 명령을 처리하므로 PauseSystem 뒤에서 일시정지와 상관없이 부른다.
        /// </summary>
        /// <param name="storage">엔티티 저장소. 플레이어(PlayerActionState, Position, Rotation, PlayerMovement, PlayerMotion)만 처리한다</param>
        /// <param name="progress">한 판의 진행 상태</param>
        /// <param name="rule">한 판의 규칙 값</param>
        /// <param name="playerUnstuckCommands">플레이어 탈출 명령 목록</param>
        public static void Tick(EntityStorage storage, in GameProgressComponent progress, in GameRuleComponent rule, List<PlayerUnstuckCommand> playerUnstuckCommands)
        {
            // 판이 끝난 뒤에 늦게 도착한 명령은 정상적으로 무시한다. 같은 틱에 여러 번 들어와도 한 번 옮긴 뒤에는 이미 NavMesh 위라 한 번만 옮긴다
            if (playerUnstuckCommands.Count > 0 && progress.phase == GamePhase.Playing)
            {
                MoveAlivePlayers(storage, in rule);
            }
            playerUnstuckCommands.Clear();
        }

        private static void MoveAlivePlayers(EntityStorage storage, in GameRuleComponent rule)
        {
            bool hasAlivePlayer = false;
            for (int index = 0; index < storage.entities.Length; index++)
            {
                ref Entity entity = ref storage.entities[index];
                if ((entity.mask & PLAYER_MASK) != PLAYER_MASK)
                {
                    continue;
                }

                hasAlivePlayer = true;
                if (!TryFindDestination(in entity.position, in entity.playerMovement, entity.rotation.current, in rule, out Vector3 destination))
                {
                    Debug.LogWarning($"PlayerUnstuckSystem >> 플레이어 위치 {entity.position.current}에서 {rule.unstuckSampleDistance}유닛 안에 NavMesh가 없어 옮기지 않았다. Game Settings Definition의 Unstuck 값을 확인한다.");
                    continue;
                }

                Place(ref entity.position, ref entity.playerMotion, destination);
            }

            if (!hasAlivePlayer)
            {
                Debug.LogWarning("PlayerUnstuckSystem >> 살아 있는 플레이어가 없어 탈출 명령을 버렸다.");
            }
        }

        // 지금 XZ에서 가장 가까운 NavMesh 점을 찾고, 캡슐 바닥이 그 점 위에 skinWidth만큼 떠 있는 위치를 돌려준다.
        // 기준 높이보다 아래로 빠졌으면 기준 높이에서 찾아, 빠진 깊이와 상관없이 그 자리 위로 올린다
        private static bool TryFindDestination(in PositionComponent position, in PlayerMovementComponent movement, Quaternion rotation, in GameRuleComponent rule, out Vector3 destination)
        {
            Vector3 samplePoint = position.current;
            samplePoint.y = Mathf.Max(samplePoint.y, rule.unstuckSampleMinimumHeight);
            if (!NavMesh.SamplePosition(samplePoint, out NavMeshHit hit, rule.unstuckSampleDistance, NavMesh.AllAreas))
            {
                destination = position.current;
                return false;
            }

            PlayerMovementUtility.GetCapsulePoints(in movement, rotation, Vector3.zero, out _, out Vector3 bottom);
            float capsuleLowestHeight = bottom.y - movement.capsuleRadius;
            destination = hit.position + Vector3.up * (movement.skinWidth - capsuleLowestHeight);
            return true;
        }

        // 보간이 옛 위치에서 끌려오지 않도록 이전 위치도 함께 옮기고, 떨어지던 속도를 없앤다
        private static void Place(ref PositionComponent position, ref PlayerMotionComponent motion, Vector3 destination)
        {
            position.previous = destination;
            position.current = destination;
            motion.verticalVelocity = 0f;
        }
    }
}
