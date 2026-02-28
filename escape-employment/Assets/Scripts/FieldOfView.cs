using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class FieldOfView : MonoBehaviour
{
    [Header("FOV Parameters")]
    public float viewRadius = 8f;
    [Range(0f, 360f)] public float viewAngle = 90f;

    [Header("Detection")]
    [SerializeField] private LayerMask playerMask;   // Layer 6: Player
    [SerializeField] private LayerMask obstacleMask; // Layer 7: Obstacle

    [Header("Mesh Quality")]
    [SerializeField] private int meshResolution = 24;

    public event Action OnPlayerSpotted;

    private MeshFilter _meshFilter;
    private Mesh _fovMesh;

    private void Awake()
    {
        _meshFilter = GetComponent<MeshFilter>();
        _fovMesh = new Mesh { name = "FOV Mesh" };
        _meshFilter.mesh = _fovMesh;
    }

    private void Start() => StartCoroutine(DetectionRoutine());

    private IEnumerator DetectionRoutine()
    {
        var wait = new WaitForSeconds(0.15f);
        while (true)
        {
            yield return wait;
            CheckForPlayer();
        }
    }

    private void CheckForPlayer()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, viewRadius, playerMask);
        foreach (Collider col in hits)
        {
            Transform target = col.transform;
            Vector3 dir = (target.position - transform.position).normalized;
            if (Vector3.Angle(transform.forward, dir) > viewAngle * 0.5f) continue;
            float dist = Vector3.Distance(transform.position, target.position);
            if (!Physics.Raycast(transform.position, dir, dist, obstacleMask))
            {
                OnPlayerSpotted?.Invoke();
                return;
            }
        }
    }

    private void LateUpdate() => DrawFOVMesh();

    private void DrawFOVMesh()
    {
        float stepAngle = viewAngle / meshResolution;
        Vector3[] verts = new Vector3[meshResolution + 2];
        int[] tris = new int[meshResolution * 3];

        verts[0] = Vector3.zero;
        for (int i = 0; i <= meshResolution; i++)
        {
            float angle = -viewAngle * 0.5f + stepAngle * i;
            Vector3 dir = new Vector3(Mathf.Sin(angle * Mathf.Deg2Rad), 0f,
                                      Mathf.Cos(angle * Mathf.Deg2Rad));
            if (Physics.Raycast(transform.position, transform.TransformDirection(dir),
                out RaycastHit hit, viewRadius, obstacleMask))
                verts[i + 1] = transform.InverseTransformPoint(hit.point);
            else
                verts[i + 1] = dir * viewRadius;
        }
        for (int i = 0; i < meshResolution; i++)
        {
            tris[i * 3] = 0; tris[i * 3 + 1] = i + 1; tris[i * 3 + 2] = i + 2;
        }

        _fovMesh.Clear();
        _fovMesh.vertices = verts;
        _fovMesh.triangles = tris;
        _fovMesh.RecalculateNormals();
    }
}
