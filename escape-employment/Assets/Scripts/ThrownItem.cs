using System.Collections;
using UnityEngine;

public class ThrownItem : MonoBehaviour
{
    [SerializeField] private float notifyRadius = 20f;

    private void Awake()
    {
        // Always ensure a visible mesh exists — runs before Init() is ever called
        if (GetComponentInChildren<Renderer>() == null)
        {
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.SetParent(transform, false);
            sphere.transform.localScale = Vector3.one;
            Destroy(sphere.GetComponent<Collider>());
            Debug.LogWarning("[ThrownItem] Prefab had no Renderer — created a default sphere.");
        }

        // Force a visible scale
        transform.localScale = Vector3.one * 0.5f;

        // If there is a Rigidbody, make it kinematic so physics doesn't pull it through the floor
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;
    }

    /// <summary>Call immediately after Instantiate. Starts the throw animation.</summary>
    /// <param name="duration">How long the distraction lasts after landing.</param>
    /// <param name="fromPos">World position of the throwing player.</param>
    public void Init(float duration, Vector3 fromPos)
    {
        Debug.Log($"[ThrownItem] Init — throwing from {fromPos} to {transform.position}");
        StartCoroutine(ThrowRoutine(duration, fromPos));
    }

    private IEnumerator ThrowRoutine(float duration, Vector3 fromPos)
    {
        Debug.Log("[ThrownItem] Coroutine started.");

        // Land at the same height as the player's feet (fromPos is player centre, -0.6 ≈ feet)
        Vector3 landPos = transform.position;
        landPos.y = fromPos.y - 0.6f;
        Debug.Log($"[ThrownItem] Landing at y={landPos.y:F2} (player centre y={fromPos.y:F2})");

        // Travel arc over 0.4 s
        float travelTime = 0.4f;
        for (float t = 0f; t < travelTime; t += Time.deltaTime)
        {
            float p = t / travelTime;
            Vector3 pos = Vector3.Lerp(fromPos, landPos, p);
            pos.y += Mathf.Sin(p * Mathf.PI) * 2f;   // parabolic arc
            transform.position = pos;
            yield return null;
        }
        transform.position = landPos;
        Debug.Log($"[ThrownItem] Landed at {landPos}. Notifying guards...");

        // Search every GuardController in the scene — no Collider needed
        GuardController[] guards = (GuardController[])Object.FindObjectsOfType(typeof(GuardController));
        Debug.Log($"[ThrownItem] Found {guards.Length} guard(s) in scene.");

        foreach (GuardController guard in guards)
        {
            float dist = Vector3.Distance(landPos, guard.transform.position);
            Debug.Log($"[ThrownItem] Guard '{guard.name}' is {dist:F1}m away (radius={notifyRadius}m).");
            if (dist <= notifyRadius)
                guard.OnDistracted(landPos, duration);
        }

        yield return new WaitForSeconds(duration);
        Destroy(gameObject);
    }
}
