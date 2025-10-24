using UnityEngine;
using System.Collections.Generic;

public class ARCameraReactiveTrees : MonoBehaviour
{
    [Header("Setup")]
    public Transform arCamera;                     // ARCamera (Transform)
    public List<Transform> targets;                // Trees / rocks

    [Header("Rotation Settings")]
    [Range(1f, 90f)] public float maxYawAngle = 30f;
    [Range(10f, 360f)] public float turnSpeed = 120f;

    // Store each tree's initial LOCAL rotation (so parents can be rotated freely)
    private readonly Dictionary<Transform, Quaternion> baseLocalRot = new();

    void Start()
    {
        if (arCamera == null && Camera.main != null)
            arCamera = Camera.main.transform;

        baseLocalRot.Clear();
        foreach (var t in targets)
        {
            if (t != null) baseLocalRot[t] = t.localRotation;
        }
    }

    void Update()
    {
        if (arCamera == null) return;

        foreach (var t in targets)
        {
            if (t == null) continue;

            // Compute camera direction in the tree's PARENT space
            var parent = t.parent;
            Vector3 camPosLocal = parent ? parent.InverseTransformPoint(arCamera.position) : arCamera.position;
            Vector3 toCamLocal = camPosLocal - t.localPosition;

            // Flatten to yaw only
            toCamLocal.y = 0f;
            if (toCamLocal.sqrMagnitude < 0.0001f) continue;

            // Desired local rotation to face camera
            Quaternion targetLocal = Quaternion.LookRotation(toCamLocal.normalized, Vector3.up);

            // Constrain yaw around the saved base local rotation
            Quaternion limitedLocal = LimitLocalYaw(baseLocalRot[t], targetLocal, maxYawAngle);

            // Smoothly rotate in LOCAL space
            t.localRotation = Quaternion.RotateTowards(t.localRotation, limitedLocal, turnSpeed * Time.deltaTime);
        }
    }

    // Limit yaw relative to a base LOCAL rotation, preserving only Y
    private static Quaternion LimitLocalYaw(Quaternion baseLocal, Quaternion targetLocal, float limit)
    {
        float baseY = baseLocal.eulerAngles.y;
        float targetY = targetLocal.eulerAngles.y;
        float yawDiff = Mathf.DeltaAngle(baseY, targetY);
        yawDiff = Mathf.Clamp(yawDiff, -limit, limit);
        return Quaternion.Euler(0f, baseY + yawDiff, 0f);
    }
}
