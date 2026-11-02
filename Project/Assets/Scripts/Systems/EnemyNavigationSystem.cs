using TopView.Entities;
using UnityEngine;
using UnityEngine.AI;

namespace TopView.Systems
{
    // 적을 NavMesh 경로를 따라 옮기고, 서로 겹치지 않게 밀어낸다. NavMesh는 질의로만 쓴다.
    public static class EnemyNavigationSystem
    {
        // 꺾이는 점에 이만큼 가까워지면 다음 점으로 넘어간다(유닛)
        private const float CORNER_REACH_DISTANCE = 0.1f;
        // 이보다 느리면 도는 방향을 정하지 않는다(유닛²/초²)
        private const float MIN_TURN_SPEED_SQUARED = 0.0001f;
        // 두 적이 이보다 가까우면 밀어낼 방향을 정할 수 없어 건너뛴다(유닛)
        private const float MIN_SEPARATION_DISTANCE = 0.00001f;

        /// <summary>
        /// 목적지가 바뀐 적은 경로를 다시 구한 뒤 경로를 따라 옮긴다. 마지막에 걷는 적끼리 겹친 만큼 밀어내고 NavMesh 안으로 되돌린다.
        /// </summary>
        /// <param name="enemies">적 저장소</param>
        /// <param name="path">경로 계산에 다시 쓰는 NavMeshPath</param>
        /// <param name="navMeshSampleDistance">목적지를 NavMesh 위로 맞출 때 찾는 최대 거리(유닛)</param>
        public static void Tick(EnemyStorage enemies, NavMeshPath path, float navMeshSampleDistance)
        {
            float deltaTime = Time.fixedDeltaTime;
            int failedPathCount = 0;

            for (int enemyIndex = 0; enemyIndex < enemies.entities.Length; enemyIndex++)
            {
                ref EnemyEntity enemy = ref enemies.entities[enemyIndex];
                if (!enemy.isAlive)
                {
                    continue;
                }

                if (enemy.agent.hasDestination && enemy.agent.isPathDirty && !TryRecalculatePath(ref enemy, path, navMeshSampleDistance))
                {
                    failedPathCount++;
                }

                Move(ref enemy, deltaTime);
            }

            if (failedPathCount > 0)
            {
                Debug.LogWarning($"EnemyNavigationSystem >> 경로를 찾지 못해 바로 도착한 것으로 본 적 {failedPathCount}마리가 있다.");
            }

            Separate(enemies);

            for (int enemyIndex = 0; enemyIndex < enemies.entities.Length; enemyIndex++)
            {
                ref EnemyEntity enemy = ref enemies.entities[enemyIndex];
                if (enemy.isAlive && enemy.agent.hasDestination && NavMesh.Raycast(enemy.position.previous, enemy.position.current, out NavMeshHit hit, NavMesh.AllAreas))
                {
                    enemy.position.current = hit.position;
                }
            }
        }

        /// <summary>
        /// 새 목적지를 정한다. 경로는 다음 Tick에 다시 구하고, 도착 여부를 지운다.
        /// </summary>
        /// <param name="enemy">적</param>
        /// <param name="destination">목적지(월드)</param>
        /// <param name="stoppingDistance">남은 경로가 이 거리 안이면 도착한 것으로 본다(유닛)</param>
        public static void SetDestination(ref EnemyEntity enemy, Vector3 destination, float stoppingDistance)
        {
            enemy.agent.hasDestination = true;
            enemy.agent.destination = destination;
            enemy.agent.stoppingDistance = stoppingDistance;
            enemy.agent.isPathDirty = true;
            enemy.agent.hasReachedDestination = false;
        }

        /// <summary>
        /// 목적지와 경로, 속도를 지워 멈춘다.
        /// </summary>
        /// <param name="enemy">적</param>
        public static void StopMove(ref EnemyEntity enemy)
        {
            enemy.agent.hasDestination = false;
            enemy.agent.isPathDirty = false;
            enemy.agent.hasReachedDestination = false;
            enemy.agent.pathCornerCount = 0;
            enemy.agent.pathCornerIndex = 0;
            enemy.agent.velocity = Vector3.zero;
        }

        // 목적지까지의 경로를 다시 구한다. 구하지 못하면 꺾이는 점이 없는 채로 false를 돌려주고, Move가 도착한 것으로 본다.
        private static bool TryRecalculatePath(ref EnemyEntity enemy, NavMeshPath path, float navMeshSampleDistance)
        {
            enemy.agent.isPathDirty = false;
            enemy.agent.pathCornerCount = 0;
            enemy.agent.pathCornerIndex = 0;

            Vector3 destination = enemy.agent.destination;
            if (NavMesh.SamplePosition(destination, out NavMeshHit destinationHit, navMeshSampleDistance, NavMesh.AllAreas))
            {
                destination = destinationHit.position;
            }

            if (!NavMesh.CalculatePath(enemy.position.current, destination, NavMesh.AllAreas, path) || path.status == NavMeshPathStatus.PathInvalid)
            {
                return false;
            }

            enemy.agent.pathCornerCount = path.GetCornersNonAlloc(enemy.agent.pathCorners);
            enemy.agent.pathCornerIndex = enemy.agent.pathCornerCount > 1 ? 1 : enemy.agent.pathCornerCount;
            return true;
        }

        // 남은 경로가 멈추는 거리 안이면 멈춘다. 아니면 다음 꺾이는 점 쪽으로 가속·도착 감속하며 움직이고, 진행 방향으로 돈다.
        private static void Move(ref EnemyEntity enemy, float deltaTime)
        {
            if (!enemy.agent.hasDestination)
            {
                return;
            }

            float remainingDistance = GetRemainingDistance(in enemy);
            if (enemy.agent.pathCornerIndex >= enemy.agent.pathCornerCount || remainingDistance <= enemy.agent.stoppingDistance)
            {
                enemy.agent.velocity = Vector3.zero;
                enemy.agent.hasReachedDestination = true;
                return;
            }

            enemy.agent.hasReachedDestination = false;

            float brakingSpeed = Mathf.Sqrt(2f * enemy.behavior.acceleration * (remainingDistance - enemy.agent.stoppingDistance));
            float desiredSpeed = Mathf.Min(enemy.agent.maxSpeed, brakingSpeed);
            Vector3 toCorner = enemy.agent.pathCorners[enemy.agent.pathCornerIndex] - enemy.position.current;
            Vector3 desiredVelocity = toCorner.normalized * desiredSpeed;
            enemy.agent.velocity = Vector3.MoveTowards(enemy.agent.velocity, desiredVelocity, enemy.behavior.acceleration * deltaTime);

            Vector3 step = enemy.agent.velocity * deltaTime;
            if (step.sqrMagnitude >= toCorner.sqrMagnitude)
            {
                enemy.position.current = enemy.agent.pathCorners[enemy.agent.pathCornerIndex];
                enemy.agent.pathCornerIndex++;
            }
            else
            {
                enemy.position.current += step;
                if ((enemy.agent.pathCorners[enemy.agent.pathCornerIndex] - enemy.position.current).sqrMagnitude <= CORNER_REACH_DISTANCE * CORNER_REACH_DISTANCE)
                {
                    enemy.agent.pathCornerIndex++;
                }
            }

            Vector3 flatVelocity = enemy.agent.velocity;
            flatVelocity.y = 0f;
            if (flatVelocity.sqrMagnitude > MIN_TURN_SPEED_SQUARED)
            {
                enemy.rotation.current = Quaternion.RotateTowards(enemy.rotation.current, Quaternion.LookRotation(flatVelocity), enemy.behavior.angularSpeed * deltaTime);
            }
        }

        private static float GetRemainingDistance(in EnemyEntity enemy)
        {
            if (enemy.agent.pathCornerIndex >= enemy.agent.pathCornerCount)
            {
                return 0f;
            }

            float distance = Vector3.Distance(enemy.position.current, enemy.agent.pathCorners[enemy.agent.pathCornerIndex]);
            for (int cornerIndex = enemy.agent.pathCornerIndex + 1; cornerIndex < enemy.agent.pathCornerCount; cornerIndex++)
            {
                distance += Vector3.Distance(enemy.agent.pathCorners[cornerIndex - 1], enemy.agent.pathCorners[cornerIndex]);
            }

            return distance;
        }

        // 두 적이 반지름 합보다 가까우면 겹친 만큼 XZ로 밀어낸다. 목적지가 있는(걷는) 적만 밀려나며, 둘 다 걸으면 반씩 나눈다.
        private static void Separate(EnemyStorage enemies)
        {
            EnemyEntity[] entities = enemies.entities;
            for (int firstIndex = 0; firstIndex < entities.Length; firstIndex++)
            {
                ref EnemyEntity first = ref entities[firstIndex];
                if (!first.isAlive)
                {
                    continue;
                }

                for (int secondIndex = firstIndex + 1; secondIndex < entities.Length; secondIndex++)
                {
                    ref EnemyEntity second = ref entities[secondIndex];
                    if (!second.isAlive || (!first.agent.hasDestination && !second.agent.hasDestination))
                    {
                        continue;
                    }

                    Vector3 delta = second.position.current - first.position.current;
                    delta.y = 0f;
                    float distance = delta.magnitude;
                    float minimumDistance = first.stats.hitRadius + second.stats.hitRadius;
                    if (distance >= minimumDistance || distance < MIN_SEPARATION_DISTANCE)
                    {
                        continue;
                    }

                    Vector3 push = delta / distance * (minimumDistance - distance);
                    if (first.agent.hasDestination && second.agent.hasDestination)
                    {
                        first.position.current -= push * 0.5f;
                        second.position.current += push * 0.5f;
                    }
                    else if (first.agent.hasDestination)
                    {
                        first.position.current -= push;
                    }
                    else
                    {
                        second.position.current += push;
                    }
                }
            }
        }
    }
}
