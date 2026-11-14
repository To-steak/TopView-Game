using System.Collections.Generic;
using TopView.Components;
using TopView.Presentation.ViewContexts;
using UnityEngine;

namespace TopView.Presentation.ViewSystems
{
    // UI 버튼 소리를 낸다. Presenter가 이번 프레임에 넣은 소리 요청마다 그 종류의 소리 하나를 무작위로 골라 재생한다.
    public static class UserInterfaceSoundViewSystem
    {
        /// <summary>
        /// 소리 요청마다 소리를 재생한다. 요청 목록은 비우지 않으므로 부른 쪽이 비운다.
        /// </summary>
        /// <param name="requests">이번 프레임의 UI 소리 요청 목록</param>
        /// <param name="context">UI 소리 실행 상태</param>
        public static void Present(List<UserInterfaceSoundKind> requests, UserInterfaceSoundViewContext context)
        {
            for (int requestIndex = 0; requestIndex < requests.Count; requestIndex++)
            {
                UserInterfaceSoundKind kind = requests[requestIndex];
                AudioClip[] clips;
                switch (kind)
                {
                    case UserInterfaceSoundKind.Click:
                        clips = context.clickClips;
                        break;
                    case UserInterfaceSoundKind.Close:
                        clips = context.closeClips;
                        break;
                    default:
                        Debug.LogWarning($"UserInterfaceSoundViewSystem >> 처리하지 않는 UI 소리 종류 {kind}를 버렸다.");
                        continue;
                }

                if (clips == null || clips.Length == 0)
                {
                    Debug.LogWarning($"UserInterfaceSoundViewSystem >> User Interface Sound Definition에 {kind} 소리가 없어 재생하지 않았다.");
                    continue;
                }

                // View 연출이라 게임 결과와 상관없어 UnityEngine.Random을 쓴다
                AudioClip clip = clips[Random.Range(0, clips.Length)];
                if (clip == null)
                {
                    Debug.LogWarning($"UserInterfaceSoundViewSystem >> User Interface Sound Definition의 {kind} 소리 칸이 비어 있어 재생하지 않았다.");
                    continue;
                }

                context.source.PlayOneShot(clip, context.volume);
            }
        }
    }
}
