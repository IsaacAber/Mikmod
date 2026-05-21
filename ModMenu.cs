using MelonLoader;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Mikmod
{
    static internal class ModMenu
    {
        private static bool _visible = false;
        private static Rect _windowRect = new Rect(100, 100, 320, 340);

        private static Texture2D _windowBg;
        private static Texture2D _dragTex;

        private static GUIStyle _centeredTitle;

        private static Vector2 _scroll;



        // ── Called from Mikmod.OnUpdate() ─────────────────────────────
        public static void OnUpdate()
        {
            if (UnityEngine.Input.GetKeyDown(KeyCode.LeftShift))
                _visible = !_visible;
        }

        // ── Called from Mikmod.OnGUI() ────────────────────────────────
        public static void OnGUI()
        {
            if (!_visible) return;

            Event e = Event.current;

            if (e.type == EventType.KeyDown &&
                e.keyCode == KeyCode.LeftShift &&
                _windowRect.Contains(e.mousePosition))
            {
                _visible = false;
                return;
            }

            if (_windowBg == null)
                _windowBg = MakeTex(new Color(0.15f, 0.15f, 0.15f, 1f));

            if (_dragTex == null)
                _dragTex = MakeDotTex();

            if (_centeredTitle == null)
            {
                _centeredTitle = new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontStyle = FontStyle.Bold
                };
                _centeredTitle.normal.textColor = Color.white;
            }

            // Block input to game when interacting with menu
            if (_windowRect.Contains(e.mousePosition))
            {
                Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
                UnityEngine.Input.ResetInputAxes();
            }

            GUI.color = Color.white;
            GUI.backgroundColor = Color.white;

            GUI.skin.window.normal.background = _windowBg;
            GUI.skin.window.onNormal.background = _windowBg;

            _windowRect = GUI.Window(9991, _windowRect, DrawWindow, "");
        }

        // ── Main Window ───────────────────────────────────────────────
        private static void DrawWindow(int id)
        {
            DrawDragArea();

            GUILayout.Space(25); // below drag bar

            // ── Sticky Header ──
            var mod = MelonMod.RegisteredMelons
                .FirstOrDefault(m => m.Info.Name == "Mikmod");

            string version = mod?.Info.Version?.ToString() ?? "Unknown";

            var scene = SceneManager.GetActiveScene();

            GUILayout.BeginVertical("box");
            GUILayout.Label($"Version: {version}, Scene: {scene.name}");
            GUILayout.EndVertical();

            // ── Scrollable Content ──
            _scroll = GUILayout.BeginScrollView(_scroll, GUILayout.ExpandHeight(true));

            switch (scene.name)
            {
                case "g_spike":
                    GUILayout.Label("Spike Game Mod Options");
                    GUILayout.Space(5);
                    GUI.color = Color.yellow;
                    SpikesHack.invincibility_mode = GUILayout.Toggle(
                        SpikesHack.invincibility_mode,
                        "Invincibility (no damage from spikes)"
                    );
                    GUI.color = Color.green;
                    SpikesHack.freeze_player = GUILayout.Toggle(
                        SpikesHack.freeze_player,
                        "Freeze Player (disable movement)"
                    );
                    GUI.color = Color.yellow;
                    SpikesHack.spawn_infront_of_the_player = GUILayout.Toggle(
                        SpikesHack.spawn_infront_of_the_player,
                        "Spawn items in front of player (not random)"
                    );
                    GUI.color = Color.red;
                    SpikesHack.infinite_spawns = GUILayout.Toggle(
                        SpikesHack.infinite_spawns,
                        "Infinite item spawns (⚠ unstable / high risk)"
                    );
                    GUILayout.Space(10);
                    GUILayout.Label(
                        "Spawn Iterations: number of items per cycle\n" +
                        "(Only works when infinite spawns is enabled)"
                    );
                    GUILayout.BeginHorizontal();
                    SpikesHack.spawn_iterations = (int)GUILayout.HorizontalSlider(
                        SpikesHack.spawn_iterations, 1, 100
                    );
                    GUILayout.Label(SpikesHack.spawn_iterations.ToString(), GUILayout.Width(40));
                    GUILayout.EndHorizontal();

                    GUILayout.Space(10);
                    GUILayout.Label(
                        "Spawn Ratio: how many of each type spawn per iteration\n" +
                        "(Only works when infinite spawns is enabled)"
                    );

                    // Points
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Points:", GUILayout.Width(60));
                    SpikesHack.ratio_points = (int)GUILayout.HorizontalSlider(
                        SpikesHack.ratio_points, 0, 10
                    );
                    GUILayout.Label(SpikesHack.ratio_points.ToString(), GUILayout.Width(40));
                    GUILayout.EndHorizontal();

                    // Shield
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Shield:", GUILayout.Width(60));
                    SpikesHack.ratio_shield = (int)GUILayout.HorizontalSlider(
                        SpikesHack.ratio_shield, 0, 10
                    );
                    GUILayout.Label(SpikesHack.ratio_shield.ToString(), GUILayout.Width(40));
                    GUILayout.EndHorizontal();

                    // Life
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Life:", GUILayout.Width(60));
                    SpikesHack.ratio_life = (int)GUILayout.HorizontalSlider(
                        SpikesHack.ratio_life, 0, 10
                    );
                    GUILayout.Label(SpikesHack.ratio_life.ToString(), GUILayout.Width(40));
                    GUILayout.EndHorizontal();

                    // Laser
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Laser:", GUILayout.Width(60));
                    SpikesHack.ratio_laser = (int)GUILayout.HorizontalSlider(
                        SpikesHack.ratio_laser, 0, 10
                    );
                    GUILayout.Label(SpikesHack.ratio_laser.ToString(), GUILayout.Width(40));
                    GUILayout.EndHorizontal();
                    break;

                default:
                    GUI.color = Color.green;
                    GUILayout.Label($"No hacks found for this scene: {scene.name}");
                    GUILayout.Space(5);
                    GUI.color = Color.white;
                    GUIStyle style = new GUIStyle(GUI.skin.label);
                    style.fontSize = 24;
                    style.fontStyle = FontStyle.Bold;

                    GUILayout.Label("General room hacks:", style);
                    GUILayout.Label("Talk freely to every player in the room with the MikMod!");
                    GUILayout.BeginHorizontal();

                    GUILayout.Label("Message timeout (sec):", GUILayout.Width(160));

                    string timeoutInput = Settings.MessageLifetime.Value.ToString();

                    timeoutInput = GUILayout.TextField(timeoutInput, GUILayout.Width(80));

                    if (float.TryParse(timeoutInput, out float parsed) && parsed != Settings.MessageLifetime.Value)
                    {
                        Settings.MessageLifetime.Value = (int)parsed;
                        MelonPreferences.Save();
                    }

                    GUILayout.EndHorizontal();
                    FreeSpeech.inputMessage = GUILayout.TextArea(FreeSpeech.inputMessage, GUILayout.ExpandWidth(true));
                    if (GUILayout.Button("Send Message"))
                    {
                        FreeSpeech.TalkSomeShit(FreeSpeech.inputMessage);
                    }
                    GUILayout.Label("Toggle SFS logging to console:");
                    if (GUILayout.Button((SFSPatches.debugLevel == SFSPatches.DebugLevel.None) ? "Enable Logging" : "Disable Logging"))
                    {
                        SFSPatches.debugLevel = (SFSPatches.debugLevel == SFSPatches.DebugLevel.None) ? SFSPatches.DebugLevel.RawMessages : SFSPatches.DebugLevel.None;
                    }
                    GUI.color = new Color(133f / 255f, 52f / 255f, 243f / 255f);
                    GUILayout.Label("Want to add hacks for this scene? Contribute on GitHub!");
                    if (GUILayout.Button("GitHub Repo"))
                    {
                        Application.OpenURL("https://github.com/IsaacAber/Mikmod");
                    }
                    break;
            }

            GUILayout.EndScrollView();

            GUI.DragWindow(); // allow dragging anywhere not blocked
        }

        // ── Drag Bar ──────────────────────────────────────────────────
        private static void DrawDragArea()
        {
            Rect dragRect = new Rect(0, 0, _windowRect.width, 22);

            // Bar background
            GUI.DrawTexture(dragRect, MakeTex(new Color(0f, 0.125f, 0.59f, 1f)));

            // Dotted overlay
            GUI.DrawTextureWithTexCoords(
                dragRect,
                _dragTex,
                new Rect(0, 0, dragRect.width / 12f, dragRect.height / 12f)
            );

            // ── Title background ──
            string title = "Mikmod";

            Vector2 textSize = _centeredTitle.CalcSize(new GUIContent(title));

            Rect titleRect = new Rect(
                (_windowRect.width - textSize.x) / 2 - 6, // center + padding
                2,
                textSize.x + 12,
                18
            );

            GUI.DrawTexture(titleRect, MakeTex(new Color(0f, 0f, 0f, 0.8f))); // semi-dark bg

            // ── Title text ──
            GUI.Label(titleRect, title, _centeredTitle);

            // Drag area
            GUI.DragWindow(dragRect);
        }

        // ── Helpers ───────────────────────────────────────────────────
        private static Texture2D MakeTex(Color col)
        {
            var t = new Texture2D(1, 1);
            t.SetPixel(0, 0, col);
            t.Apply();
            return t;
        }

        private static Texture2D MakeDotTex()
        {
            int size = 6;
            var tex = new Texture2D(size, size);

            Color bg = new Color(0.15f, 0.15f, 0.15f, 1f);
            Color dot = new Color(0.8f, 0.8f, 0.8f, 1f);

            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                    tex.SetPixel(x, y, ((x + y) % 3 == 0) ? dot : bg);

            tex.wrapMode = TextureWrapMode.Repeat;
            tex.filterMode = FilterMode.Point;
            tex.Apply();

            return tex;
        }
    }
}