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
        /// 직전 위치와 회전을 남기고, 목적지가 바뀐 적은 경로를 다시 구한 뒤 경로를 따라 옮긴다. 마지막에 걷는 적끼리 겹친 만큼 밀어내고 NavMesh 안으로 되돌린다.
        /// </summary>
        /// <param name="enemies">적 저장소</param>
        /// <param name="path">경로 계산에 다시 쓰는 NavMeshPath</param>
        public static void Tick(EnemyStorage enemies, NavMeshPath path)
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

                enemy.previousPosition = enemy.currentPosition;
                enemy.previousRotation = enemy.currentRotation;

                if (enemy.hasDestination && enemy.isPathDirty && !TryRecalculatePath(ref enemy, path))
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
                if (enemy.isAlive && enemy.hasDestination && NavMesh.Raycast(enemy.previousPosition, enemy.currentPosition, out NavMeshHit hit, NavMesh.AllAreas))
                {
                    enemy.currentPosition = hit.position;
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
            enemy.hasDestination = true;
            enemy.destination = destination;
            enemy.stoppingDistance = stoppingDistance;
            enemy.isPathDirty = true;
            enemy.hasReachedDestination = false;
        }

        /// <summary>
        /// 목적지와 경로, 속도를 지워 멈춘다.
        /// </summary>
        /// <param name="enemy">적</param>
        public static void StopMove(ref EnemyEntity enemy)
        {
            enemy.hasDestination = false;
            enemy.isPathDirty = false;
            enemy.hasReachedDestination = false;
            enemy.pathCornerCount = 0;
            enemy.pathCornerIndex = 0;
            enemy.velocity = Vector3.zero;
        }

        // 목적지까지의 경로를 다시 구한다. 구하지 못하면 꺾이는 점이 없는 채로 false를 돌려주고, Move가 도착한 것으로 본다.
        private static bool TryRecalculatePath(ref EnemyEntity enemy, NavMeshPath path)
        {
            enemy.isPathDirty = false;
            enemy.pathCornerCount = 0;
            enemy.pathCornerIndex = 0;

            Vector3 destination = enemy.destination;
            if (NavMesh.SamplePosition(destination, out NavMeshHit destinationHit, enemy.navigation.navMeshSampleDistance, NavMesh.AllAreas))
            {
                destination = destinationHit.position;
            }

            if (!NavMesh.CalculatePath(enemy.currentPosition, destination, NavMesh.AllAreas, path) || path.status == NavMeshPathStatus.PathInvalid)
            {
                return false;
            }

            enemy.pathCornerCount = path.GetCornersNonAlloc(enemy.pathCorners);
            enemy.pathCornerIndex = enemy.pathCornerCount > 1 ? 1 : enemy.pathCornerCount;
            return true;
        }

        // 남은 경로가 멈추는 거리 안이면 멈춘다. 아니면 다음 꺾이는 점 쪽으로 가속·도착 감속하며 움직이고, 진행 방향으로 돈다.
        private static void Move(ref EnemyEntity enemy, float deltaTime)
        {
            if (!enemy.hasDestination)
            {
                return;
            }

            float remainingDistance = GetRemainingDistance(in enemy);
            if (enemy.pathCornerIndex >= enemy.pathCornerCount || remainingDistance <= enemy.stoppingDistance)
            {
                enemy.velocity = Vector3.zero;
                enemy.hasReachedDestination = true;
                return;
            }

            enemy.hasReachedDestination = false;

            float brakingSpeed = Mathf.Sqrt(2f * enemy.navigation.acceleration * (remainingDistance - enemy.stoppingDistance));
            float desiredSpeed = Mathf.Min(enemy.navigation.speed, brakingSpeed);
            Vector3 toCorner = enemy.pathCorners[enemy.pathCornerIndex] - enemy.currentPosition;
            Vector3 desiredVelocity = toCorner.normalized * desiredSpeed;
            enemy.velocity = Vector3.MoveTowards(enemy.velocity, desiredVelocity, enemy.navigation.acceleration * deltaTime);

            Vector3 step = enemy.velocity * deltaTime;
            if (step.sqrMagnitude >= toCorner.sqrMagnitude)
            {
                enemy.currentPosition = enemy.pathCorners[enemy.pathCornerIndex];
                enemy.pathCornerIndex++;
            }
            else
            {
                enemy.currentPosition += step;
                if ((enemy.pathCorners[enemy.pathCornerIndex] - enemy.currentPosition).sqrMagnitude <= CORNER_REACH_DISTANCE * CORNER_REACH_DISTANCE)
                {
                    enemy.pathCornerIndex++;
                }
            }

            Vector3 flatVelocity = enemy.velocity;
            flatVelocity.y = 0f;
            if (flatVelocity.sqrMagnitude > MIN_TURN_SPEED_SQUARED)
            {
                enemy.currentRotation = Quaternion.RotateTowards(enemy.currentRotation, Quaternion.LookRotation(flatVelocity), enemy.navigation.angularSpeed * deltaTime);
            }
        }

        private static float GetRemainingDistance(in EnemyEntity enemy)
        {
            if (enemy.pathCornerIndex >= enemy.pathCornerCount)
            {
                return 0f;
            }

            float distance = Vector3.Distance(enemy.currentPosition, enemy.pathCorners[enemy.pathCornerIndex]);
            for (int cornerIndex = enemy.pathCornerIndex + 1; cornerIndex < enemy.pathCornerCount; cornerIndex++)
            {
                distance += Vector3.Distance(enemy.pathCorners[cornerIndex - 1], enemy.pathCorners[cornerIndex]);
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
                    if (!second.isAlive || (!first.hasDestination && !second.hasDestination))
                    {
                        continue;
                    }

                    Vector3 delta = second.currentPosition - first.currentPosition;
                    delta.y = 0f;
                    float distance = delta.magnitude;
                    float minimumDistance = first.navigation.radius + second.navigation.radius;
                    if (distance >= minimumDistance || distance < MIN_SEPARATION_DISTANCE)
                    {
                        continue;
                    }

                    Vector3 push = delta / distance * (minimumDistance - distance);
                    if (first.hasDestination && second.hasDestination)
                    {
                        first.currentPosition -= push * 0.5f;
                        second.currentPosition += push * 0.5f;
                    }
                    else if (first.hasDestination)
                    {
                        first.currentPosition -= push;
                    }
                    else
                    {
                        second.currentPosition += push;
                    }
                }
            }
        }
    }
}
