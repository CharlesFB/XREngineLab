using UnityEngine;

public class StereoDemo : MonoBehaviour
{
    public float ipd = 0.064f;

    Camera left;
    Camera right;

    void Awake()
    {
        var head = new GameObject("Head");
        head.transform.position = new Vector3(0f, 1.1f, -0.4f);
        head.AddComponent<AudioListener>();

        left = Eye(head.transform, "Left Eye", new Rect(0f, 0f, 0.5f, 1f));
        right = Eye(head.transform, "Right Eye", new Rect(0.5f, 0f, 0.5f, 1f));
        left.gameObject.tag = "MainCamera";

        DemoRig.Cube(new Vector3(0f, 1.1f, 1.3f), Vector3.one * 0.42f, DemoRig.Coral);
        DemoRig.Cube(new Vector3(0.15f, 1.15f, 6.5f), Vector3.one * 1.15f, DemoRig.Teal);
        DemoRig.Cube(new Vector3(-1.1f, 0.7f, 3.2f), new Vector3(0.35f, 1.2f, 0.35f), DemoRig.Sand);
        DemoRig.Cube(new Vector3(1.2f, 0.55f, 3.4f), new Vector3(0.5f, 0.9f, 0.5f), DemoRig.Paper);
    }

    Camera Eye(Transform parent, string name, Rect rect)
    {
        Camera cam = DemoRig.Camera(name, Vector3.zero);
        cam.transform.SetParent(parent, false);
        cam.rect = rect;
        return cam;
    }

    void Update()
    {
        left.transform.localPosition = new Vector3(-ipd * 0.5f, 0f, 0f);
        right.transform.localPosition = new Vector3(ipd * 0.5f, 0f, 0f);
    }

    void OnDestroy()
    {
        DemoRig.Cleanup();
    }

    void OnGUI()
    {
        DemoGui.Begin("双目视差", "左右各一只相机，只在 x 上差一段瞳距。近处的珊瑚色块错位大，远处的青色块几乎重合。瞳距拉到 0 时两眼变成同一张图。");
        ipd = DemoGui.Slider("瞳距（米）", ipd, 0f, 0.09f);
        DemoGui.End();
    }
}
