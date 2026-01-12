using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuarterViewCamera : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] Transform target;

    [Header("View")]
    [SerializeField] float distance = 14f;
    [SerializeField] float height = 16f;
    [Range(10f, 80f)]
    [SerializeField] float pitch = 50f;
    [SerializeField] float yaw = 45f;

    [Header("Smoothing")]
    [SerializeField] float followSmooth = 12f;
    [SerializeField] float rotateSmooth = 12f;

    void LateUpdate()
    {
        if (!target) return;

        // =========================
        // 위치 계산 (고정 거리)
        // =========================
        Quaternion yawRot = Quaternion.Euler(0f, yaw, 0f);
        Vector3 offset = yawRot * Vector3.back * distance;
        offset.y += height;

        Vector3 desiredPos = target.position + offset;
        transform.position = Smooth(transform.position, desiredPos, followSmooth);

        // =========================
        // 회전 계산 (Yaw + Pitch 고정)
        // =========================
        Quaternion rotYaw = Quaternion.Euler(0f, yaw, 0f);
        Quaternion rotPitch = Quaternion.Euler(pitch, 0f, 0f);
        Quaternion desiredRot = rotYaw * rotPitch;

        transform.rotation = Smooth(transform.rotation, desiredRot, rotateSmooth);
    }

    // =========================
    // 프레임 독립 스무딩
    // =========================
    Vector3 Smooth(Vector3 current, Vector3 target, float smooth)
    {
        float t = 1f - Mathf.Exp(-smooth * Time.deltaTime);
        return Vector3.Lerp(current, target, t);
    }

    Quaternion Smooth(Quaternion current, Quaternion target, float smooth)
    {
        float t = 1f - Mathf.Exp(-smooth * Time.deltaTime);
        return Quaternion.Slerp(current, target, t);
    }
}
