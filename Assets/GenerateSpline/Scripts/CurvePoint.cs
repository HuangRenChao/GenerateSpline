using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CurvePoint : MonoBehaviour {

    public float GizmoSize = 0.25f;
    public float width;
    public Color color = Color.white;
    private void Start()
    {
    }
    private void Update()
    {
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(base.transform.position, this.GizmoSize);
        this.width = this.mag(base.transform.localScale);
    }
    private float mag(Vector3 v)
    {
        return (v.x + v.y + v.z) / 3f;
    }
}
