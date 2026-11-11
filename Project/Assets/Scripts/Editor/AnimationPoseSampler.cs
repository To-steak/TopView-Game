using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace TopView.Editor
{
    // 미리보기 씬에 만든 프리팹을 클립의 한 시점 자세로 멈춰 놓고, 그 자세에서 한 지점의 위치를 루트 기준으로 재는 에디터 도구.
    // 위치를 굽는 도구(MuzzleOffsetBaker, EnemySpawnOffsetBaker)가 함께 쓴다. 프리팹을 준비하는 일은 각 도구가 한다.
    public static class AnimationPoseSampler
    {
        private const string GRAPH_NAME = "Animation Pose Sample";

        /// <summary>
        /// animator를 클립의 seconds 시점 자세로 멈춘다. 루트 모션은 끈다. 자세는 돌려준 그래프를 없앨 때까지 유지되므로, 부르는 쪽이 다 쓴 뒤 Destroy한다.
        /// </summary>
        /// <param name="animator">클립을 재생할 Animator</param>
        /// <param name="clip">샘플링할 클립</param>
        /// <param name="seconds">샘플링 시점(초)</param>
        /// <returns>샘플링에 쓴 그래프</returns>
        public static PlayableGraph SamplePose(Animator animator, AnimationClip clip, float seconds)
        {
            animator.applyRootMotion = false;
            PlayableGraph graph = PlayableGraph.Create(GRAPH_NAME);
            graph.SetTimeUpdateMode(DirectorUpdateMode.Manual);
            AnimationPlayableOutput output = AnimationPlayableOutput.Create(graph, GRAPH_NAME, animator);
            AnimationClipPlayable playable = AnimationClipPlayable.Create(graph, clip);
            output.SetSourcePlayable(playable);
            playable.SetTime(seconds);
            graph.Evaluate();
            return graph;
        }

        /// <summary>
        /// 클립을 sampleTicks 시점으로 샘플링한 자세에서 target의 위치를 root의 회전 기준 로컬 위치로 잰다. 클립이 없으면 지금 자세 그대로 잰다.
        /// 루트의 크기는 반영하지 않는다. 게임에서 rotation * offset으로 되돌리기 때문이다.
        /// </summary>
        /// <param name="root">기준이 되는 루트</param>
        /// <param name="animator">클립을 재생할 Animator. 클립이 없으면 null이어도 된다</param>
        /// <param name="clip">샘플링할 클립. 없으면 null</param>
        /// <param name="sampleTicks">샘플링 시점(틱)</param>
        /// <param name="target">위치를 잴 지점</param>
        /// <param name="logOwner">경고 앞에 붙일 도구 이름</param>
        /// <param name="prefabName">경고에 남길 프리팹 이름</param>
        /// <param name="offset">잰 로컬 위치</param>
        /// <returns>잴 수 있었으면 true. 클립이 있는데 Animator가 없으면 경고를 남기고 false</returns>
        public static bool TryMeasureLocalPosition(Transform root, Animator animator, AnimationClip clip, int sampleTicks, Transform target, string logOwner, string prefabName, out Vector3 offset)
        {
            offset = Vector3.zero;
            PlayableGraph graph = default;
            try
            {
                if (clip != null)
                {
                    if (animator == null)
                    {
                        Debug.LogWarning($"{logOwner} >> {prefabName}에 Animator가 없어 클립을 샘플링할 수 없어 위치를 굽지 않았다.");
                        return false;
                    }

                    float time = sampleTicks * Time.fixedDeltaTime;
                    if (time > clip.length)
                    {
                        Debug.LogWarning($"{logOwner} >> 샘플링 시점({sampleTicks}틱)이 {clip.name}의 길이({clip.length}초)보다 길어 클립 끝에서 샘플링했다.");
                        time = clip.length;
                    }

                    graph = SamplePose(animator, clip, time);
                }

                offset = Quaternion.Inverse(root.rotation) * (target.position - root.position);
                return true;
            }
            finally
            {
                if (graph.IsValid())
                {
                    graph.Destroy();
                }
            }
        }
    }
}
