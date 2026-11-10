using System.Collections.Generic;
using TopView.Presentation.ViewContexts;
using TopView.Records;
using UnityEngine;

namespace TopView.Presentation.ViewSystems
{
    // 명중·폭발 기록과 근접 공격 기록마다 피격 이펙트(와 이펙트에 붙은 소리)를 풀에서 꺼내 재생하고, 끝난 이펙트를 풀에 돌려놓는다.
    public static class HitEffectViewSystem
    {
        /// <summary>
        /// 끝난 피격 이펙트를 풀에 돌려놓은 뒤 이번 프레임의 기록마다 이펙트를 재생한다.
        /// </summary>
        /// <param name="hitRecords">이번 프레임의 명중·폭발 기록</param>
        /// <param name="attackRecords">이번 프레임에 플레이어가 공격을 내보낸 기록. 임팩트 이펙트 번호가 있는 기록(근접)만 재생한다</param>
        /// <param name="hitEffectPrefabs">피격 이펙트 표. 번호별 ParticleSystem 프리팹</param>
        /// <param name="isPaused">판이 일시정지 중이면 true. 재생 중인 이펙트를 멈춘다</param>
        /// <param name="context">피격 이펙트 실행 상태</param>
        public static void Present(List<ProjectileHitRecord> hitRecords, List<PlayerAttackCommittedRecord> attackRecords, GameObject[] hitEffectPrefabs, bool isPaused, HitEffectViewContext context)
        {
            ReturnFinishedHitEffects(context);
            ApplyPause(context, isPaused);

            for (int recordIndex = 0; recordIndex < hitRecords.Count; recordIndex++)
            {
                ProjectileHitRecord record = hitRecords[recordIndex];
                if (record.hitEffectIndex >= 0)
                {
                    PlayHitEffect(context, record.hitEffectIndex, record.position, 1f, hitEffectPrefabs);
                }
            }

            for (int recordIndex = 0; recordIndex < attackRecords.Count; recordIndex++)
            {
                PlayerAttackCommittedRecord record = attackRecords[recordIndex];
                if (record.impactEffectIndex >= 0)
                {
                    PlayHitEffect(context, record.impactEffectIndex, record.impactPosition, record.impactScale, hitEffectPrefabs);
                }
            }
        }

        private static void ReturnFinishedHitEffects(HitEffectViewContext context)
        {
            for (int playingIndex = context.playingEffects.Count - 1; playingIndex >= 0; playingIndex--)
            {
                ParticleSystem effect = context.playingEffects[playingIndex];
                // 소리가 파티클보다 길면 끝까지 들리도록 소리도 끝난 뒤에 돌려놓는다
                if (effect.IsAlive(true) || (effect.TryGetComponent(out AudioSource sound) && sound.isPlaying))
                {
                    continue;
                }

                effect.gameObject.SetActive(false);
                context.pools[context.playingEffectIndices[playingIndex]].Push(effect);
                context.playingEffects.RemoveAt(playingIndex);
                context.playingEffectIndices.RemoveAt(playingIndex);
            }
        }

        private static void ApplyPause(HitEffectViewContext context, bool isPaused)
        {
            for (int playingIndex = 0; playingIndex < context.playingEffects.Count; playingIndex++)
            {
                ParticleSystem effect = context.playingEffects[playingIndex];
                if (isPaused && !effect.isPaused)
                {
                    effect.Pause(true);
                }
                // 멈추지 않은 이펙트에 Play를 부르면 처음부터 다시 재생되므로, 멈춰 둔 이펙트만 이어서 재생한다
                else if (!isPaused && effect.isPaused)
                {
                    effect.Play(true);
                }
            }
        }

        // 풀에서 꺼낸 이펙트는 스스로 지워지지 않도록 Stop Action을 끈 뒤 재생한다. 이펙트에 AudioSource가 있으면 그 소리도 함께 재생한다.
        // 크기는 루트 Transform의 배율로 정한다. 파티클이 배율을 따르려면 프리팹의 모든 Particle System에서 Scaling Mode가 Hierarchy여야 한다
        private static void PlayHitEffect(HitEffectViewContext context, int hitEffectIndex, Vector3 position, float scale, GameObject[] hitEffectPrefabs)
        {
            Stack<ParticleSystem> pool = context.pools[hitEffectIndex];
            ParticleSystem effect;
            if (pool.Count > 0)
            {
                effect = pool.Pop();
            }
            else
            {
                effect = Object.Instantiate(hitEffectPrefabs[hitEffectIndex]).GetComponent<ParticleSystem>();
                ParticleSystem.MainModule main = effect.main;
                main.stopAction = ParticleSystemStopAction.None;
            }

            effect.transform.SetPositionAndRotation(position, Quaternion.identity);
            // 풀의 이펙트를 다시 쓰므로 배율도 매번 정한다
            effect.transform.localScale = Vector3.one * scale;
            effect.gameObject.SetActive(true);
            effect.Play(true);
            if (effect.TryGetComponent(out AudioSource sound))
            {
                sound.Play();
            }
            context.playingEffects.Add(effect);
            context.playingEffectIndices.Add(hitEffectIndex);
        }
    }
}
