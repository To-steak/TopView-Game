using System.Collections.Generic;
using TopView.Presentation.Views;
using TopView.Records;
using UnityEngine;

namespace TopView.Presentation.ViewSystems
{
    // 전리품 획득 기록마다 그 종류 프리팹의 획득 소리를 플레이어의 효과음 AudioSource로 재생한다.
    public static class LootSoundViewSystem
    {
        /// <summary>
        /// 이번 프레임의 기록마다 획득 소리를 겹쳐 재생한다. 프리팹이나 소리가 없는 종류는 소리를 내지 않는다.
        /// </summary>
        /// <param name="pickedUpRecords">이번 프레임에 전리품을 획득한 기록</param>
        /// <param name="lootPrefabs">전리품 종류 번호별 LootView 프리팹. 없는 종류는 null</param>
        /// <param name="soundEffectSource">플레이어의 효과음 AudioSource. 플레이어 View가 없거나 연결되지 않았으면 null</param>
        public static void Present(List<LootPickedUpRecord> pickedUpRecords, GameObject[] lootPrefabs, AudioSource soundEffectSource)
        {
            if (pickedUpRecords.Count == 0)
            {
                return;
            }

            // 연결이 빠진 경우는 플레이어 View를 만들 때 이미 알렸다
            if (soundEffectSource == null)
            {
                return;
            }

            for (int recordIndex = 0; recordIndex < pickedUpRecords.Count; recordIndex++)
            {
                int kind = (int)pickedUpRecords[recordIndex].kind;
                if (kind < 0 || kind >= lootPrefabs.Length)
                {
                    Debug.LogError($"LootSoundViewSystem >> 전리품 종류 번호({kind})가 프리팹 표 크기({lootPrefabs.Length}) 밖이라 획득 소리를 내지 않는다.");
                    continue;
                }

                // 프리팹이 없는 종류는 판을 준비할 때 로그를 남겼다
                GameObject prefab = lootPrefabs[kind];
                if (prefab == null)
                {
                    continue;
                }

                // 소리가 없는 종류도 판을 준비할 때 로그를 남겼다
                AudioClip clip = prefab.GetComponent<LootView>().pickupClip;
                if (clip != null)
                {
                    soundEffectSource.PlayOneShot(clip);
                }
            }
        }
    }
}
