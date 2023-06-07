using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrajectoryRenderer : MonoBehaviour
{
    [Header("Display Controls")]
    [SerializeField] private LineRenderer lineRenderer;

    //[SerializeField]
    //[Range(10, 100)]
    //private int _linePoints = 100;

    //[SerializeField]
    //[Range(0.01f, 0.25f)]
    //private float _timeBetweenPoints = 0.1f;

    // Start is called before the first frame update
    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    public void ShowTrajectory(Vector3 origin, Vector3 jumpVector)
    {
        Vector3[] points = new Vector3[50];
        lineRenderer.positionCount = points.Length;
        //lineRenderer.positionCount= Mathf.CeilToInt(_linePoints / _timeBetweenPoints) + 1;

        for (int i = 0; i < points.Length; i++)
        {
            float time = i * 0.1f;
            points[i] = origin + jumpVector * time + Physics.gravity * time * time / 2f;

        }

        lineRenderer.SetPositions(points);
    }

    public void ClearTrajectory()
    {
        lineRenderer.positionCount = 0; 
    }

}
