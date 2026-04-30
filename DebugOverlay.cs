using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using MelonLoader;

namespace Mikmod
{
    internal static class DebugOverlay
    {
        private static bool _visible;

        private static float _fps;
        private static float _timer;
        private static int _frames;

        private static GUIStyle _textStyle;
        private static GUIStyle _shadowStyle;
        private static Texture2D _bgTex;

        private const float LineHeight = 18f;
        private const float Width = 380f;

        private static int _cachedObjectCount;
        private static int _cachedCanvasCount;

        private static float _cacheTimer = CacheInterval;
        private const float CacheInterval = 2f;

        // ── Called from Mikmod.OnUpdate() ─────────────────────────────
        public static void OnUpdate()
        {
            HandleToggle();
            UpdateFPS();
            UpdateCache();
        }

        // ── Called from Mikmod.OnGUI() ────────────────────────────────
        public static void OnGUI()
        {
            if (!_visible) return;

            EnsureStyles();
            Draw();
        }

        private static void HandleToggle()
        {
            if (Input.GetKeyDown(KeyCode.F3))
                _visible = !_visible;
        }

        private static void UpdateCache()
        {
            _cacheTimer += Time.unscaledDeltaTime;

            if (_cacheTimer < CacheInterval)
                return;

            _cacheTimer = 0f;

            _cachedObjectCount = Object.FindObjectsOfType<GameObject>().Length;
            _cachedCanvasCount = Object.FindObjectsOfType<Canvas>().Length;
        }

        private static void UpdateFPS()
        {
            _frames++;
            _timer += Time.unscaledDeltaTime;

            if (_timer >= 0.5f)
            {
                _fps = _frames / _timer;
                _frames = 0;
                _timer = 0f;
            }
        }

        private static void EnsureStyles()
        {
            if (_textStyle != null) return;

            _textStyle = new GUIStyle(GUI.skin.label);
            _textStyle.fontSize = 12;
            _textStyle.normal.textColor = Color.white;

            _shadowStyle = new GUIStyle(_textStyle);
            _shadowStyle.normal.textColor = new Color(0, 0, 0, 0.9f);

            _bgTex = MakeTex(new Color(0, 0, 0, 0.45f));
        }

        private static void Draw()
        {
            var left = BuildLeft();
            var right = BuildRight();

            float marginX = 3f;

            float y = 0;

            // Left column
            foreach (var line in left)
            {
                DrawLine(line, marginX, y);
                y += LineHeight;
            }

            // Right column
            y = 0;
            float rightX = Screen.width - Width - marginX;

            foreach (var line in right)
            {
                DrawLine(line, rightX, y);
                y += LineHeight;
            }
        }

        private static void DrawLine(string text, float x, float y)
        {
            Rect bg = new Rect(x - 2, y, Width, LineHeight);
            GUI.DrawTexture(bg, _bgTex);

            GUI.Label(new Rect(x + 1, y + 1, Width, LineHeight), text, _shadowStyle);
            GUI.Label(new Rect(x, y, Width, LineHeight), text, _textStyle);
        }

        private static List<string> BuildLeft()
        {
            var list = new List<string>();

            var scene = SceneManager.GetActiveScene();

            list.Add($"Scene: {scene.name}");
            list.Add($"Path: {scene.path}");
            list.Add($"Objects: {_cachedObjectCount}");
            list.Add($"Canvases: {_cachedCanvasCount}");

            var cam = Camera.main;
            if (cam != null)
            {
                var p = cam.transform.position;
                var r = cam.transform.eulerAngles;

                list.Add($"Cam Pos: {p.x:F2}, {p.y:F2}, {p.z:F2}");
                list.Add($"Cam Rot: {r.x:F2}, {r.y:F2}, {r.z:F2}");
            }
            else
            {
                list.Add("Camera: null");
            }

            list.Add($"FPS: {_fps:F1}");
            list.Add($"Time: {Time.time:F2}");
            list.Add($"Delta: {Time.deltaTime:F4}");

            list.Add($"Res: {Screen.width}x{Screen.height}");

            int q = QualitySettings.GetQualityLevel();
            list.Add($"Quality: {q} ({QualitySettings.names[q]})");

            list.Add($"Version: {Application.version}");

            return list;
        }

        private static List<string> BuildRight()
        {
            var list = new List<string>();

            list.Add($"MelonLoader: {MelonLoader.Properties.BuildInfo.Version}");

            list.Add("Mods:");
            foreach (var mod in MelonMod.RegisteredMelons)
            {
                list.Add($"- {mod.Info.Name} v{mod.Info.Version}");
            }

            list.Add($"Device: {SystemInfo.deviceName}");
            list.Add($"GPU: {SystemInfo.graphicsDeviceName}");
            list.Add($"RAM: {SystemInfo.systemMemorySize} MB");
            list.Add($"CPU: {SystemInfo.processorType}");

            return list;
        }

        private static Texture2D MakeTex(Color col)
        {
            var t = new Texture2D(1, 1);
            t.SetPixel(0, 0, col);
            t.Apply();
            return t;
        }
    }
}