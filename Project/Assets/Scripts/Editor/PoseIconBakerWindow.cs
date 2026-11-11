using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;

namespace TopView.Editor
{
    // 캐릭터 프리팹을 클립의 한 시점 자세로 멈춰 작은 해상도로 찍어, 픽셀 아트처럼 보이는 아이콘 이미지 파일로 굽는 편집 창.
    // 자세는 AnimationPoseSampler가 잡고, 촬영은 PreviewRenderUtility의 미리보기 씬에서 한다. 게임 코드와는 상관없다.
    public class PoseIconBakerWindow : EditorWindow
    {
        private GameObject _prefab;
        private AnimationClip _clip;
        // 샘플링 시점(클립 길이의 %)
        private float _samplePercent = 50f;
        // 카메라가 캐릭터를 바라보는 수평 각도(도). 90이면 캐릭터의 오른쪽 옆모습이다
        private float _cameraYawDegrees = 90f;
        // 카메라가 위에서 내려다보는 각도(도)
        private float _cameraPitchDegrees = 10f;
        // 캐릭터 둘레에 남길 여백 비율
        private float _padding = 0.1f;
        private int _imageSize = 24;
        // 주 조명 세기
        private float _lightIntensity = 1.5f;
        // 그림자 쪽까지 고르게 밝히는 환경광 색
        private Color _ambientColor = new Color(0.6f, 0.6f, 0.6f, 1f);
        private Texture2D _previewTexture;

        private const string WINDOW_TITLE = "Pose Icon Baker";
        private const float MAX_PERCENT = 100f;
        private const float PREVIEW_SCALE = 8f;
        private const float CAMERA_DISTANCE_MARGIN = 2f;
        private const float MAX_LIGHT_INTENSITY = 4f;
        private const float LIGHT_PITCH_DEGREES = 40f;
        private const float LIGHT_YAW_OFFSET_DEGREES = -30f;
        // 배경을 이 색으로 칠한 뒤 같은 색 픽셀을 투명하게 만든다. 렌더 파이프라인이 알파를 지키지 않아도 배경이 빠진다
        private static readonly Color KEY_COLOR = new Color(1f, 0f, 1f, 1f);
        private const float KEY_TOLERANCE = 0.02f;

        /// <summary>
        /// 편집 창을 연다.
        /// </summary>
        [MenuItem("Tools/Pose Icon Baker")]
        public static void ShowWindow()
        {
            GetWindow<PoseIconBakerWindow>(WINDOW_TITLE);
        }

        private void OnDisable()
        {
            DestroyPreviewTexture();
        }

        private void OnGUI()
        {
            _prefab = (GameObject)EditorGUILayout.ObjectField("Prefab", _prefab, typeof(GameObject), false);
            _clip = (AnimationClip)EditorGUILayout.ObjectField("Clip", _clip, typeof(AnimationClip), false);
            _samplePercent = EditorGUILayout.Slider("Sample %", _samplePercent, 0f, MAX_PERCENT);
            _cameraYawDegrees = EditorGUILayout.Slider("Camera Yaw", _cameraYawDegrees, -180f, 180f);
            _cameraPitchDegrees = EditorGUILayout.Slider("Camera Pitch", _cameraPitchDegrees, -89f, 89f);
            _padding = EditorGUILayout.Slider("Padding", _padding, 0f, 1f);
            _imageSize = Mathf.Max(1, EditorGUILayout.IntField("Image Size", _imageSize));
            _lightIntensity = EditorGUILayout.Slider("Light Intensity", _lightIntensity, 0f, MAX_LIGHT_INTENSITY);
            _ambientColor = EditorGUILayout.ColorField("Ambient Color", _ambientColor);

            EditorGUILayout.Space();
            using (new EditorGUI.DisabledScope(_prefab == null || _clip == null))
            {
                if (GUILayout.Button("Preview"))
                {
                    Preview();
                }
                if (GUILayout.Button("Bake Image"))
                {
                    Bake();
                }
            }

            if (_previewTexture != null)
            {
                EditorGUILayout.Space();
                float size = _imageSize * PREVIEW_SCALE;
                Rect rect = GUILayoutUtility.GetRect(size, size, GUILayout.ExpandWidth(false));
                EditorGUI.DrawTextureTransparent(rect, _previewTexture, ScaleMode.ScaleToFit);
            }
        }

        private void Preview()
        {
            Texture2D texture = Render();
            if (texture == null)
            {
                return;
            }

            DestroyPreviewTexture();
            _previewTexture = texture;
        }

        // 고른 경로에 이미지 파일을 쓰고, 픽셀이 뭉개지지 않도록 Sprite·Point 필터·무압축으로 가져온다
        private void Bake()
        {
            string path = EditorUtility.SaveFilePanelInProject(WINDOW_TITLE, $"{_clip.name} Icon", "png", "아이콘 이미지를 저장할 위치를 고른다.");
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            Texture2D texture = Render();
            if (texture == null)
            {
                return;
            }

            File.WriteAllBytes(path, texture.EncodeToPNG());
            DestroyPreviewTexture();
            _previewTexture = texture;

            AssetDatabase.ImportAsset(path);
            TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.SaveAndReimport();
            Debug.Log($"{nameof(PoseIconBakerWindow)} >> {path}에 아이콘을 구웠다.");
        }

        // 미리보기 씬에 프리팹을 만들어 자세를 잡고 찍는다. Animator가 없거나 그릴 Renderer가 없으면 경고하고 null을 돌려준다.
        // PreviewRenderUtility는 BeginStaticPreview와 EndStaticPreview 사이에서만 그려야 한다. 그 밖에서 Render를 부르면
        // 미리보기 씬의 조명·환경 설정(RenderSettings)이 준비되지 않아 에디터가 강제 종료된다
        private Texture2D Render()
        {
            PreviewRenderUtility preview = new PreviewRenderUtility();
            PlayableGraph graph = default;
            bool hasBegun = false;
            try
            {
                GameObject instance = preview.InstantiatePrefabInScene(_prefab);
                Animator animator = instance.GetComponentInChildren<Animator>();
                if (animator == null)
                {
                    Debug.LogWarning($"{nameof(PoseIconBakerWindow)} >> {_prefab.name}에 Animator가 없어 아이콘을 굽지 않았다.");
                    return null;
                }

                graph = AnimationPoseSampler.SamplePose(animator, _clip, _clip.length * _samplePercent / MAX_PERCENT);
                if (!TryGetBounds(instance, out Bounds bounds))
                {
                    Debug.LogWarning($"{nameof(PoseIconBakerWindow)} >> {_prefab.name}에 보이는 Renderer가 없어 아이콘을 굽지 않았다.");
                    return null;
                }

                // 화면 배율만큼 크게 잡히므로 나눠서 넘겨 결과가 _imageSize 픽셀이 되게 한다
                float pointSize = _imageSize / EditorGUIUtility.pixelsPerPoint;
                preview.BeginStaticPreview(new Rect(0f, 0f, pointSize, pointSize));
                hasBegun = true;

                Camera camera = preview.camera;
                Quaternion cameraRotation = Quaternion.Euler(_cameraPitchDegrees, _cameraYawDegrees + 180f, 0f);
                float distance = bounds.extents.magnitude + CAMERA_DISTANCE_MARGIN;
                camera.transform.SetPositionAndRotation(bounds.center - cameraRotation * Vector3.forward * distance, cameraRotation);
                camera.orthographic = true;
                camera.orthographicSize = GetOrthographicSize(camera.transform, bounds) * (1f + _padding);
                camera.nearClipPlane = 0.01f;
                camera.farClipPlane = distance * 2f;
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = KEY_COLOR;
                camera.allowMSAA = false;

                preview.lights[0].intensity = _lightIntensity;
                preview.ambientColor = _ambientColor;
                preview.lights[0].transform.rotation = Quaternion.Euler(LIGHT_PITCH_DEGREES, _cameraYawDegrees + LIGHT_YAW_OFFSET_DEGREES, 0f);

                // 작게 찍고 점 필터로 두어 픽셀 아트처럼 보이게 한다
                preview.Render(true, false);
                Texture2D rendered = preview.EndStaticPreview();
                hasBegun = false;
                return KeyOutBackground(rendered);
            }
            finally
            {
                if (hasBegun)
                {
                    DestroyImmediate(preview.EndStaticPreview());
                }
                if (graph.IsValid())
                {
                    graph.Destroy();
                }
                preview.Cleanup();
            }
        }

        private static bool TryGetBounds(GameObject instance, out Bounds bounds)
        {
            bounds = default;
            bool hasBounds = false;
            Renderer[] renderers = instance.GetComponentsInChildren<Renderer>();
            for (int rendererIndex = 0; rendererIndex < renderers.Length; rendererIndex++)
            {
                Renderer renderer = renderers[rendererIndex];
                if (!renderer.enabled)
                {
                    continue;
                }

                if (hasBounds)
                {
                    bounds.Encapsulate(renderer.bounds);
                }
                else
                {
                    bounds = renderer.bounds;
                    hasBounds = true;
                }
            }

            return hasBounds;
        }

        // 경계 상자의 여덟 꼭짓점을 카메라 기준으로 옮겨, 정사각형 화면에 모두 들어오는 직교 크기(화면 높이의 절반)를 구한다
        private static float GetOrthographicSize(Transform cameraTransform, Bounds bounds)
        {
            float halfSize = 0f;
            for (int cornerIndex = 0; cornerIndex < 8; cornerIndex++)
            {
                Vector3 corner = bounds.center + Vector3.Scale(bounds.extents, new Vector3(
                    (cornerIndex & 1) == 0 ? -1f : 1f,
                    (cornerIndex & 2) == 0 ? -1f : 1f,
                    (cornerIndex & 4) == 0 ? -1f : 1f));
                Vector3 local = cameraTransform.InverseTransformPoint(corner);
                halfSize = Mathf.Max(halfSize, Mathf.Abs(local.x), Mathf.Abs(local.y));
            }

            return halfSize;
        }

        // 찍은 이미지를 알파가 있는 이미지로 옮기고 배경색 픽셀을 투명하게 바꾼다. 찍은 이미지는 없앤다
        private static Texture2D KeyOutBackground(Texture2D rendered)
        {
            Texture2D texture = new Texture2D(rendered.width, rendered.height, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point
            };
            Color[] pixels = rendered.GetPixels();
            DestroyImmediate(rendered);
            for (int pixelIndex = 0; pixelIndex < pixels.Length; pixelIndex++)
            {
                Color pixel = pixels[pixelIndex];
                if (Mathf.Abs(pixel.r - KEY_COLOR.r) < KEY_TOLERANCE && Mathf.Abs(pixel.g - KEY_COLOR.g) < KEY_TOLERANCE && Mathf.Abs(pixel.b - KEY_COLOR.b) < KEY_TOLERANCE)
                {
                    pixels[pixelIndex] = Color.clear;
                }
                else
                {
                    pixel.a = 1f;
                    pixels[pixelIndex] = pixel;
                }
            }
            texture.SetPixels(pixels);
            texture.Apply();
            return texture;
        }

        private void DestroyPreviewTexture()
        {
            if (_previewTexture != null)
            {
                DestroyImmediate(_previewTexture);
                _previewTexture = null;
            }
        }
    }
}
