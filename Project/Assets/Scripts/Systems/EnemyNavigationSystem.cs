using TopView.Components;
using TopView.Entities;
using UnityEngine;
using UnityEngine.AI;

namespace TopView.Systems
{
    // 길찾기로 움직이는 엔티티를 NavMesh 경로를 따라 옮기고, 서로 겹치지 않게 밀어낸다. NavMesh는 질의로만 쓴다.
    public static class EnemyNavigationSystem
    {
        // 꺾이는 점에 이만큼 가까워지면 다음 점으로 넘어간다(유닛)
        private const float CORNER_REACH_DISTANCE = 0.1f;
        // 이보다 느리면 도는 방향을 정하지 않는다(유닛²/초²)
        private const float MIN_TURN_SPEED_SQUARED = 0.0001f;
        // 두 엔티티가 이보다 가까우면 밀어낼 방향을 정할 수 없어 건너뛴다(유닛)
        private const float MIN_SEPARATION_DISTANCE = 0.00001f;

        // 경로를 따라 움직이는 엔티티가 가지는 컴포넌트
        private const ComponentMask MOVE_QUERY = ComponentMask.Position | ComponentMask.Rotation | ComponentMask.NavigationAgent | ComponentMask.EnemyBehavior;
        // 서로 밀어내는 엔티티가 가지는 컴포넌트
        private const ComponentMask SEPARATE_QUERY = ComponentMask.Position | ComponentMask.NavigationAgent | ComponentMask.EnemyStats;

        /// <summary>
        /// 목적지가 바뀐 엔티티는 경로를 다시 구한 뒤 경로를 따라 옮긴다. 마지막에 걷는 엔티티끼리 겹친 만큼 밀어내고 NavMesh 안으로 되돌린다.
        /// </summary>
        /// <param name="masks">칸별 컴포넌트 표시</param>
        /// <param name="positions">위치</param>
        /// <param name="rotations">회전</param>
        /// <param name="agents">목적지와 경로</param>
        /// <param name="behaviors">적 행동 값</param>
        /// <param name="stats">피격 반지름 값</param>
        /// <param name="path">경로 계산에 다시 쓰는 NavMeshPath</param>
        /// <param name="navMeshSampleDistance">목적지를 NavMesh 위로 맞출 때 찾는 최대 거리(유닛)</param>
        public static void Tick(ComponentMask[] masks, PositionComponent[] positions, RotationComponent[] rotations, NavigationAgentComponent[] agents, EnemyBehaviorComponent[] behaviors, EnemyStatsComponent[] stats, NavMeshPath path, float navMeshSampleDistance)
        {
            float deltaTime = Time.fixedDeltaTime;
            int failedPathCount = 0;

            for (int index = 0; index < masks.Length; index++)
            {
                if ((masks[index] & MOVE_QUERY) != MOVE_QUERY)
                {
                    continue;
                }

                ref NavigationAgentComponent agent = ref agents[index];
                if (agent.hasDestination && agent.isPathDirty && !TryRecalculatePath(ref agent, positions[index].current, path, navMeshSampleDistance))
                {
                    failedPathCount++;
                }

                Move(ref agent, in behaviors[index], ref positions[index], ref rotations[index], deltaTime);
            }

            if (failedPathCount > 0)
            {
                Debug.LogWarning($"EnemyNavigationSystem >> 경로를 찾지 못해 바로 도착한 것으로 본 엔티티 {failedPathCount}개가 있다.");
            }

            Separate(masks, positions, agents, stats);

            for (int index = 0; index < masks.Length; index++)
            {
                if ((masks[index] & MOVE_QUERY) == MOVE_QUERY && agents[index].hasDestination && NavMesh.Raycast(positions[index].previous, positions[index].current, out NavMeshHit hit, NavMesh.AllAreas))
                {
                    positions[index].current = hit.position;
                }
            }
        }

        /// <summary>
        /// 새 목적지를 정한다. 경로는 다음 Tick에 다시 구하고, 도착 여부를 지운다.
        /// </summary>
        /// <param name="agent">목적지와 경로</param>
        /// <param name="destination">목적지(월드)</param>
        /// <param name="stoppingDistance">남은 경로가 이 거리 안이면 도착한 것으로 본다(유닛)</param>
        public static void SetDestination(ref NavigationAgentComponent agent, Vector3 destination, float stoppingDistance)
        {
            agent.hasDestination = true;
            agent.destination = destination;
            agent.stoppingDistance = stoppingDistance;
            agent.isPathDirty = true;
            agent.hasReachedDestination = false;
        }

        /// <summary>
        /// 목적지와 경로, 속도를 지워 멈춘다.
        /// </summary>
        /// <param name="agent">목적지와 경로</param>
        public static void StopMove(ref NavigationAgentComponent agent)
        {
            agent.hasDestination = false;
            agent.isPathDirty = false;
            agent.hasReachedDestination = false;
            agent.pathCornerCount = 0;
            agent.pathCornerIndex = 0;
            agent.velocity = Vector3.zero;
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
        private static void Move(ref NavigationAgentComponent agent, in EnemyBehaviorComponent behavior, ref PositionComponent position, ref RotationComponent rotation, float deltaTime)
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

            float brakingSpeed = Mathf.Sqrt(2f * behavior.acceleration * (remainingDistance - agent.stoppingDistance));
            float desiredSpeed = Mathf.Min(agent.maxSpeed, brakingSpeed);
            Vector3 toCorner = agent.pathCorners[agent.pathCornerIndex] - position.current;
            Vector3 desiredVelocity = toCorner.normalized * desiredSpeed;
            agent.velocity = Vector3.MoveTowards(agent.velocity, desiredVelocity, behavior.acceleration * deltaTime);

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
                rotation.current = Quaternion.RotateTowards(rotation.current, Quaternion.LookRotation(flatVelocity), behavior.angularSpeed * deltaTime);
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

        // 두 엔티티가 반지름 합보다 가까우면 겹친 만큼 XZ로 밀어낸다. 목적지가 있는(걷는) 쪽만 밀려나며, 둘 다 걸으면 반씩 나눈다.
        private static void Separate(ComponentMask[] masks, PositionComponent[] positions, NavigationAgentComponent[] agents, EnemyStatsComponent[] stats)
        {
            for (int firstIndex = 0; firstIndex < masks.Length; firstIndex++)
            {
                if ((masks[firstIndex] & SEPARATE_QUERY) != SEPARATE_QUERY)
                {
                    continue;
                }

                bool isFirstWalking = agents[firstIndex].hasDestination;
                for (int secondIndex = firstIndex + 1; secondIndex < masks.Length; secondIndex++)
                {
                    if ((masks[secondIndex] & SEPARATE_QUERY) != SEPARATE_QUERY)
                    {
                        continue;
                    }

                    bool isSecondWalking = agents[secondIndex].hasDestination;
                    if (!isFirstWalking && !isSecondWalking)
                    {
                        continue;
                    }

                    Vector3 delta = positions[secondIndex].current - positions[firstIndex].current;
                    delta.y = 0f;
                    float distance = delta.magnitude;
                    float minimumDistance = stats[firstIndex].hitRadius + stats[secondIndex].hitRadius;
                    if (distance >= minimumDistance || distance < MIN_SEPARATION_DISTANCE)
                    {
                        continue;
                    }

                    Vector3 push = delta / distance * (minimumDistance - distance);
                    if (isFirstWalking && isSecondWalking)
                    {
                        positions[firstIndex].current -= push * 0.5f;
                        positions[secondIndex].current += push * 0.5f;
                    }
                    else if (isFirstWalking)
                    {
                        positions[firstIndex].current -= push;
                    }
                    else
                    {
                        positions[secondIndex].current += push;
                    }
                }
            }
        }
    }
}
