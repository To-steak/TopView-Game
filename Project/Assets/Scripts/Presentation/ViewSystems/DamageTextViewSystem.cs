using System.Collections.Generic;
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
        /// <param name="enemyViews">엔티티 저장소 칸별 적 View</param>
        /// <param name="enemyViewGenerations">엔티티 저장소 칸별 적 View가 보여 주는 엔티티의 generation</param>
        /// <param name="textViewPrefab">표시 값을 읽을 월드 글자 프리팹. 없으면 글자를 넣지 않는다</param>
        /// <param name="mainCamera">시작 위치를 어긋나게 할 기준 카메라. 없으면 null</param>
        /// <param name="pendingTexts">월드 글자 대기 목록</param>
        public static void Present(List<EnemyDamagedRecord> damagedRecords, EnemyView[] enemyViews, int[] enemyViewGenerations, WorldTextView textViewPrefab, UnityEngine.Camera mainCamera, List<WorldTextEntry> pendingTexts)
        {
            // 프리팹이 없으면 판을 준비할 때 로그를 남겼으므로 여기서는 건너뛴다
            if (textViewPrefab == null)
            {
                return;
            }

            for (int recordIndex = 0; recordIndex < damagedRecords.Count; recordIndex++)
            {
                EnemyDamagedRecord record = damagedRecords[recordIndex];
                pendingTexts.Add(new WorldTextEntry
                {
                    position = GetStartPosition(in record, enemyViews, enemyViewGenerations, textViewPrefab.spreadRadius, mainCamera),
                    text = record.amount.ToString("F0"),
                    color = textViewPrefab.damageColor
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
