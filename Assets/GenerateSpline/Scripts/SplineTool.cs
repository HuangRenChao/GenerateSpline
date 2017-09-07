using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplineTool : MonoBehaviour {

    public float Smoothness = 1f;
    public int segment = 2;
    public float MeshWidth = 1f;
    public bool showGizmos = true;
    public float gizmoSize = 1f;
    public List<GameObject> points = new List<GameObject>();
    private List<Point> P = new List<Point>();
    private CubicSpline spline;
    private List<Vector3> verts = new List<Vector3>();
    private List<int> tris = new List<int>();
    private Vector2 riverOffset = Vector2.zero;
    private List<Vector3> np = new List<Vector3>();
    public List<Color> tempcolors = new List<Color>();
    private List<Color> colors = new List<Color>();
    private List<Vector3> norms = new List<Vector3>();
    private List<Vector2> uvs = new List<Vector2>();
    [HideInInspector]
    public Mesh mesh;
    [HideInInspector]
    public Material mat;
    private float c;
    private void Start()
    {
        this.spline = new CubicSpline(this.points, this.segment, this.Smoothness, true);
    }
    public virtual string getSceneName()
    {
        return "";
    }
    public void init()
    {
        string sceneName = this.getSceneName();
        string name = "Spline" + UnityEngine.Object.FindObjectsOfType(typeof(SplineTool)).Length.ToString() + sceneName;
        base.gameObject.name = name;
        base.gameObject.AddComponent<MeshRenderer>();
        base.gameObject.AddComponent<MeshFilter>();
        this.mesh = new Mesh();
        this.mat = new Material(Shader.Find("Diffuse"));
        this.mat.name = base.gameObject.name + "Mat";
        this.mesh.name = base.gameObject.name + "Mesh";
        base.gameObject.GetComponent<MeshFilter>().sharedMesh = this.mesh;
        base.gameObject.GetComponent<MeshRenderer>().sharedMaterial = this.mat;
    }
    private void OnDrawGizmos()
    {
        Gizmos.matrix = base.transform.localToWorldMatrix;
        if (this.points.Count == 0 || this.P.Count < this.points.Count)
        {
            return;
        }
        if (this.showGizmos)
        {
            for (int i = 0; i < this.P.Count - 1; i++)
            {
                this.drawGizmos(this.P[i].p, this.P[i], this.P[i + 1]);
                Gizmos.DrawSphere(this.P[i].p, 0.1f * this.gizmoSize);
            }
            this.drawGizmos(this.P[this.P.Count - 1].p, this.P[this.P.Count - 1], this.P[this.P.Count - 2]);
            Gizmos.DrawSphere(this.P[this.P.Count - 1].p, 0.1f * this.gizmoSize);
            Gizmos.color = Color.white;
            Gizmos.color = Color.blue;
        }
    }
    private void drawNormal()
    {
        for (int i = 0; i < this.mesh.normals.Length; i++)
        {
            Gizmos.DrawLine(base.transform.position + this.mesh.vertices[i], base.transform.position + this.mesh.vertices[i] + this.mesh.normals[i]);
        }
    }
    private void drawGizmos(Vector3 p, Point A, Point B)
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(A.c1, 0.2f * this.gizmoSize);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(A.c2, 0.2f * this.gizmoSize);
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(A.p, B.p);
    }
    private void Update()
    {
        if (this.points.Count < 2)
        {
            return;
        }
        this.segment = Mathf.Clamp(this.segment, 1, 30);
        this.Smoothness = Mathf.Clamp(this.Smoothness, 0f, 1f);
        this.c += 0.01f;
        if (this.np == null || this.spline == null)
        {
            return;
        }
        this.spline.updateSegment(this.segment);
        this.spline.updateSmoothness(this.Smoothness);
        this.spline.updatePointsByGameobjects(this.points);
        this.np.Clear();
        if (this.Smoothness == 0f)
        {
            this.np = this.spline.GetLinearPoints();
        }
        else
        {
            if (this.Smoothness == 1f)
            {
                this.np = this.spline.GetCubicPoints();
            }
            else
            {
                this.np = this.spline.GetBazierPoints(this.Smoothness);
            }
        }
        if (this.np.Count < 3)
        {
            return;
        }
        this.getPfromNP();
        if (this.np == null || this.P.Count < this.points.Count)
        {
            return;
        }
        if (this.np.Count > 2)
        {
            this.makeMesh();
        }
        if (this.mat == null)
        {
            return;
        }
        base.GetComponent<MeshFilter>().sharedMesh = this.mesh;
        base.GetComponent<MeshRenderer>().sharedMaterial = this.mat;
    }
    private void getPfromNP()
    {
        try
        {
            if (this.np.Count != 0 && this.points.Count != 0)
            {
                this.P.Clear();
                this.P.Add(new Point());
                this.P[0].p = this.np[0];
                float num = Mathf.SmoothStep(this.mag(this.points[0].transform.localScale), this.mag(this.points[1].transform.localScale), 0f) + this.MeshWidth;
                this.P[0].c1 = this.Cpoint(this.np[0], this.np[1], num);
                this.P[0].c2 = this.Cpoint(this.np[0], this.np[1], -num);
                int num2 = 1;
                int num3 = 0;
                Color[] array = new Color[this.points.Count];
                for (int i = 0; i < this.points.Count; i++)
                {
                    array[i] = this.points[i].GetComponent<CurvePoint>().color;
                }
                this.tempcolors.Clear();
                this.tempcolors.Add(array[0]);
                for (int j = 1; j < this.np.Count - 1; j++)
                {
                    if (j % this.segment == 0)
                    {
                        this.tempcolors.Add(array[num2]);
                        num2++;
                        num3 = 0;
                    }
                    else
                    {
                        num3++;
                        float t = (float)num3 / (float)this.segment;
                        this.tempcolors.Add(Color.Lerp(array[num2 - 1], array[num2], t));
                        num = Mathf.SmoothStep(this.mag(this.points[num2 - 1].transform.localScale), this.mag(this.points[num2].transform.localScale), t) + this.MeshWidth;
                    }
                    this.P.Add(new Point());
                    this.P[j].p = this.np[j];
                    this.P[j].c1 = this.Cpoint(this.np[j], this.np[j + 1], num);
                    this.P[j].c2 = this.Cpoint(this.np[j], this.np[j + 1], -num);
                }
                this.tempcolors.Add(array[array.Length - 1]);
                this.P.Add(new Point());
                this.P[this.P.Count - 1].p = this.np[this.np.Count - 1];
                this.P[this.P.Count - 1].c1 = this.Cpoint(this.np[this.np.Count - 1], this.np[this.np.Count - 2], -num);
                this.P[this.P.Count - 1].c2 = this.Cpoint(this.np[this.np.Count - 1], this.np[this.np.Count - 2], num);
            }
        }
        catch (IndexOutOfRangeException)
        {
        }
    }
    private float mag(Vector3 v)
    {
        return (v.x + v.y + v.z) / 3f;
    }
    private Vector3 Cpoint(Vector3 A, Vector3 B, float D)
    {
        return A + Vector3.Cross(B - A, Vector3.up).normalized * D;
    }
    private void makeMesh()
    {
        this.verts.Clear();
        this.tris.Clear();
        this.norms.Clear();
        this.uvs.Clear();
        this.colors.Clear();
        this.calculatePlane(this.P[0], this.P[1], 0, true);
        for (int i = 1; i < this.P.Count - 1; i++)
        {
            this.calculatePlane(this.P[i], this.P[i + 1], i * 2, false);
        }
        if (this.mesh == null)
        {
            return;
        }
        this.mesh.Clear();
        this.mesh.vertices = this.verts.ToArray();
        this.mesh.triangles = this.tris.ToArray();
        this.mesh.RecalculateBounds();
        this.mesh.RecalculateNormals();
        this.mesh.uv = this.uvs.ToArray();
        this.mesh.colors = this.colors.ToArray();
    }
    private void calculatePlane(Point _P1, Point _P2, int i, bool isFirst)
    {
        if (isFirst)
        {
            this.verts.Add(_P1.c1);
            this.verts.Add(_P1.c2);
            this.verts.Add(_P2.c1);
            this.verts.Add(_P2.c2);
            int num = i / 4;
            float num2 = 1f / (float)this.segment;
            float num3 = (float)num * num2;
            this.uvs.Add(new Vector2(num3, 0f));
            this.uvs.Add(new Vector2(num3, 1f));
            this.uvs.Add(new Vector2(num3 + num2, 0f));
            this.uvs.Add(new Vector2(num3 + num2, 1f));
            this.colors.Add(this.tempcolors[num]);
            this.colors.Add(this.tempcolors[num]);
            this.colors.Add(this.tempcolors[num + 1]);
            this.colors.Add(this.tempcolors[num + 1]);
            this.tris.Add(i);
            this.tris.Add(i + 2);
            this.tris.Add(i + 1);
            this.tris.Add(i + 1);
            this.tris.Add(i + 2);
            this.tris.Add(i + 3);
            return;
        }
        this.verts.Add(_P2.c1);
        this.verts.Add(_P2.c2);
        int num4 = i / 2;
        float num5 = 1f / (float)this.segment;
        float num6 = (float)num4 * num5;
        this.uvs.Add(new Vector2(num6 + num5, 0f));
        this.uvs.Add(new Vector2(num6 + num5, 1f));
        this.colors.Add(this.tempcolors[num4 + 1]);
        this.colors.Add(this.tempcolors[num4 + 1]);
        this.tris.Add(i);
        this.tris.Add(i + 2);
        this.tris.Add(i + 1);
        this.tris.Add(i + 1);
        this.tris.Add(i + 2);
        this.tris.Add(i + 3);
    }
    private Vector3 getNormal(Point p1, Point p2)
    {
        return Vector3.Cross(p1.c1 - p1.c2, p1.c1 - p2.c1).normalized;
    }
}
