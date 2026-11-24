using UnityEngine;
using UnityEngine.SceneManagement;

public static class DemoGui
{
    public static readonly string[] Scenes =
    {
        "01_Distortion",
        "02_Stereo",
        "03_Reprojection",
        "04_Foveated",
        "05_SinglePass",
        "06_FrameBudget",
    };

    public static readonly string[] Labels =
    {
        "1 畸变色差",
        "2 双目视差",
        "3 重投影",
        "4 注视点",
        "5 单 Pass",
        "6 帧预算",
    };

    static GUIStyle label;
    static GUIStyle note;
    static GUIStyle button;
    static float smoothDt = 0.016f;
    static bool fontReady;

    public static float FrameMs => smoothDt * 1000f;

    public static void Begin(string title, string body)
    {
        EnsureStyles();
        smoothDt = Mathf.Lerp(smoothDt, Time.unscaledDeltaTime, 0.08f);
        GUILayout.BeginArea(new Rect(10, 10, 286, Screen.height - 20), GUI.skin.box);
        GUILayout.Label(title, label);
        GUILayout.Label(body, note);
        GUILayout.Space(6);
    }

    public static void End()
    {
        GUILayout.Space(8);
        GUILayout.Label("帧间隔  " + FrameMs.ToString("0.0") + " ms", label);
        GUILayout.Space(4);
        string current = SceneManager.GetActiveScene().name;
        for (int i = 0; i < Scenes.Length; i++)
        {
            string caption = Scenes[i] == current ? "当前  " + Labels[i] : Labels[i];
            if (GUILayout.Button(caption, button) && Scenes[i] != current)
                SceneManager.LoadScene(Scenes[i]);
        }
        GUILayout.EndArea();
    }

    public static float Slider(string caption, float value, float min, float max)
    {
        GUILayout.Label(caption + "  " + value.ToString("0.00"), label);
        return GUILayout.HorizontalSlider(value, min, max);
    }

    public static int IntSlider(string caption, int value, int min, int max)
    {
        GUILayout.Label(caption + "  " + value, label);
        return Mathf.RoundToInt(GUILayout.HorizontalSlider(value, min, max));
    }

    public static bool Toggle(string caption, bool value)
    {
        return GUILayout.Toggle(value, caption, label);
    }

    static void EnsureStyles()
    {
        if (label != null)
            return;
        Font font = null;
        if (!fontReady)
        {
            fontReady = true;
            font = Font.CreateDynamicFontFromOSFont(
                new[]
                {
                    "Microsoft YaHei",
                    "PingFang SC",
                    "Noto Sans CJK SC",
                    "Noto Sans SC",
                    "Source Han Sans SC",
                    "WenQuanYi Micro Hei",
                    "SimHei",
                },
                15);
        }
        label = new GUIStyle(GUI.skin.label) { fontSize = 14, wordWrap = false };
        note = new GUIStyle(GUI.skin.label) { fontSize = 13, wordWrap = true };
        button = new GUIStyle(GUI.skin.button) { fontSize = 13 };
        if (font != null)
        {
            label.font = font;
            note.font = font;
            button.font = font;
        }
    }
}
