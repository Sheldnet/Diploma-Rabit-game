using Character;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrajectoryRenderer : MonoBehaviour
{
    [Header("Display Controls")]
    [SerializeField] private LineRenderer lineRenderer;
    //[SerializeField] private NewBehaviourScript _player;
    //private LayerMask PlayerCollisionMask;

    //private void Awake()
    //{
    //    int finishLayer = _player.gameObject.layer;
    //    for (int i = 0; i < 32; i++)
    //    {
    //        if (!Physics.GetIgnoreLayerCollision(finishLayer, i))
    //        {
    //            PlayerCollisionMask =
    //        }
    //    }
    //}
    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    public void ShowTrajectory(Vector3 origin, Vector3 jumpVector)
    {
        Vector3[] points = new Vector3[50];
        lineRenderer.positionCount = points.Length;

        for (int i = 0; i < points.Length; i++)
        {
            float time = i * 0.1f;
            points[i] = origin + jumpVector * time + Physics.gravity * time * time / 2f;

            if (points[i].y < - 10) 
            {
                lineRenderer.positionCount = i;
                break;
            }
        }

        lineRenderer.SetPositions(points);

    }

    public void ClearTrajectory()
    {
        lineRenderer.positionCount = 0; 
    }

}
