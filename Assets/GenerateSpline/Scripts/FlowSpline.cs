using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlowSpline : MonoBehaviour
{

    private SplineTool st;
    private bool isPlaying;
    public List<GameObject> points;
    public float MeshWidth = 1f;
    private float Smoothness = 1f;
    private CubicSpline spline;
    private List<Vector3> np = new List<Vector3>();
    private List<Point> P = new List<Point>();
    private List<Color> tempcolors = new List<Color>();
    private List<Vector3> verts = new List<Vector3>();
    private List<int> tris = new List<int>();
    private List<Color> colors = new List<Color>();
    private List<Vector3> norms = new List<Vector3>();
    private List<Vector2> uvs = new List<Vector2>();
    public Mesh mesh;
    public Material mat;

    private int segment = 30;


    // Use this for initialization
    void Start()
    {
        //st = gameObject.GetComponent<SplineTool>();
    }

    // Update is called once per frame
    void Update()
    {
        if (isPlaying)
        {
            if (points == null)
            {
                Debug.Log("points is null");
                return;
            }
            currentFrame++;
            if (currentFrame < frameTick) return;
            currentFrame = 0;
            if (spline == null) {

                totalNum = points.Count;
                spline = new CubicSpline(this.points, this.segment, this.Smoothness, true);
                smallTotalNum = this.segment;
            }
            //Debug.Log("currentNum:"+ currentNum);    
            #region MakeMesh
            //计算当前需要的点数
            if (smallCurrentNum == 0)
            {
                currentNum++;
                num = currentNum + 1;
                if (num > totalNum)
                {
                    isPlaying = false;
                    return;
                }
                smallCurrentNum += 1;
            }
            //Debug.Log("num:"+ num+ " smallCurrentNum:" + smallCurrentNum+ " totalNum:" + totalNum);
            GetPointsData();
            UpdateMesh();
            #endregion
        }
    }

    private void OnGUI()
    {
        if (GUI.Button(new Rect(10, 20, 100, 30), "Play"))
        {
            isPlaying = true;
        }
    }
    [ContextMenu("Auto-Copy References")]
    void Copy() {
        st = gameObject.GetComponent<SplineTool>();
        if (st == null) Debug.LogError("this gameobject SplineTool compent is null");
        this.mesh = st.mesh;
        this.mat = st.mat;
        points = st.points;
        MeshWidth = st.MeshWidth;
    }


    //private void OnDrawGizmos()
    //{
    //    Gizmos.color = Color.red;
    //    if (np2 != null && np2.Count > 0)
    //    {
    //        Debug.Log("OnDrawGizmos...................");
    //        for (int i = 0; i < np2.Count; i++)
    //        {
    //            Gizmos.DrawSphere(np2[i], 0.2f);
    //        }
    //    }
    //}

    private int totalNum,smallTotalNum;
    private int currentNum, smallCurrentNum;
    private int frameTick=2;
    private int currentFrame;
    List<Vector3> np2;
    int num = 0;
    List<Vector3> currentPoints = new List<Vector3>();
    private void GetPointsData() {
        //添加当前的点
        currentPoints.Clear();
        //把当前的点切分成segment段
        currentPoints.Add(points[0].transform.localPosition);
        if (num != 2)
        {
            for (int i = 1; i < num - 1; i++)
            {
                currentPoints.Add(points[i].transform.localPosition);
            }
        }
        //补全最后一段
        if (np2 == null) {
            List<Vector3> lastPoints = new List<Vector3>();
            for (int i = 0; i < points.Count; i++)
            {
                lastPoints.Add(points[i].transform.localPosition);
                
            }
            np2 = calculatePoint(lastPoints);
        }
        smallCurrentNum++;
        if (np2 == null) Debug.LogError("np2 is null");
        int j = num - 2;
        currentPoints.Add(np2[j * smallTotalNum + smallCurrentNum]);
        if (smallCurrentNum >= smallTotalNum)
            smallCurrentNum = 0;
        //Debug.Log("currentPoints:"+ currentPoints.Count);
    }

    private List<Vector3> calculatePoint(List<Vector3> cp)
    {
        this.segment = Mathf.Clamp(this.segment, 1, 100);
        List<Vector3> np2 = new List<Vector3>();
        int num = 1;
        np2.Add(cp[0]);;
        for (int i = 0; i < cp.Count - 1; i++)
        {
            for (int j = 0; j < this.segment; j++)
            {
                if (i != 0 || j != 0)
                {
                    float item = (float)num * (1f / ((float)this.segment * (float)(cp.Count - 1)));
                    //Debug.Log("item:"+ item);
                    np2.Add(QuadraticN(cp, item));
                    num++;
                }
            }
        }
        np2.Add(cp[cp.Count - 1]);
        return np2;
    }

    private Vector3 QuadraticN(List<Vector3> p, float t)
    {
        int num = p.Count - 1;
        Vector3 zero = Vector3.zero;
        zero.x = Mathf.Pow(1f - t, (float)num) * p[0].x;
        zero.y = Mathf.Pow(1f - t, (float)num) * p[0].y;
        zero.z = Mathf.Pow(1f - t, (float)num) * p[0].z;
        for (int i = 1; i < num; i++)
        {
            zero.x += (float)CubicSpline.BinomCoefficient((long)num, (long)i) * Mathf.Pow(1f - t, (float)(num - i)) * (Mathf.Pow(t, (float)i) * p[i].x);
            zero.y += (float)CubicSpline.BinomCoefficient((long)num, (long)i) * Mathf.Pow(1f - t, (float)(num - i)) * (Mathf.Pow(t, (float)i) * p[i].y);
            zero.z += (float)CubicSpline.BinomCoefficient((long)num, (long)i) * Mathf.Pow(1f - t, (float)(num - i)) * (Mathf.Pow(t, (float)i) * p[i].z);
        }
        zero.x += Mathf.Pow(t, (float)num) * p[num].x;
        zero.y += Mathf.Pow(t, (float)num) * p[num].y;
        zero.z += Mathf.Pow(t, (float)num) * p[num].z;
        return zero;
    }

    private void UpdateMesh()
    {
        if (this.currentPoints.Count < 2)
        {
            return;
        }
        this.segment = Mathf.Clamp(this.segment, 1, 100);
        this.Smoothness = Mathf.Clamp(this.Smoothness, 0f, 1f);
        if (this.np == null || this.spline == null)
        {
            return;
        }
        this.spline.updateSegment(this.segment);
        this.spline.updateSmoothness(this.Smoothness);
        //this.spline.updatePointsByGameobjects(this.points);
        this.spline.updatePointsByPoints(currentPoints);
        this.np.Clear();
        if (this.Smoothness == 0f)
        {
            this.np = this.spline.GetLinearPoints();
        }
        else
        {
            if (this.Smoothness == 1f)
            {
                //Debug.Log("GetCubicPoints");
                this.np = this.spline.GetCubicPoints();
            }
            else
            {
                Debug.Log("GetBazierPoints");
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
