using System.Collections.Generic;
using TopView.Records;
using UnityEngine;

namespace TopView.Presentation.Projectile.ViewSystems
{
    // 명중·폭발 기록마다 피격 이펙트를 풀에서 꺼내 재생하고, 끝난 이펙트를 풀에 돌려놓는다.
    public static class HitEffectViewSystem
    {
        /// <summary>
        /// 끝난 피격 이펙트를 풀에 돌려놓은 뒤 이번 프레임의 기록마다 이펙트를 재생한다.
        /// </summary>
        /// <param name="hitRecords">이번 프레임의 명중·폭발 기록</param>
        /// <param name="hitEffectPrefabs">피격 이펙트 표. 번호별 ParticleSystem 프리팹</param>
        /// <param name="context">피격 이펙트 실행 상태</param>
        public static void Present(List<ProjectileHitRecord> hitRecords, GameObject[] hitEffectPrefabs, HitEffectViewContext context)
        {
            ReturnFinishedHitEffects(context);

            for (int recordIndex = 0; recordIndex < hitRecords.Count; recordIndex++)
            {
                ProjectileHitRecord record = hitRecords[recordIndex];
                if (record.hitEffectIndex >= 0)
                {
                    PlayHitEffect(context, record.hitEffectIndex, record.position, hitEffectPrefabs);
                }
            }
        }

        private static void ReturnFinishedHitEffects(HitEffectViewContext context)
        {
            for (int playingIndex = context.playingEffects.Count - 1; playingIndex >= 0; playingIndex--)
            {
                ParticleSystem effect = context.playingEffects[playingIndex];
                if (effect.IsAlive(true))
                {
                    continue;
                }

                effect.gameObject.SetActive(false);
                context.pools[context.playingEffectIndices[playingIndex]].Push(effect);
                context.playingEffects.RemoveAt(playingIndex);
                context.playingEffectIndices.RemoveAt(playingIndex);
            }
        }

        // 풀에서 꺼낸 이펙트는 스스로 지워지지 않도록 Stop Action을 끈 뒤 재생한다.
        private static void PlayHitEffect(HitEffectViewContext context, int hitEffectIndex, Vector3 position, GameObject[] hitEffectPrefabs)
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
            effect.gameObject.SetActive(true);
            effect.Play(true);
            context.playingEffects.Add(effect);
            context.playingEffectIndices.Add(hitEffectIndex);
        }
    }
}
