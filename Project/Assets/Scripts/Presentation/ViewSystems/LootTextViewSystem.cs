using System.Collections.Generic;
using TopView.Components;
using TopView.Presentation.ViewContexts;
using TopView.Presentation.Views;
using TopView.Records;
using UnityEngine;

namespace TopView.Presentation.ViewSystems
{
    // 전리품 획득 기록마다 획득 글자를 월드 글자 대기 목록에 넣는다.
    public static class LootTextViewSystem
    {
        /// <summary>
        /// 이번 프레임의 기록마다 종류별 색과 형식으로 획득 글자를 대기 목록에 넣는다. WorldTextViewSystem.Present 전에 부른다.
        /// </summary>
        /// <param name="pickedUpRecords">이번 프레임에 전리품을 획득한 기록</param>
        /// <param name="textViewPrefab">표시 값을 읽을 월드 글자 프리팹. 없으면 글자를 넣지 않는다</param>
        /// <param name="pendingTexts">월드 글자 대기 목록</param>
        public static void Present(List<LootPickedUpRecord> pickedUpRecords, WorldTextView textViewPrefab, List<WorldTextEntry> pendingTexts)
        {
            // 프리팹이 없으면 판을 준비할 때 로그를 남겼으므로 여기서는 건너뛴다
            if (textViewPrefab == null)
            {
                return;
            }

            for (int recordIndex = 0; recordIndex < pickedUpRecords.Count; recordIndex++)
            {
                LootPickedUpRecord record = pickedUpRecords[recordIndex];
                string format;
                Color color;
                switch (record.kind)
                {
                    case LootKind.Gold:
                        format = textViewPrefab.goldFormat;
                        color = textViewPrefab.goldColor;
                        break;
                    case LootKind.Health:
                        format = textViewPrefab.healthFormat;
                        color = textViewPrefab.healthColor;
                        break;
                    case LootKind.Ammo:
                        format = textViewPrefab.ammoFormat;
                        color = textViewPrefab.ammoColor;
                        break;
                    default:
                        Debug.LogError($"LootTextViewSystem >> 글자가 정해지지 않은 전리품 종류 {record.kind}라 글자를 띄우지 않는다.");
                        continue;
                }

                pendingTexts.Add(new WorldTextEntry
                {
                    position = record.position + Vector3.up * textViewPrefab.lootHeightOffset,
                    text = string.Format(format, record.amount),
                    color = color
                });
            }
        }
    }
}
