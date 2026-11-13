using TopView.Components;
using TopView.Entities;
using TopView.Requests;
using UnityEngine;

namespace TopView.Utilities
{
    // 판정 방식별 공격이 언제 나가는지와 무엇을 내보내는지 정하는 도구. 플레이어 무기와 적 공격이 함께 쓴다.
    // 강화처럼 값을 바꿔야 하면 부르는 쪽이 공격 값을 복사해 고친 뒤 넘긴다. 요청을 큐에 넣는 일은 부르는 System이 한다.
    public static class AttackUtility
    {
        /// <summary>
        /// 근접 공격 한 번에 내보내는 발 수를 돌려준다. 판정 하나라 항상 1이다.
        /// </summary>
        /// <param name="attack">근접 공격</param>
        /// <returns>1</returns>
        public static int GetShotCount(in MeleeAttackComponent attack)
        {
            return 1;
        }

        /// <summary>
        /// 직선 투사체 공격 한 번에 내보내는 발 수를 돌려준다.
        /// </summary>
        /// <param name="attack">직선 투사체 공격</param>
        /// <returns>shots의 칸 수. 비어 있으면 0</returns>
        public static int GetShotCount(in StraightAttackComponent attack)
        {
            return attack.shots != null ? attack.shots.Length : 0;
        }

        /// <summary>
        /// 포물선 투사체 공격 한 번에 내보내는 발 수를 돌려준다.
        /// </summary>
        /// <param name="attack">포물선 투사체 공격</param>
        /// <returns>shots의 칸 수. 비어 있으면 0</returns>
        public static int GetShotCount(in ArcAttackComponent attack)
        {
            return attack.shots != null ? attack.shots.Length : 0;
        }

        /// <summary>
        /// 근접 판정이 지금 나갈 때인지 판정한다.
        /// </summary>
        /// <param name="attack">근접 공격</param>
        /// <param name="committedShotCount">이번 공격 상태에서 이미 내보낸 발 수</param>
        /// <param name="stateElapsedTicks">공격 상태에 들어간 뒤 지난 시간(틱)</param>
        /// <returns>아직 나가지 않았고 startTicks가 지났으면 true</returns>
        public static bool IsShotDue(in MeleeAttackComponent attack, int committedShotCount, int stateElapsedTicks)
        {
            return committedShotCount == 0 && stateElapsedTicks >= attack.startTicks;
        }

        /// <summary>
        /// 직선 투사체의 다음 발이 지금 나갈 때인지 판정한다. 같은 틱에 시점이 겹친 발은 부르는 쪽이 되풀이해 함께 내보낸다.
        /// </summary>
        /// <param name="attack">직선 투사체 공격</param>
        /// <param name="committedShotCount">이번 공격 상태에서 이미 내보낸 발 수. 다음에 내보낼 shots 칸 번호다</param>
        /// <param name="stateElapsedTicks">공격 상태에 들어간 뒤 지난 시간(틱)</param>
        /// <returns>남은 발이 있고 그 발의 startTicks가 지났으면 true</returns>
        public static bool IsShotDue(in StraightAttackComponent attack, int committedShotCount, int stateElapsedTicks)
        {
            return committedShotCount < GetShotCount(in attack) && stateElapsedTicks >= attack.shots[committedShotCount].startTicks;
        }

        /// <summary>
        /// 포물선 투사체의 다음 발이 지금 나갈 때인지 판정한다. 같은 틱에 시점이 겹친 발은 부르는 쪽이 되풀이해 함께 내보낸다.
        /// </summary>
        /// <param name="attack">포물선 투사체 공격</param>
        /// <param name="committedShotCount">이번 공격 상태에서 이미 내보낸 발 수. 다음에 내보낼 shots 칸 번호다</param>
        /// <param name="stateElapsedTicks">공격 상태에 들어간 뒤 지난 시간(틱)</param>
        /// <returns>남은 발이 있고 그 발의 startTicks가 지났으면 true</returns>
        public static bool IsShotDue(in ArcAttackComponent attack, int committedShotCount, int stateElapsedTicks)
        {
            return committedShotCount < GetShotCount(in attack) && stateElapsedTicks >= attack.shots[committedShotCount].startTicks;
        }

        /// <summary>
        /// 한 발이 생기는 위치를 구한다.
        /// </summary>
        /// <param name="shot">발의 시점과 위치</param>
        /// <param name="position">내보내는 쪽의 위치(월드)</param>
        /// <param name="rotation">내보내는 쪽의 회전</param>
        /// <returns>발이 생기는 위치(월드)</returns>
        public static Vector3 GetShotPosition(in ProjectileShotComponent shot, Vector3 position, Quaternion rotation)
        {
            return position + rotation * shot.spawnOffset;
        }

        /// <summary>
        /// 근접 판정 생성 요청을 만든다. 판정은 endTicks까지 이어진다.
        /// </summary>
        /// <param name="attack">근접 공격</param>
        /// <param name="position">공격하는 쪽의 위치(월드)</param>
        /// <param name="rotation">공격하는 쪽의 회전</param>
        /// <param name="owner">공격한 엔티티</param>
        /// <param name="faction">공격한 쪽의 진영</param>
        /// <param name="damage">맞을 때의 피해량</param>
        /// <returns>근접 판정 생성 요청. 판정 원의 중심은 center에 있다</returns>
        public static MeleeSpawnRequest CreateSpawnRequest(in MeleeAttackComponent attack, Vector3 position, Quaternion rotation, in EntityIdentifier owner, Faction faction, float damage)
        {
            return new MeleeSpawnRequest
            {
                owner = owner,
                faction = faction,
                center = MeleeShapeUtility.GetCenter(position, rotation, attack.shape.offset),
                radius = attack.shape.radius,
                damage = damage,
                durationTicks = attack.endTicks - attack.startTicks
            };
        }

        /// <summary>
        /// 직선 투사체 한 발의 생성 요청을 만든다. shotRotation의 정면으로 날아간다.
        /// </summary>
        /// <param name="attack">직선 투사체 공격</param>
        /// <param name="projectileView">투사체를 무엇으로 그릴지</param>
        /// <param name="shotPosition">발이 생기는 위치(월드)</param>
        /// <param name="shotRotation">발이 날아갈 방향의 회전</param>
        /// <param name="faction">공격한 쪽의 진영</param>
        /// <param name="damage">맞을 때의 피해량</param>
        /// <returns>직선 투사체 생성 요청</returns>
        public static StraightProjectileSpawnRequest CreateSpawnRequest(in StraightAttackComponent attack, in ProjectileViewComponent projectileView, Vector3 shotPosition, Quaternion shotRotation, Faction faction, float damage)
        {
            return new StraightProjectileSpawnRequest
            {
                projectile = attack.projectile,
                projectileView = projectileView,
                faction = faction,
                damage = damage,
                position = shotPosition,
                rotation = shotRotation,
                velocity = shotRotation * Vector3.forward * attack.projectile.flight.speed
            };
        }

        /// <summary>
        /// 포물선 투사체 한 발의 생성 요청을 만든다. targetPosition에 떨어진다.
        /// </summary>
        /// <param name="attack">포물선 투사체 공격</param>
        /// <param name="projectileView">투사체를 무엇으로 그릴지</param>
        /// <param name="shotPosition">발이 생기는 위치(월드)</param>
        /// <param name="shotRotation">발의 회전</param>
        /// <param name="targetPosition">착탄점(월드)</param>
        /// <param name="faction">공격한 쪽의 진영</param>
        /// <param name="damage">맞을 때의 피해량</param>
        /// <returns>포물선 투사체 생성 요청</returns>
        public static ArcProjectileSpawnRequest CreateSpawnRequest(in ArcAttackComponent attack, in ProjectileViewComponent projectileView, Vector3 shotPosition, Quaternion shotRotation, Vector3 targetPosition, Faction faction, float damage)
        {
            return new ArcProjectileSpawnRequest
            {
                projectile = attack.projectile,
                projectileView = projectileView,
                faction = faction,
                damage = damage,
                position = shotPosition,
                rotation = shotRotation,
                targetPosition = targetPosition
            };
        }
    }
}
