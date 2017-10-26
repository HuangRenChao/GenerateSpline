using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flowroad : MonoBehaviour {

    public List<GameObject> points;
    private int totalNum;
    private float smoothness = 0f;
    private int segment = 50;
    private List<Vector3> np;

    public List<Vector3> Np {
        get {
            if (np == null)
            {
                List<Vector3> lastPoints = new List<Vector3>();
                for (int i = 0; i < points.Count; i++)
                {
                    lastPoints.Add(points[i].transform.localPosition);
                }
                np = calculatePoint(lastPoints, this.segment, this.smoothness);
                totalNum = np.Count;
            }
            return np;
        }
    }

    [ContextMenu("Auto-Copy References")]
    void Copy()
    {
        //FlowMoveAnimation st = gameObject.GetComponent<FlowMoveAnimation>();
        SplineTool st = gameObject.GetComponent<SplineTool>();
        if (st == null) Debug.LogError("this gameobject SplineTool compent is null");
        points = st.points;
    }


    public static List<Vector3> calculatePoint(List<Vector3> cp, int _segment, float _smoothness)
    {
        int mSegment = Mathf.Clamp(_segment, 1, 100);
        List<Vector3> np2 = new List<Vector3>();
        int num = 1;
        np2.Add(cp[0]); ;
        if (_smoothness == 0)
        {
            for (int i = 0; i < cp.Count - 1; i++)
            {
                for (int j = 0; j < mSegment; j++)
                {
                    if (i != 0 || j != 0)
                    {
                        num++;
                        float num2 = (float)j * (1f / (float)mSegment);
                        np2.Add(Linear(cp[i], cp[i + 1], num2));
                    }
                }
            }
        }
        else
        {
            for (int i = 0; i < cp.Count - 1; i++)
            {
                for (int j = 0; j < mSegment; j++)
                {
                    if (i != 0 || j != 0)
                    {
                        float item = (float)num * (1f / ((float)mSegment * (float)(cp.Count - 1)));
                        np2.Add(QuadraticN(cp, item));
                        num++;
                    }
                }
            }
        }

        np2.Add(cp[cp.Count - 1]);
        return np2;
    }

    public static Vector3 Linear(Vector3 p0, Vector3 p1, float t)
    {
        t = Mathf.Clamp01(t);
        Vector3 result;
        result.x = (1f - t) * p0.x + t * p1.x;
        result.y = (1f - t) * p0.y + t * p1.y;
        result.z = (1f - t) * p0.z + t * p1.z;
        return result;
    }

    public static Vector3 QuadraticN(List<Vector3> p, float t)
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

    public Vector3 GetWorldPos(int index) {
        if(Np!=null)
        return transform.rotation * (Np[index]) + transform.position;
        return Vector3.zero;
    }

    public int GetTotalNum() {
        if (Np != null)
            return Np.Count;
        return 0;
    }
}
