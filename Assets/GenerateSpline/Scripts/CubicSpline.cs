using System;
using System.Collections.Generic;
using UnityEngine;
[Serializable]
public class CubicSpline {

    public int segment;
    public float smoothness = 0.5f;
    public bool isLocal = true;
    public List<GameObject> ep = new List<GameObject>();
    private List<Vector3> cp = new List<Vector3>();
    public List<Vector3> lp = new List<Vector3>();
    public List<Vector3> np = new List<Vector3>();
    public List<Vector3> bp = new List<Vector3>();
    public List<float> t = new List<float>();
    public List<float> z = new List<float>();
    public CubicSpline(List<GameObject> _cp, int _segment, float _smoothness, bool _isLocal)
    {
        this.isLocal = _isLocal;
        this.cp.Clear();
        for (int i = 0; i < _cp.Count; i++)
        {
            if (_cp[i] == null)
            {
                _cp.RemoveAt(i);
                i--;
            }
            else
            {
                if (_isLocal)
                {
                    this.cp.Add(_cp[i].transform.localPosition);
                }
                else
                {
                    this.cp.Add(_cp[i].transform.position);
                }
            }
        }
        this.segment = _segment;
        this.smoothness = _smoothness;
    }
    public CubicSpline(List<Vector3> _cp, int _segment, float _smoothness)
    {
        this.cp.Clear();
        this.cp = _cp;
        this.segment = _segment;
        this.smoothness = _smoothness;
    }
    public void updateSmoothness(float _smoothness)
    {
        this.smoothness = _smoothness;
    }
    public void updateSegment(int _seg)
    {
        this.segment = _seg;
    }
    public void updatePointsByPoints(List<Vector3> _cp)
    {
        //this.cp.Clear();
        this.cp = _cp;
    }
    public void updatePointsByGameobjects(List<GameObject> _cp)
    {
        this.cp.Clear();
        for (int i = 0; i < _cp.Count; i++)
        {
            if (_cp[i] == null)
            {
                _cp.RemoveAt(i);
                i--;
            }
            else
            {
                if (this.isLocal)
                {
                    this.cp.Add(_cp[i].transform.localPosition);
                }
                else
                {
                    this.cp.Add(_cp[i].transform.position);
                }
            }
        }
    }

    public List<Vector3> GetCubicPoints()
    {
        this.calculateNP();
        return this.np;
    }
    public List<Vector3> GetLinearPoints()
    {
        this.calculateLP();
        return this.lp;
    }
    public List<Vector3> GetBazierPoints(float _smoothness)
    {
        this.smoothness = _smoothness;
        this.calculateLP();
        this.calculateNP();
        this.calculateBP();
        return this.bp;
    }
    private void calculateLP()
    {
        this.lp.Clear();
        int num = 1;
        this.lp.Add(this.cp[0]);
        for (int i = 0; i < this.cp.Count - 1; i++)
        {
            for (int j = 0; j < this.segment; j++)
            {
                if (i != 0 || j != 0)
                {
                    num++;
                    float num2 = (float)j * (1f / (float)this.segment);
                    this.lp.Add(this.Linear(this.cp[i], this.cp[i + 1], num2));
                }
            }
        }
        this.lp.Add(this.cp[this.cp.Count - 1]);
    }
    private void calculateBP()
    {
        this.bp.Clear();
        for (int i = 0; i < this.np.Count; i++)
        {
            if (i > this.lp.Count - 1)
            {
                return;
            }
            Vector3 zero = Vector3.zero;
            zero.x = Mathf.SmoothStep(this.np[i].x, this.lp[i].x, 1f - this.smoothness);
            zero.y = Mathf.SmoothStep(this.np[i].y, this.lp[i].y, 1f - this.smoothness);
            zero.z = Mathf.SmoothStep(this.np[i].z, this.lp[i].z, 1f - this.smoothness);
            this.bp.Add(zero);
        }
    }
    private Vector3 Linear(Vector3 p0, Vector3 p1, float t)
    {
        t = Mathf.Clamp01(t);
        Vector3 result;
        result.x = (1f - t) * p0.x + t * p1.x;
        result.y = (1f - t) * p0.y + t * p1.y;
        result.z = (1f - t) * p0.z + t * p1.z;
        return result;
    }
    private void calculateNP()
    {
        this.segment = Mathf.Clamp(this.segment, 1, 30);
        this.np.Clear();
        int num = 1;
        this.np.Add(this.cp[0]);
        this.t.Clear();
        this.z.Clear();
        this.t.Add(0f);
        this.z.Add(0f);
        for (int i = 0; i < this.cp.Count - 1; i++)
        {
            for (int j = 0; j < this.segment; j++)
            {
                if (i != 0 || j != 0)
                {
                    float item = (float)num * (1f / ((float)this.segment * (float)(this.cp.Count - 1)));
                    this.t.Add(item);
                    this.np.Add(this.QuadraticN(this.cp, item));
                    this.z.Add(this.np[this.np.Count - 1].z);
                    num++;
                }
            }
        }
        this.np.Add(this.cp[this.cp.Count - 1]);
    }
    public static long BinomCoefficient(long n, long k)
    {
        if (k > n)
        {
            return 0L;
        }
        if (n == k)
        {
            return 1L;
        }
        if (k > n - k)
        {
            k = n - k;
        }
        long num = 1L;
        for (long num2 = 1L; num2 <= k; num2 += 1L)
        {
            long arg_29_0 = num;
            long expr_23 = n;
            n = expr_23 - 1L;
            num = arg_29_0 * expr_23;
            num /= num2;
        }
        return num;
    }
    private Vector3 q5(List<Vector3> p, float t)
    {
        Vector3 a = Mathf.Pow(1f - t, 5f) * p[0];
        a += (float)CubicSpline.BinomCoefficient(5L, 1L) * t * Mathf.Pow(1f - t, 4f) * p[1];
        a += (float)CubicSpline.BinomCoefficient(5L, 2L) * Mathf.Pow(t, 2f) * Mathf.Pow(1f - t, 3f) * p[2];
        a += (float)CubicSpline.BinomCoefficient(5L, 3L) * Mathf.Pow(t, 3f) * Mathf.Pow(1f - t, 2f) * p[3];
        a += (float)CubicSpline.BinomCoefficient(5L, 4L) * Mathf.Pow(t, 4f) * Mathf.Pow(1f - t, 1f) * p[4];
        return a + Mathf.Pow(t, 5f) * p[5];
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
    private Vector3 RationalN(List<Vector3> p, float t)
    {
        int num = p.Count - 1;
        Vector3 zero = Vector3.zero;
        Vector3 zero2 = Vector3.zero;
        Vector3 zero3 = Vector3.zero;
        for (int i = 0; i < num; i++)
        {
            zero2.x += (float)CubicSpline.BinomCoefficient((long)num, (long)i) * Mathf.Pow(1f - t, (float)(num - i)) * p[i].x * 1f;
            zero2.y += (float)CubicSpline.BinomCoefficient((long)num, (long)i) * Mathf.Pow(1f - t, (float)(num - i)) * p[i].y * 1f;
            zero2.z += (float)CubicSpline.BinomCoefficient((long)num, (long)i) * Mathf.Pow(1f - t, (float)(num - i)) * p[i].z * 1f;
            zero3.x += (float)CubicSpline.BinomCoefficient((long)num, (long)i) * Mathf.Pow(1f - t, (float)(num - i)) * 1f;
            zero3.y += (float)CubicSpline.BinomCoefficient((long)num, (long)i) * Mathf.Pow(1f - t, (float)(num - i)) * 1f;
            zero3.z += (float)CubicSpline.BinomCoefficient((long)num, (long)i) * Mathf.Pow(1f - t, (float)(num - i)) * 1f;
        }
        zero.x = zero2.x / zero3.x;
        zero.y = zero2.x / zero3.y;
        zero.z = zero2.x / zero3.z;
        return zero;
    }
}
