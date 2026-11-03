using System.Collections.Generic;
using TopView.Components;
using TopView.Presentation.ViewContexts;
using TopView.Presentation.Views;
using TopView.Records;
using UnityEngine;

namespace TopView.Presentation.ViewSystems
{
    // 적 피해 기록마다 피해량 글자를 월드 글자 대기 목록에 넣는다.
    public static class DamageTextViewSystem
    {
        /// <summary>
        /// 이번 프레임의 기록마다 피해량 글자를 대기 목록에 넣는다. EnemyViewSystem.Present 다음, WorldTextViewSystem.Present 전에 부른다.
        /// </summary>
        /// <param name="damagedRecords">이번 프레임에 적이 피해를 받은 기록</param>
        /// <param name="enemyViews">적 저장소 칸별 View</param>
        /// <param name="enemyViewGenerations">적 저장소 칸별 View가 보여 주는 엔티티의 generation</param>
        /// <param name="rule">월드 글자의 표시 값</param>
        /// <param name="mainCamera">시작 위치를 어긋나게 할 기준 카메라. 없으면 null</param>
        /// <param name="pendingTexts">월드 글자 대기 목록</param>
        public static void Present(List<EnemyDamagedRecord> damagedRecords, EnemyView[] enemyViews, int[] enemyViewGenerations, in WorldTextRuleComponent rule, UnityEngine.Camera mainCamera, List<WorldTextEntry> pendingTexts)
        {
            for (int recordIndex = 0; recordIndex < damagedRecords.Count; recordIndex++)
            {
                EnemyDamagedRecord record = damagedRecords[recordIndex];
                pendingTexts.Add(new WorldTextEntry
                {
                    position = GetStartPosition(in record, enemyViews, enemyViewGenerations, rule.spreadRadius, mainCamera),
                    text = record.amount.ToString("F0"),
                    color = rule.damageColor
                });
            }
        }

        // 적 View가 있으면 그 적의 Damage Anchor에서, 없으면 기록의 위치에서 시작한다. 이 위치로 고정하므로 글자는 적이 사라져도 제자리에 남는다.
        private static Vector3 GetStartPosition(in EnemyDamagedRecord record, EnemyView[] enemyViews, int[] enemyViewGenerations, float spreadRadius, UnityEngine.Camera mainCamera)
        {
            EnemyView enemyView = enemyViews[record.enemy.index];
            bool hasAnchor = enemyView != null
                && enemyViewGenerations[record.enemy.index] == record.enemy.generation
                && enemyView.damageAnchor != null;
            Vector3 position = hasAnchor ? enemyView.damageAnchor.position : record.position;

            Quaternion spreadRotation = mainCamera != null ? mainCamera.transform.rotation : Quaternion.identity;
            Vector3 spread = Random.insideUnitCircle * spreadRadius;
            return position + spreadRotation * spread;
        }
    }
}
