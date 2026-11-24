using UnityEngine;

public class DistortionDemo : MonoBehaviour
{
    public float k1 = 0.25f;
    public float chroma = 0.035f;
    public bool enabledEffect = true;

    Material mat;

    void Awake()
    {
        Camera cam = DemoRig.Camera("Main Camera", new Vector3(0f, 1.8f, -1.2f));
        cam.gameObject.tag = "MainCamera";
        cam.gameObject.AddComponent<AudioListener>();
        cam.gameObject.AddComponent<DistortionPass>().host = this;

        Color[] colors =
        {
            DemoRig.Coral, DemoRig.Sand, DemoRig.Teal, new Color(0.45f, 0.58f, 0.82f),
        };
        for (int y = 0; y < 5; y++)
        {
            for (int x = 0; x < 7; x++)
                DemoRig.Cube(new Vector3((x - 3) * 0.72f, 0.2f + y * 0.72f, 3f), Vector3.one * 0.6f, colors[(x + y) % 4]);
        }
        DemoRig.Cube(new Vector3(0f, 1.8f, 2.72f), new Vector3(5.2f, 0.04f, 0.04f), DemoRig.Paper);
        DemoRig.Cube(new Vector3(0f, 1.8f, 2.68f), new Vector3(0.04f, 3.3f, 0.04f), DemoRig.Paper);
        mat = DemoRig.Make("Distortion");
    }

    void OnDestroy()
    {
        DemoRig.Cleanup();
    }

    void OnGUI()
    {
        DemoGui.Begin("畸变和色差", "直网格先画完，再按半径把采样拉开。k1 越大越弯，色差让红蓝边缘分开。左边面板保持笔直，它画在这张图之后。");
        enabledEffect = DemoGui.Toggle("开启校正采样", enabledEffect);
        k1 = DemoGui.Slider("k1", k1, 0f, 0.55f);
        chroma = DemoGui.Slider("色差", chroma, 0f, 0.08f);
        if (mat == null)
            GUILayout.Label("没有载入 Distortion 着色器");
        DemoGui.End();
    }

    public void Blit(RenderTexture src, RenderTexture dst)
    {
        if (!enabledEffect || mat == null)
        {
            Graphics.Blit(src, dst);
            return;
        }
        mat.SetFloat("_K1", k1);
        mat.SetFloat("_Chroma", chroma);
        Graphics.Blit(src, dst, mat);
    }
}

public class DistortionPass : MonoBehaviour
{
    public DistortionDemo host;

    void OnRenderImage(RenderTexture src, RenderTexture dst)
    {
        host.Blit(src, dst);
    }
}
