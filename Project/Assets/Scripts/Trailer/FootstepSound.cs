using UnityEngine;

namespace TopView.Trailer
{
    // 트레일러 촬영 전용. 달리기 클립의 애니메이션 이벤트 Footstep을 받아 발자국 소리를 낸다.
    // 게임 로직(World)이 꺼진 Trailer 씬에서만 쓰므로, View는 메서드를 두지 않는다는 규칙과 이벤트 금지 규칙의 예외로 둔다.
    // Animator와 같은 오브젝트에 붙여야 애니메이션 이벤트가 이 메서드를 찾는다.
    public class FootstepSound : MonoBehaviour
    {
        // 발자국 소리를 재생할 AudioSource. Play On Awake와 Loop는 끈다
        [SerializeField] private AudioSource _source;
        // 발을 디딜 때마다 이 중 하나를 무작위로 고른다. 직전에 낸 소리는 연달아 고르지 않는다
        [SerializeField] private AudioClip[] _clips;

        private int _lastClipIndex = -1;

        /// <summary>
        /// 애니메이션 이벤트가 부른다. 발자국 소리 하나를 무작위로 골라 재생한다.
        /// </summary>
        public void Footstep()
        {
            if (_source == null)
            {
                Debug.LogWarning($"FootstepSound >> {name}에 AudioSource가 연결되지 않아 발자국 소리를 내지 않았다.", this);
                return;
            }

            if (_clips == null || _clips.Length == 0)
            {
                Debug.LogWarning($"FootstepSound >> {name}에 발자국 소리가 없어 재생하지 않았다.", this);
                return;
            }

            int clipIndex = Random.Range(0, _clips.Length);
            if (_clips.Length > 1 && clipIndex == _lastClipIndex)
            {
                // 직전 소리를 피해 다음 칸으로 넘긴다
                clipIndex = (clipIndex + 1) % _clips.Length;
            }

            AudioClip clip = _clips[clipIndex];
            if (clip == null)
            {
                Debug.LogWarning($"FootstepSound >> {name}의 발자국 소리 {clipIndex}번 칸이 비어 있어 재생하지 않았다.", this);
                return;
            }

            _lastClipIndex = clipIndex;
            _source.PlayOneShot(clip);
        }
    }
}
