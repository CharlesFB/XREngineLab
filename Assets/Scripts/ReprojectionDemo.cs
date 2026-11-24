using UnityEngine;

public class ReprojectionDemo : MonoBehaviour
{
    public float lag = 0.35f;
    public float shift = 0.18f;
    public float speed = 0.9f;
    public bool warp = true;

    Camera content;
    Material mat;
    RenderTexture rt;

    void Awake()
    {
        content = DemoRig.Camera("Lagged Camera", new Vector3(0f, 1.7f, -3.2f));
        content.enabled = false;
        content.fieldOfView = 70f;

        Camera view = DemoRig.Camera("View", Vector3.zero);
        view.cullingMask = 0;
        view.gameObject.tag = "MainCamera";
        view.gameObject.AddComponent<AudioListener>();
        view.gameObject.AddComponent<ReprojectionPass>().host = this;

        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
        Destroy(floor.GetComponent<Collider>());
        floor.transform.position = new Vector3(0f, 0f, 3f);
        floor.transform.localScale = new Vector3(2.4f, 1f, 2.4f);
        MeshRenderer floorRenderer = floor.GetComponent<MeshRenderer>();
        floorRenderer.sharedMaterial = DemoRig.Make("GridFloor");

        DemoRig.Cube(new Vector3(-0.45f, 0.7f, -0.6f), new Vector3(0.1f, 1.4f, 0.1f), DemoRig.Coral);
        DemoRig.Cube(new Vector3(0.7f, 1f, 6.5f), new Vector3(0.16f, 2f, 0.16f), DemoRig.Teal);
        mat = DemoRig.Make("Reprojection");
    }

    void OnDestroy()
    {
        if (rt != null)
        {
            rt.Release();
            Destroy(rt);
        }
        DemoRig.Cleanup();
    }

    public void RenderLagged()
    {
        EnsureTarget();
        float yaw = Mathf.Sin((Time.time - lag) * speed) * 28f;
        content.transform.rotation = Quaternion.Euler(16f, yaw, 0f);
        content.Render();
        if (mat == null)
            return;
        mat.SetFloat("_Lag", lag);
        mat.SetFloat("_Shift", shift);
        mat.SetFloat("_Warp", warp ? 1f : 0f);
    }

    public void Blit(RenderTexture dst)
    {
        if (mat == null || rt == null)
        {
            Graphics.Blit(rt, dst);
            return;
        }
        Graphics.Blit(rt, dst, mat);
    }

    void EnsureTarget()
    {
        int w = Mathf.Max(16, Screen.width);
        int h = Mathf.Max(16, Screen.height);
        if (rt != null && rt.width == w && rt.height == h)
            return;
        if (rt != null)
        {
            rt.Release();
            Destroy(rt);
        }
        rt = new RenderTexture(w, h, 24) { name = "Lagged" };
        content.targetTexture = rt;
    }

    void OnGUI()
    {
        DemoGui.Begin("晚一帧再扭曲", "左半屏是延迟后的朝向。右半屏仍用这张旧图，只按延迟去改采样，画面下方挪得更多。这是时间扭曲的草图：不重画场景。近处柱子和远处柱子对不齐，是因为没有按深度分开移。");
        lag = DemoGui.Slider("延迟（秒）", lag, 0f, 0.8f);
        warp = DemoGui.Toggle("右半屏扭曲", warp);
        shift = DemoGui.Slider("扭曲幅度", shift, 0f, 0.45f);
        speed = DemoGui.Slider("摆头速度", speed, 0.2f, 2f);
        DemoGui.End();
    }
}

public class ReprojectionPass : MonoBehaviour
{
    public ReprojectionDemo host;

    void LateUpdate()
    {
        host.RenderLagged();
    }

    void OnRenderImage(RenderTexture src, RenderTexture dst)
    {
        host.Blit(dst);
    }
}
