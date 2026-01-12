using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Player : MonoBehaviour
{
    enum MoveState
    {
        Idle,
        Moving,
        Dashing
    }

    [Header("Refs")]
    [SerializeField] Camera mainCamera;
    [SerializeField] NavMeshAgent agent;
    [SerializeField] GameObject marker;

    [Header("Move")]
    [SerializeField] LayerMask groundMask;
    [SerializeField] float moveSpeed = 6f;

    [Header("Dash")]
    [SerializeField] float dashDistance = 8f;
    [SerializeField] float dashDuration = 0.22f;
    [SerializeField] float dashCooldown = 1f;
    [SerializeField] AnimationCurve dashCurve;

    MoveState state = MoveState.Idle;

    float nextDashTime;
    Vector3 moveDirection;

    void Awake()
    {
        agent.speed = moveSpeed;
        agent.updateRotation = false;
        marker.SetActive(false);
    }

    void Update()
    {
        HandleClickMove();
        HandleDashInput();
        HandleRotation();
        CheckArrival();
    }

    // =========================
    // 이동 입력
    // =========================
    void HandleClickMove()
    {
        if (state == MoveState.Dashing) return;
        if (!Input.GetMouseButtonDown(1)) return;

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out var hit, 500f, groundMask)) return;

        agent.ResetPath();
        agent.SetDestination(hit.point);

        moveDirection = (hit.point - transform.position).normalized;
        state = MoveState.Moving;

        marker.transform.position = hit.point + Vector3.up * 0.05f;
        marker.SetActive(true);
    }

    // =========================
    // 대시 입력
    // =========================
    void HandleDashInput()
    {
        if (state == MoveState.Dashing) return;
        if (Time.time < nextDashTime) return;
        if (!Input.GetKeyDown(KeyCode.Space)) return;

        CancelMove();

        if (moveDirection.sqrMagnitude < 0.001f)
            moveDirection = transform.forward;

        StartCoroutine(DashRoutine(moveDirection));
    }

    // =========================
    // 대시 처리
    // =========================
    IEnumerator DashRoutine(Vector3 dir)
    {
        state = MoveState.Dashing;
        nextDashTime = Time.time + dashCooldown;

        agent.isStopped = true;
        agent.ResetPath();

        float elapsed = 0f;
        float baseSpeed = dashDistance / dashDuration;

        while (elapsed < dashDuration)
        {
            float t = elapsed / dashDuration;
            float speed = dashCurve.Evaluate(t) * baseSpeed;

            agent.Move(dir * speed * Time.deltaTime);

            elapsed += Time.deltaTime;
            yield return null;
        }

        agent.isStopped = false;
        state = MoveState.Idle;
    }

    // =========================
    // 대쉬 후 처리
    // =========================
    void CancelMove()
    {
        agent.ResetPath();
        marker.SetActive(false);
        moveDirection = Vector3.zero;
        state = MoveState.Idle;
    }

    // =========================
    // 회전 처리
    // =========================
    void HandleRotation()
    {
        if (state == MoveState.Dashing) return;

        Vector3 v = agent.velocity;
        v.y = 0f;

        if (v.sqrMagnitude < 0.01f) return;

        Quaternion target = Quaternion.LookRotation(v);
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            target,
            720f * Time.deltaTime
        );
    }

    // =========================
    // 도착 판정
    // =========================
    void CheckArrival()
    {
        if (state != MoveState.Moving) return;
        if (agent.pathPending) return;

        if (agent.remainingDistance <= agent.stoppingDistance &&
            agent.velocity.sqrMagnitude < 0.01f)
        {
            state = MoveState.Idle;
            agent.ResetPath();
            marker.SetActive(false);
        }
    }
}
