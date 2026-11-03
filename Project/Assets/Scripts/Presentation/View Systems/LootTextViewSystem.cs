using System.Collections.Generic;
using TopView.Components;
using TopView.Presentation.ViewContexts;
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
        /// <param name="rule">월드 글자의 표시 값</param>
        /// <param name="pendingTexts">월드 글자 대기 목록</param>
        public static void Present(List<LootPickedUpRecord> pickedUpRecords, in WorldTextRuleComponent rule, List<WorldTextEntry> pendingTexts)
        {
            for (int recordIndex = 0; recordIndex < pickedUpRecords.Count; recordIndex++)
            {
                LootPickedUpRecord record = pickedUpRecords[recordIndex];
                string format;
                Color color;
                switch (record.kind)
                {
                    case LootKind.Gold:
                        format = rule.goldFormat;
                        color = rule.goldColor;
                        break;
                    case LootKind.Health:
                        format = rule.healthFormat;
                        color = rule.healthColor;
                        break;
                    case LootKind.Ammo:
                        format = rule.ammoFormat;
                        color = rule.ammoColor;
                        break;
                    default:
                        Debug.LogError($"LootTextViewSystem >> 글자가 정해지지 않은 전리품 종류 {record.kind}라 글자를 띄우지 않는다.");
                        continue;
                }

                pendingTexts.Add(new WorldTextEntry
                {
                    position = record.position + Vector3.up * rule.lootHeightOffset,
                    text = string.Format(format, record.amount),
                    color = color
                });
            }
        }
    }
}
