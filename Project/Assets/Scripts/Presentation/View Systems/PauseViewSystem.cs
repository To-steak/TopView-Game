using System.Collections.Generic;
using DG.Tweening;
using TopView.Presentation.ViewContexts;
using TopView.Presentation.Views;
using UnityEngine;

namespace TopView.Presentation.ViewSystems
{
    // 판이 일시정지되면 Unity가 스스로 돌리는 연출(소리, DOTween, 파티클, Animator)을 멈추고, 풀리면 이어서 움직이게 한다.
    // 일시정지는 판 전체에 걸리므로, 소리와 DOTween은 씬의 모든 소리와 트윈에 걸린다.
    public static class PauseViewSystem
    {
        /// <summary>
        /// 일시정지 상태가 바뀐 프레임에만 연출을 멈추거나 다시 움직인다. 다른 ViewSystem보다 먼저 부른다.
        /// </summary>
        /// <param name="isPaused">판이 일시정지 중인지</param>
        /// <param name="playerView">플레이어 View. 아직 만들지 않았으면 null</param>
        /// <param name="enemyViews">적 저장소 칸별 View. 빈 칸은 null</param>
        /// <param name="playingHitEffects">재생 중인 피격 이펙트</param>
        /// <param name="context">일시정지 연출 실행 상태</param>
        public static void Present(bool isPaused, PlayerView playerView, EnemyView[] enemyViews, List<ParticleSystem> playingHitEffects, PauseViewContext context)
        {
            if (isPaused == context.isApplied)
            {
                return;
            }

            AudioListener.pause = isPaused;
            if (isPaused)
            {
                DOTween.PauseAll();
            }
            else
            {
                DOTween.PlayAll();
            }

            float animatorSpeed = isPaused ? 0f : 1f;
            if (playerView != null)
            {
                playerView.animator.speed = animatorSpeed;
            }
            for (int index = 0; index < enemyViews.Length; index++)
            {
                if (enemyViews[index] != null)
                {
                    enemyViews[index].animator.speed = animatorSpeed;
                }
            }

            for (int effectIndex = 0; effectIndex < playingHitEffects.Count; effectIndex++)
            {
                ParticleSystem effect = playingHitEffects[effectIndex];
                if (isPaused)
                {
                    effect.Pause(true);
                }
                // 멈추지 않은 이펙트에 Play를 부르면 처음부터 다시 재생되므로, 멈춰 둔 이펙트만 이어서 재생한다
                else if (effect.isPaused)
                {
                    effect.Play(true);
                }
            }

            context.isApplied = isPaused;
        }

        /// <summary>
        /// 걸어 둔 일시정지를 푼다. 소리와 DOTween은 씬이 바뀌어도 남으므로, 판을 끝내고 씬을 떠나기 전에 부른다.
        /// </summary>
        /// <param name="context">일시정지 연출 실행 상태</param>
        public static void Release(PauseViewContext context)
        {
            if (!context.isApplied)
            {
                return;
            }

            AudioListener.pause = false;
            DOTween.PlayAll();
            context.isApplied = false;
        }
    }
}
