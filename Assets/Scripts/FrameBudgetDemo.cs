using UnityEngine;

public class FrameBudgetDemo : MonoBehaviour
{
    public float scale = 1f;
    public bool msaa = false;
    public bool shadows = true;
    public bool extraLights = false;
    public int count = 24;
    public int steps = 4;

    Camera content;
    Light sun;
    Light[] points;
    GameObject[] cubes;
    RenderTexture rt;
    Material present;
    Material lit;
    int rtW = -1;
    int rtH = -1;
    int rtMsaa = -1;

    void OnEnable()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = -1;
        QualitySettings.pixelLightCount = 6;
        QualitySettings.shadowDistance = 30f;
    }

    void OnDisable()
    {
        QualitySettings.vSyncCount = 1;
    }

    void Awake()
    {
        var sunGo = new GameObject("Sun");
        sunGo.transform.rotation = Quaternion.Euler(48f, -28f, 0f);
        sun = sunGo.AddComponent<Light>();
        sun.type = LightType.Directional;
        sun.intensity = 1.05f;
        sun.shadows = LightShadows.Soft;

        points = new Light[4];
        Vector3[] at =
        {
            new Vector3(-2f, 2.2f, 1f),
            new Vector3(2f, 2.2f, 1f),
            new Vector3(-2f, 2.2f, 5f),
            new Vector3(2f, 2.2f, 5f),
        };
        for (int i = 0; i < points.Length; i++)
        {
            var go = new GameObject("Point " + i);
            go.transform.position = at[i];
            Light light = go.AddComponent<Light>();
            light.type = LightType.Point;
            light.range = 7f;
            light.intensity = 1.3f;
            light.color = Color.Lerp(DemoRig.Sand, Color.white, 0.4f);
            light.shadows = LightShadows.None;
            points[i] = light;
        }

        Shader standard = Shader.Find("Standard");
        if (standard == null)
            standard = DemoRig.Load("UnlitColor");
        lit = new Material(standard);
        lit.color = new Color(0.72f, 0.66f, 0.55f);
        lit.SetFloat("_Glossiness", 0.08f);
        lit.SetFloat("_Metallic", 0f);
        lit.hideFlags = HideFlags.HideAndDontSave;

        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
        Destroy(floor.GetComponent<Collider>());
        floor.transform.position = new Vector3(0f, 0f, 3f);
        floor.transform.localScale = new Vector3(1.6f, 1f, 1.6f);
        floor.GetComponent<MeshRenderer>().sharedMaterial = lit;

        cubes = new GameObject[64];
        for (int i = 0; i < cubes.Length; i++)
        {
            int col = i % 8;
            int row = i / 8;
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Destroy(cube.GetComponent<Collider>());
            cube.transform.position = new Vector3((col - 3.5f) * 1.15f, 0.45f, row * 1.15f);
            cube.transform.localScale = Vector3.one * 0.7f;
            MeshRenderer renderer = cube.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = lit;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            var block = new MaterialPropertyBlock();
            block.SetColor("_Color", DemoRig.Hue(i));
            renderer.SetPropertyBlock(block);
            cubes[i] = cube;
        }

        content = DemoRig.Camera("Scene Camera", new Vector3(0f, 3.2f, -6.5f), 55f);
        content.transform.rotation = Quaternion.Euler(18f, 0f, 0f);
        content.depth = -2;

        Camera view = DemoRig.Camera("Present", Vector3.zero);
        view.cullingMask = 0;
        view.depth = 0;
        view.gameObject.tag = "MainCamera";
        view.gameObject.AddComponent<AudioListener>();
        view.gameObject.AddComponent<BudgetPass>().host = this;
        present = DemoRig.Make("Present");
    }

    void Update()
    {
        sun.shadows = shadows ? LightShadows.Soft : LightShadows.None;
        for (int i = 0; i < points.Length; i++)
            points[i].enabled = extraLights;
        for (int i = 0; i < cubes.Length; i++)
        {
            bool on = i < count;
            cubes[i].SetActive(on);
            if (on)
                cubes[i].transform.Rotate(0f, 18f * Time.deltaTime, 0f, Space.World);
        }
        EnsureTarget();
    }

    void EnsureTarget()
    {
        int samples = msaa ? 4 : 1;
        int w = Mathf.Max(32, Mathf.RoundToInt(Screen.width * scale));
        int h = Mathf.Max(32, Mathf.RoundToInt(Screen.height * scale));
        if (rt != null && rtW == w && rtH == h && rtMsaa == samples)
            return;
        if (rt != null)
        {
            rt.Release();
            Destroy(rt);
        }
        var desc = new RenderTextureDescriptor(w, h, RenderTextureFormat.ARGB32, 24)
        {
            msaaSamples = samples,
        };
        rt = new RenderTexture(desc) { name = "Budget" };
        content.targetTexture = rt;
        rtW = w;
        rtH = h;
        rtMsaa = samples;
    }

    public void Blit(RenderTexture dst)
    {
        if (present == null || rt == null)
        {
            Graphics.Blit(rt, dst);
            return;
        }
        present.SetFloat("_Steps", steps);
        Graphics.Blit(rt, dst, present);
    }

    void OnDestroy()
    {
        if (rt != null)
        {
            rt.Release();
            Destroy(rt);
        }
        if (lit != null)
            Destroy(lit);
        DemoRig.Cleanup();
    }

    void OnGUI()
    {
        DemoGui.Begin("帧预算", "72fps 整帧约 13.8ms，合成器还要留几毫秒。先看分辨率和 MSAA，再关阴影和多余的灯。全屏循环用来把片元算力拉高，方便看出毫秒变化。已关掉垂直同步。");
        scale = DemoGui.Slider("分辨率", scale, 0.35f, 1f);
        msaa = DemoGui.Toggle("MSAA 4x", msaa);
        shadows = DemoGui.Toggle("实时阴影", shadows);
        extraLights = DemoGui.Toggle("四盏点光源", extraLights);
        count = DemoGui.IntSlider("方块数量", count, 8, 64);
        steps = DemoGui.IntSlider("全屏循环", steps, 1, 64);
        string budget = DemoGui.FrameMs > 13.8f ? "已经超过 13.8ms" : "还在 13.8ms 以内";
        GUILayout.Label(budget + "   缓冲 " + rtW + "×" + rtH);
        DemoGui.End();
    }
}

public class BudgetPass : MonoBehaviour
{
    public FrameBudgetDemo host;

    void OnRenderImage(RenderTexture src, RenderTexture dst)
    {
        host.Blit(dst);
    }
}
