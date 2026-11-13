using TopView.Components;
using TopView.Entities;
using UnityEngine;
using UnityEngine.AI;

namespace TopView.Systems
{
    // 길찾기로 움직이는 엔티티를 NavMesh 경로를 따라 옮긴다. 적끼리는 겹쳐도 밀어내지 않는다. NavMesh는 질의로만 쓴다.
    public static class EnemyNavigationSystem
    {
        // 처리할 적이 가진 컴포넌트
        private const ComponentMask ENEMY_MASK = ComponentMask.Position | ComponentMask.Rotation | ComponentMask.NavigationMovement | ComponentMask.NavigationAgent;

        // 꺾이는 점에 이만큼 가까워지면 다음 점으로 넘어간다(유닛)
        private const float CORNER_REACH_DISTANCE = 0.1f;
        // 이보다 느리면 도는 방향을 정하지 않는다(유닛²/초²)
        private const float MIN_TURN_SPEED_SQUARED = 0.0001f;

        /// <summary>
        /// 목적지가 바뀐 적은 경로를 다시 구한 뒤 경로를 따라 옮긴다.
        /// </summary>
        /// <param name="storage">엔티티 저장소. Position, Rotation, NavigationMovement, NavigationAgent를 가진 칸만 처리하고, 경로와 위치, 회전을 고친다</param>
        /// <param name="path">경로 계산에 다시 쓰는 NavMeshPath</param>
        /// <param name="navMeshSampleDistance">목적지를 NavMesh 위로 맞출 때 찾는 최대 거리(유닛)</param>
        public static void Tick(EntityStorage storage, NavMeshPath path, float navMeshSampleDistance)
        {
            Entity[] entities = storage.entities;
            float deltaTime = Time.fixedDeltaTime;
            int failedPathCount = 0;

            for (int index = 0; index < entities.Length; index++)
            {
                ref Entity entity = ref entities[index];
                if ((entity.mask & ENEMY_MASK) != ENEMY_MASK)
                {
                    continue;
                }

                if (entity.navigationAgent.hasDestination && entity.navigationAgent.isPathDirty && !TryRecalculatePath(ref entity.navigationAgent, entity.position.current, path, navMeshSampleDistance))
                {
                    failedPathCount++;
                }

                Move(ref entity.navigationAgent, in entity.navigationMovement, ref entity.position, ref entity.rotation, deltaTime);
            }

            if (failedPathCount > 0)
            {
                Debug.LogWarning($"EnemyNavigationSystem >> 경로를 찾지 못해 바로 도착한 것으로 본 적 {failedPathCount}마리가 있다.");
            }
        }

        // 목적지까지의 경로를 다시 구한다. 구하지 못하면 꺾이는 점이 없는 채로 false를 돌려주고, Move가 도착한 것으로 본다.
        private static bool TryRecalculatePath(ref NavigationAgentComponent agent, Vector3 position, NavMeshPath path, float navMeshSampleDistance)
        {
            agent.isPathDirty = false;
            agent.pathCornerCount = 0;
            agent.pathCornerIndex = 0;

            Vector3 destination = agent.destination;
            if (NavMesh.SamplePosition(destination, out NavMeshHit destinationHit, navMeshSampleDistance, NavMesh.AllAreas))
            {
                destination = destinationHit.position;
            }

            if (!NavMesh.CalculatePath(position, destination, NavMesh.AllAreas, path) || path.status == NavMeshPathStatus.PathInvalid)
            {
                return false;
            }

            agent.pathCornerCount = path.GetCornersNonAlloc(agent.pathCorners);
            agent.pathCornerIndex = agent.pathCornerCount > 1 ? 1 : agent.pathCornerCount;
            return true;
        }

        // 남은 경로가 멈추는 거리 안이면 멈춘다. 아니면 다음 꺾이는 점 쪽으로 가속·도착 감속하며 움직이고, 진행 방향으로 돈다.
        private static void Move(ref NavigationAgentComponent agent, in NavigationMovementComponent movement, ref PositionComponent position, ref RotationComponent rotation, float deltaTime)
        {
            if (!agent.hasDestination)
            {
                return;
            }

            float remainingDistance = GetRemainingDistance(in agent, position.current);
            if (agent.pathCornerIndex >= agent.pathCornerCount || remainingDistance <= agent.stoppingDistance)
            {
                agent.velocity = Vector3.zero;
                agent.hasReachedDestination = true;
                return;
            }

            agent.hasReachedDestination = false;

            float brakingSpeed = Mathf.Sqrt(2f * movement.acceleration * (remainingDistance - agent.stoppingDistance));
            float desiredSpeed = Mathf.Min(agent.maxSpeed, brakingSpeed);
            Vector3 toCorner = agent.pathCorners[agent.pathCornerIndex] - position.current;
            Vector3 desiredVelocity = toCorner.normalized * desiredSpeed;
            agent.velocity = Vector3.MoveTowards(agent.velocity, desiredVelocity, movement.acceleration * deltaTime);

            Vector3 step = agent.velocity * deltaTime;
            if (step.sqrMagnitude >= toCorner.sqrMagnitude)
            {
                position.current = agent.pathCorners[agent.pathCornerIndex];
                agent.pathCornerIndex++;
            }
            else
            {
                position.current += step;
                if ((agent.pathCorners[agent.pathCornerIndex] - position.current).sqrMagnitude <= CORNER_REACH_DISTANCE * CORNER_REACH_DISTANCE)
                {
                    agent.pathCornerIndex++;
                }
            }

            Vector3 flatVelocity = agent.velocity;
            flatVelocity.y = 0f;
            if (flatVelocity.sqrMagnitude > MIN_TURN_SPEED_SQUARED)
            {
                rotation.current = Quaternion.RotateTowards(rotation.current, Quaternion.LookRotation(flatVelocity), movement.angularSpeed * deltaTime);
            }
        }

        private static float GetRemainingDistance(in NavigationAgentComponent agent, Vector3 position)
        {
            if (agent.pathCornerIndex >= agent.pathCornerCount)
            {
                return 0f;
            }

            float distance = Vector3.Distance(position, agent.pathCorners[agent.pathCornerIndex]);
            for (int cornerIndex = agent.pathCornerIndex + 1; cornerIndex < agent.pathCornerCount; cornerIndex++)
            {
                distance += Vector3.Distance(agent.pathCorners[cornerIndex - 1], agent.pathCorners[cornerIndex]);
            }

            return distance;
        }
    }
}
