using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TopView.Editor
{
    // 애니메이션 클립 길이를 Fixed Timestep 기준 틱으로 바꿔 ~Ticks 칸을 채우는 인스펙터 도구.
    // 클립과 백분율은 에셋에 저장하지 않고, 에디터가 켜져 있는 동안만 칸마다 기억한다.
    public static class AnimationTickConverter
    {
        private static readonly Dictionary<string, AnimationClip> CLIPS = new Dictionary<string, AnimationClip>();
        private static readonly Dictionary<string, float> START_PERCENTS = new Dictionary<string, float>();
        private static readonly Dictionary<string, float> END_PERCENTS = new Dictionary<string, float>();

        // 길이 틱 칸 옆에 붙는 클립 칸의 너비 비율
        private const float CLIP_FIELD_WIDTH_RATIO = 0.4f;
        private const float FIELD_GAP = 2f;
        private const float MAX_PERCENT = 100f;
        private const int RANGE_LINE_COUNT = 3;
        private const string CLIP_LABEL = "Clip";
        private const string START_PERCENT_LABEL = "Start %";
        private const string END_PERCENT_LABEL = "End %";
        private const string APPLY_LABEL = "Convert From Clip";

        /// <summary>
        /// 인스펙터 한 줄의 높이(줄 간격 포함).
        /// </summary>
        public static float LineHeight => EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

        /// <summary>
        /// 구간 변환 칸(클립, 백분율, 변환 버튼)의 높이.
        /// </summary>
        public static float RangeHeight => LineHeight * RANGE_LINE_COUNT;

        /// <summary>
        /// 초를 Fixed Timestep 기준 틱으로 바꾼다. 가장 가까운 정수로 반올림한다.
        /// </summary>
        /// <param name="seconds">시간(초)</param>
        /// <returns>시간(틱)</returns>
        public static int ToTicks(float seconds)
        {
            return Mathf.RoundToInt(seconds / Time.fixedDeltaTime);
        }

        /// <summary>
        /// 틱 칸과 그 옆의 클립 칸을 한 줄에 그린다. 클립을 넣으면 클립 길이 전체를 틱으로 바꿔 칸을 채운다.
        /// </summary>
        /// <param name="rect">그릴 영역(한 줄)</param>
        /// <param name="ticks">채울 ~Ticks 칸</param>
        public static void DrawLengthTicks(Rect rect, SerializedProperty ticks)
        {
            string key = GetKey(ticks);
            float clipWidth = rect.width * CLIP_FIELD_WIDTH_RATIO;
            Rect ticksRect = new Rect(rect.x, rect.y, rect.width - clipWidth - FIELD_GAP, rect.height);
            Rect clipRect = new Rect(ticksRect.xMax + FIELD_GAP, rect.y, clipWidth, rect.height);

            EditorGUI.PropertyField(ticksRect, ticks);

            CLIPS.TryGetValue(key, out AnimationClip clip);
            int indentLevel = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            EditorGUI.BeginChangeCheck();
            clip = (AnimationClip)EditorGUI.ObjectField(clipRect, clip, typeof(AnimationClip), false);
            if (EditorGUI.EndChangeCheck())
            {
                CLIPS[key] = clip;
                if (clip != null)
                {
                    ticks.intValue = ToTicks(clip.length);
                }
            }
            EditorGUI.indentLevel = indentLevel;
        }

        /// <summary>
        /// 클립과 시작·끝 백분율(클립 미리보기의 진행률)을 받아, 변환 버튼을 누르면 시작·끝·상태 길이 틱을 채운다.
        /// </summary>
        /// <param name="rect">그릴 영역(RangeHeight 높이)</param>
        /// <param name="startTicks">시작 백분율로 채울 칸</param>
        /// <param name="endTicks">끝 백분율로 채울 칸. 없으면 null이고 끝 백분율 칸도 그리지 않는다</param>
        /// <param name="durationTicks">클립 길이 전체로 채울 칸. 없으면 null</param>
        public static void DrawRange(Rect rect, SerializedProperty startTicks, SerializedProperty endTicks, SerializedProperty durationTicks)
        {
            string key = GetKey(startTicks);
            Rect line = new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight);

            CLIPS.TryGetValue(key, out AnimationClip clip);
            CLIPS[key] = (AnimationClip)EditorGUI.ObjectField(line, CLIP_LABEL, clip, typeof(AnimationClip), false);
            clip = CLIPS[key];

            line.y += LineHeight;
            START_PERCENTS.TryGetValue(key, out float startPercent);
            END_PERCENTS.TryGetValue(key, out float endPercent);
            if (endTicks == null)
            {
                startPercent = EditorGUI.Slider(line, START_PERCENT_LABEL, startPercent, 0f, MAX_PERCENT);
            }
            else
            {
                Rect startRect = new Rect(line.x, line.y, (line.width - FIELD_GAP) * 0.5f, line.height);
                Rect endRect = new Rect(startRect.xMax + FIELD_GAP, line.y, startRect.width, line.height);
                startPercent = Mathf.Clamp(EditorGUI.FloatField(startRect, START_PERCENT_LABEL, startPercent), 0f, MAX_PERCENT);
                endPercent = Mathf.Clamp(EditorGUI.FloatField(endRect, END_PERCENT_LABEL, endPercent), startPercent, MAX_PERCENT);
            }
            START_PERCENTS[key] = startPercent;
            END_PERCENTS[key] = endPercent;

            line.y += LineHeight;
            Rect buttonRect = EditorGUI.IndentedRect(line);
            using (new EditorGUI.DisabledScope(clip == null))
            {
                if (GUI.Button(buttonRect, APPLY_LABEL))
                {
                    startTicks.intValue = ToTicks(clip.length * startPercent / MAX_PERCENT);
                    if (endTicks != null)
                    {
                        endTicks.intValue = ToTicks(clip.length * endPercent / MAX_PERCENT);
                    }
                    if (durationTicks != null)
                    {
                        durationTicks.intValue = ToTicks(clip.length);
                    }
                }
            }
        }

        // 에셋과 칸 경로로 칸마다 다른 열쇠를 만든다. 같은 에셋의 배열 원소도 서로 구분된다.
        private static string GetKey(SerializedProperty property)
        {
            return $"{property.serializedObject.targetObject.GetInstanceID()}:{property.propertyPath}";
        }
    }
}
