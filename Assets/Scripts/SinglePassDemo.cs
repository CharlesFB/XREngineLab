using UnityEngine;
using UnityEngine.Rendering;

public class SinglePassDemo : MonoBehaviour
{
    public bool singlePass = true;
    public float ipd = 0.064f;
    public int count = 12;

    Transform head;
    Camera left;
    Camera right;
    Camera single;
    GameObject[] cubes;
    Matrix4x4[] matrices;
    Mesh mesh;
    Material instanced;
    CommandBuffer buffer;

    void Awake()
    {
        head = new GameObject("Head").transform;
        head.position = new Vector3(0f, 1.2f, -4.2f);
        head.gameObject.AddComponent<AudioListener>();

        left = Eye("Left Eye", new Rect(0f, 0f, 0.5f, 1f));
        right = Eye("Right Eye", new Rect(0.5f, 0f, 0.5f, 1f));
        single = Eye("Single Camera", new Rect(0f, 0f, 1f, 1f));
        single.gameObject.tag = "MainCamera";
        buffer = new CommandBuffer { name = "BothEyes" };
        single.AddCommandBuffer(CameraEvent.AfterForwardOpaque, buffer);

        mesh = DemoRig.CubeMesh();
        instanced = DemoRig.Make("StereoInstanced");
        if (instanced != null)
            instanced.enableInstancing = true;

        cubes = new GameObject[16];
        matrices = new Matrix4x4[32];
        for (int i = 0; i < cubes.Length; i++)
        {
            int col = i % 4;
            int row = i / 4;
            Vector3 pos = new Vector3((col - 1.5f) * 0.85f, 0.45f + (row % 2) * 0.7f, row * 0.95f);
            cubes[i] = DemoRig.Cube(pos, Vector3.one * 0.42f, DemoRig.Hue(i));
        }
    }

    Camera Eye(string name, Rect rect)
    {
        Camera cam = DemoRig.Camera(name, Vector3.zero);
        cam.transform.SetParent(head, false);
        cam.rect = rect;
        return cam;
    }

    void Update()
    {
        left.transform.localPosition = new Vector3(-ipd * 0.5f, 0f, 0f);
        right.transform.localPosition = new Vector3(ipd * 0.5f, 0f, 0f);
        single.transform.localPosition = Vector3.zero;
        float halfAspect = (Screen.width * 0.5f) / Mathf.Max(1f, Screen.height);
        single.aspect = halfAspect;

        left.enabled = !singlePass;
        right.enabled = !singlePass;
        single.enabled = singlePass;
        for (int i = 0; i < cubes.Length; i++)
            cubes[i].SetActive(!singlePass && i < count);

        buffer.Clear();
        if (!singlePass || instanced == null || mesh == null)
            return;
        instanced.SetFloat("_Ipd", ipd);
        int n = Mathf.Clamp(count, 1, cubes.Length);
        for (int i = 0; i < n; i++)
        {
            Matrix4x4 trs = Matrix4x4.TRS(cubes[i].transform.position, cubes[i].transform.rotation, cubes[i].transform.localScale);
            matrices[i * 2] = trs;
            matrices[i * 2 + 1] = trs;
        }
        buffer.DrawMeshInstanced(mesh, 0, instanced, 0, matrices, n * 2, null);
    }

    void OnDestroy()
    {
        DemoRig.Cleanup();
    }

    void OnGUI()
    {
        int submits = singlePass ? 1 : count * 2;
        DemoGui.Begin("单 Pass 模拟", "多 Pass：两只眼睛各画一遍。单 Pass：一次 DrawMeshInstanced，实例号区分左右眼，再把裁剪坐标收进半屏。真机是 Single Pass Instanced 或 Multiview，自定义 HLSL 要写 UNITY_VERTEX_OUTPUT_STEREO。省的是提交次数，片元几乎仍是两眼。");
        singlePass = DemoGui.Toggle(singlePass ? "当前：单 Pass" : "当前：多 Pass", singlePass);
        ipd = DemoGui.Slider("瞳距（米）", ipd, 0f, 0.09f);
        count = DemoGui.IntSlider("物体数量", count, 4, 16);
        GUILayout.Label("物体 " + count + "，提交 " + submits + " 次");
        DemoGui.End();
    }
}
