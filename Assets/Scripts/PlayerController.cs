using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using System.Collections.Generic;

public class PlayerAgent : Agent
{
    public float moveSpeed = 3f;
    private Rigidbody2D rb;

    public Transform goal;
    public Transform spawn;

    public LayerMask wallLayer;
    public LayerMask enemyLayer;   // 🔥 Enemy 오브젝트 레이어

    public EnemyController[] enemy;

    public GameObject up, down, left, right;

    private Vector2 lastMoveDir;
    private float prevDistance;

    // 이동 금지 방향 (벽 + 적)
    private Dictionary<Vector2, int> blockedDirTimer = new Dictionary<Vector2, int>();

    private readonly Vector2[] dirs =
    {
        Vector2.up,
        Vector2.down,
        Vector2.left,
        Vector2.right
    };

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public override void OnEpisodeBegin()
    {
        rb.velocity = Vector2.zero;
        transform.localPosition = spawn.localPosition;

        foreach (var e in enemy)
            e.ResetEnemy();

        if (enemy.Length > 0)
            enemy[Random.Range(0, enemy.Length)].SpawnEnemy();

        prevDistance = Vector2.Distance(transform.position, goal.position);
        lastMoveDir = Vector2.zero;
        blockedDirTimer.Clear();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // 🎯 목표 방향
        Vector2 toGoal = goal.position - transform.position;
        sensor.AddObservation(toGoal.normalized);

        // 🚧 벽 + 적 4방향 관측
        foreach (var d in dirs)
        {
            bool wallHit = Physics2D.Raycast(transform.position, d, 0.4f, wallLayer);
            bool enemyHit = Physics2D.Raycast(transform.position, d, 0.6f, enemyLayer);

            sensor.AddObservation(wallHit ? 1f : 0f);
            sensor.AddObservation(enemyHit ? 1f : 0f);
        }
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        // ⏱ 금지 방향 타이머 감소
        var keys = new List<Vector2>(blockedDirTimer.Keys);
        foreach (var k in keys)
        {
            blockedDirTimer[k]--;
            if (blockedDirTimer[k] <= 0)
                blockedDirTimer.Remove(k);
        }

        AddReward(-0.001f); // 시간 패널티

        int action = actions.DiscreteActions[0];
        Vector2 moveDir = Vector2.zero;

        if (action >= 1 && action <= 4)
            moveDir = dirs[action - 1];

        if (moveDir == Vector2.zero)
        {
            rb.velocity = Vector2.zero;
            AddReward(-0.01f);
            return;
        }

        // 🚫 이미 금지된 방향
        if (blockedDirTimer.ContainsKey(moveDir))
        {
            rb.velocity = Vector2.zero;
            AddReward(-0.05f);
            return;
        }

        // 🚧 벽 감지
        if (Physics2D.Raycast(transform.position, moveDir, 0.2f, wallLayer))
        {
            rb.velocity = Vector2.zero;
            blockedDirTimer[moveDir] = 15;
            AddReward(-0.1f);
            return;
        }

        // 👿 적 오브젝트 감지 (핵심)
        if (Physics2D.Raycast(transform.position, moveDir, 0.3f, enemyLayer))
        {
            rb.velocity = Vector2.zero;
            blockedDirTimer[moveDir] = 20;   // 벽과 동일하게 취급
            AddReward(-0.12f);
            return;
        }

        // ✅ 이동
        rb.velocity = moveDir * moveSpeed;

        if (moveDir == lastMoveDir)
            AddReward(-0.005f);

        lastMoveDir = moveDir;
        UpdateDirectionVisual(moveDir);

        // 🎯 골 거리 보상 (완만)
        float currDist = Vector2.Distance(transform.position, goal.position);
        float delta = prevDistance - currDist;

        if (delta > 0)
            AddReward(delta * 0.02f);
        else
            AddReward(-0.01f);

        prevDistance = currDist;
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Goal"))
        {
            AddReward(+1f);
            EndEpisode();
        }

        if (col.gameObject.CompareTag("Enemy"))
        {
            AddReward(-1f);
            EndEpisode();
        }
    }

    void UpdateDirectionVisual(Vector2 dir)
    {
        if (up) up.SetActive(dir == Vector2.up);
        if (down) down.SetActive(dir == Vector2.down);
        if (left) left.SetActive(dir == Vector2.left);
        if (right) right.SetActive(dir == Vector2.right);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var a = actionsOut.DiscreteActions;
        a[0] = 0;

        if (Input.GetKey(KeyCode.W)) a[0] = 1;
        if (Input.GetKey(KeyCode.S)) a[0] = 2;
        if (Input.GetKey(KeyCode.A)) a[0] = 3;
        if (Input.GetKey(KeyCode.D)) a[0] = 4;
    }
}
