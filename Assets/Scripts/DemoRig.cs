using System.Collections.Generic;
using UnityEngine;

public static class DemoRig
{
    public static readonly Color Background = new Color(0.078f, 0.071f, 0.055f, 1f);
    public static readonly Color Coral = new Color(0.91f, 0.46f, 0.32f, 1f);
    public static readonly Color Teal = new Color(0.36f, 0.68f, 0.58f, 1f);
    public static readonly Color Sand = new Color(0.95f, 0.78f, 0.40f, 1f);
    public static readonly Color Paper = new Color(0.93f, 0.90f, 0.82f, 1f);

    static readonly List<Object> owned = new List<Object>();

    public static Shader Load(string name)
    {
        Shader shader = Resources.Load<Shader>("Shaders/" + name);
        if (shader == null)
            shader = Shader.Find("XRLab/" + name);
        return shader;
    }

    public static Material Unlit(Color color)
    {
        Shader shader = Load("UnlitColor");
        Material mat = shader != null ? new Material(shader) : new Material(Shader.Find("Standard"));
        mat.color = color;
        mat.hideFlags = HideFlags.HideAndDontSave;
        owned.Add(mat);
        return mat;
    }

    public static Material Make(string shaderName)
    {
        Shader shader = Load(shaderName);
        if (shader == null)
            return null;
        Material mat = new Material(shader) { hideFlags = HideFlags.HideAndDontSave };
        owned.Add(mat);
        return mat;
    }

    public static Camera Camera(string name, Vector3 position, float fov = 60f)
    {
        var go = new GameObject(name);
        Camera cam = go.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Background;
        cam.fieldOfView = fov;
        cam.nearClipPlane = 0.05f;
        cam.farClipPlane = 80f;
        cam.allowHDR = false;
        cam.transform.position = position;
        return cam;
    }

    public static GameObject Cube(Vector3 position, Vector3 scale, Color color)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Object.Destroy(go.GetComponent<Collider>());
        go.transform.position = position;
        go.transform.localScale = scale;
        go.GetComponent<MeshRenderer>().sharedMaterial = Unlit(color);
        return go;
    }

    public static Mesh CubeMesh()
    {
        GameObject temp = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Mesh mesh = temp.GetComponent<MeshFilter>().sharedMesh;
        Object.Destroy(temp);
        return mesh;
    }

    public static Color Hue(int index)
    {
        return Color.HSVToRGB(Mathf.Repeat(index * 0.17f, 1f), 0.55f, 0.9f);
    }

    public static void Cleanup()
    {
        for (int i = 0; i < owned.Count; i++)
        {
            if (owned[i] != null)
                Object.Destroy(owned[i]);
        }
        owned.Clear();
    }
}
