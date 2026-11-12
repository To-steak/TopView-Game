using TopView.Components;
using TopView.Entities;
using TopView.Utilities;
using UnityEngine;

namespace TopView.Systems
{
    // 투척 충전 중인 플레이어가 지금 던지면 수류탄이 지나갈 궤도와 폭발할 자리를 구한다. 실제 비행(ProjectileSystem)과 같은 계산을 같은 틱 간격으로 한다.
    public static class PlayerThrowPreviewSystem
    {
        /// <summary>
        /// 투척 무기로 충전 중인 플레이어는 미리보기를 채우고, 나머지는 숨긴다. 이동과 조준이 끝난 뒤에 부른다.
        /// </summary>
        /// <param name="players">플레이어 저장소</param>
        public static void Tick(PlayerStorage players)
        {
            int shortArrayCount = 0;
            int invalidFlightCount = 0;
            for (int index = 0; index < players.entities.Length; index++)
            {
                ref PlayerEntity entity = ref players.entities[index];
                if (!entity.isAlive)
                {
                    continue;
                }

                entity.throwPreview.isVisible = false;
                entity.throwPreview.pointCount = 0;
                if (entity.actionState.state != PlayerState.ThrowCharge)
                {
                    continue;
                }

                ref readonly WeaponComponent weapon = ref entity.weaponList.weapons[entity.weaponSlots.currentIndex];
                if (weapon.kind != WeaponKind.Throw)
                {
                    continue;
                }
                if (weapon.projectile.flightTicks <= 0)
                {
                    // 충전 중 매 틱 걸리므로 충전을 시작한 틱에만 센다
                    if (entity.actionState.elapsedTicks == 0)
                    {
                        invalidFlightCount++;
                    }
                    continue;
                }

                if (!Preview(ref entity.throwPreview, in weapon, entity.position.current, entity.rotation.current, entity.actionState.throwCharge))
                {
                    shortArrayCount++;
                }
            }

            if (invalidFlightCount > 0)
            {
                Debug.LogWarning($"PlayerThrowPreviewSystem >> flightTicks가 0 이하인 투척 무기라 플레이어 {invalidFlightCount}명의 미리보기를 숨겼다. flightTicks는 0보다 커야 한다.");
            }
            if (shortArrayCount > 0)
            {
                Debug.LogWarning($"PlayerThrowPreviewSystem >> 미리보기 점 배열이 flightTicks + 1보다 짧아 플레이어 {shortArrayCount}명의 미리보기를 숨겼다.");
            }
        }

        // 던지는 위치에서 1틱씩 포물선을 따라가며 점을 남기고, 환경 레이어에 닿으면 거기서 멈춘다. 점 배열이 모자라면 숨기고 false를 돌려준다.
        private static bool Preview(ref PlayerThrowPreviewComponent preview, in WeaponComponent weapon, Vector3 position, Quaternion rotation, int throwCharge)
        {
            ref readonly ProjectileComponent projectile = ref weapon.projectile;
            if (preview.points == null || preview.points.Length < projectile.flightTicks + 1)
            {
                return false;
            }

            Vector3 launchPosition = position + rotation * weapon.muzzleOffset;
            Vector3 targetPosition = PlayerWeaponUtility.GetThrowTargetPosition(in weapon.throwing, position, rotation, throwCharge);

            preview.points[0] = launchPosition;
            int pointCount = 1;
            Vector3 currentPosition = launchPosition;
            for (int elapsedTicks = 1; elapsedTicks <= projectile.flightTicks; elapsedTicks++)
            {
                float progress = ThrowableUtility.GetArcProgress(elapsedTicks, projectile.flightTicks);
                Vector3 nextPosition = ThrowableUtility.GetArcPosition(launchPosition, targetPosition, projectile.arcHeight, progress);
                bool isBlocked = ThrowableUtility.TrySweep(in projectile, currentPosition, nextPosition, out currentPosition);

                preview.points[pointCount] = currentPosition;
                pointCount++;
                if (isBlocked)
                {
                    break;
                }
            }

            preview.isVisible = true;
            preview.pointCount = pointCount;
            preview.impactPosition = currentPosition;
            preview.explosionRadius = projectile.explosionRadius;
            return true;
        }
    }
}
