using UnityEngine;

public class FoveatedDemo : MonoBehaviour
{
    public float radius = 0.28f;
    public float block = 16f;
    public bool followMouse = true;

    Material mat;

    void Awake()
    {
        Camera cam = DemoRig.Camera("Main Camera", new Vector3(0f, 1.6f, -1.4f));
        cam.gameObject.tag = "MainCamera";
        cam.gameObject.AddComponent<AudioListener>();
        cam.gameObject.AddComponent<FoveatedPass>().host = this;

        Color[] colors = { DemoRig.Coral, DemoRig.Sand, DemoRig.Teal, DemoRig.Paper };
        for (int y = 0; y < 6; y++)
        {
            for (int x = 0; x < 8; x++)
                DemoRig.Cube(new Vector3((x - 3.5f) * 0.62f, 0.15f + y * 0.62f, 2.6f), Vector3.one * 0.52f, colors[(x + y) % 4]);
        }
        mat = DemoRig.Make("Foveated");
    }

    void OnDestroy()
    {
        DemoRig.Cleanup();
    }

    public void Blit(RenderTexture src, RenderTexture dst)
    {
        if (mat == null)
        {
            Graphics.Blit(src, dst);
            return;
        }
        Vector2 gaze = followMouse
            ? new Vector2(Input.mousePosition.x / Mathf.Max(1f, Screen.width), Input.mousePosition.y / Mathf.Max(1f, Screen.height))
            : new Vector2(0.5f, 0.5f);
        mat.SetVector("_Gaze", gaze);
        mat.SetFloat("_Radius", radius);
        mat.SetFloat("_Block", block);
        Graphics.Blit(src, dst, mat);
    }

    void OnGUI()
    {
        DemoGui.Begin("注视点少算", "圆内是原分辨率，圆外合成大色块。正式引擎用可变速率着色或把四周画到更小的缓冲。视线偏了，细的那一块会对不齐眼睛正在看的地方。");
        followMouse = DemoGui.Toggle("注视点跟鼠标", followMouse);
        radius = DemoGui.Slider("圆半径", radius, 0.08f, 0.7f);
        block = DemoGui.Slider("外围块大小", block, 2f, 36f);
        DemoGui.End();
    }
}

public class FoveatedPass : MonoBehaviour
{
    public FoveatedDemo host;

    void OnRenderImage(RenderTexture src, RenderTexture dst)
    {
        host.Blit(src, dst);
    }
}
