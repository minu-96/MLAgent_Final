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
    public LayerMask enemyLayer;   
    public LayerMask FOVLayer;

    public EnemyController[] enemy;

    public GameObject up, down, left, right;

    private Vector2 lastMoveDir; // 🔑 마지막 이동 방향 = '앞'
    private float prevDistance;

    private Dictionary<Vector2, int> blockedDirTimer = new Dictionary<Vector2, int>();

    private readonly Vector2[] dirs =
    {
        Vector2.up,
        Vector2.down,
        Vector2.left,
        Vector2.right
    };

    // 감지 거리
    private float wallBlockDist = 0.3f;      // 벽 차단 거리
    private float enemyBlockDist = 0.5f;     // 적 오브젝트 차단 거리
    private float fovBlockDist = 0.5f;       // 적 시야 차단 거리

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

        // 🚧 벽/적/적시야 감지
        foreach (var d in dirs)
        {
            // 월드 기준
            bool wallHit = Physics2D.Raycast(transform.position, d, wallBlockDist, wallLayer);
            bool enemyHit = Physics2D.Raycast(transform.position, d, enemyBlockDist, enemyLayer);
            bool fovHit = Physics2D.Raycast(transform.position, d, fovBlockDist, FOVLayer);

            sensor.AddObservation(wallHit ? 1f : 0f);
            sensor.AddObservation(enemyHit ? 1f : 0f);
            sensor.AddObservation(fovHit ? 1f : 0f);
        }

        // 🔑 마지막 이동 방향도 관측값으로 추가 (앞 방향 정보)
        Vector2 forward = lastMoveDir == Vector2.zero ? Vector2.up : lastMoveDir;
        sensor.AddObservation(forward);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        // 금지 방향 타이머 감소
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

        // 🔑 이동 방향 앞 벡터 기준
        Vector2 forward = lastMoveDir == Vector2.zero ? Vector2.up : lastMoveDir;

        // 이미 금지된 방향
        if (blockedDirTimer.ContainsKey(moveDir))
        {
            rb.velocity = Vector2.zero;
            AddReward(-0.05f);
            return;
        }

        // 벽 감지
        if (Physics2D.Raycast(transform.position, moveDir, wallBlockDist, wallLayer))
        {
            rb.velocity = Vector2.zero;
            blockedDirTimer[moveDir] = 15;
            AddReward(-0.1f);
            return;
        }

        // 적 오브젝트 감지
        if (Physics2D.Raycast(transform.position, moveDir, enemyBlockDist, enemyLayer))
        {
            rb.velocity = Vector2.zero;
            blockedDirTimer[moveDir] = 20;
            AddReward(-0.2f);
            return;
        }

        // 적 시야 감지
        if (Physics2D.Raycast(transform.position, moveDir, fovBlockDist, FOVLayer))
        {
            rb.velocity = Vector2.zero;
            blockedDirTimer[moveDir] = 20;
            AddReward(-0.12f);
            return;
        }

        // 이동
        rb.velocity = moveDir * moveSpeed;

        // 반복 이동 페널티
        if (moveDir == lastMoveDir)
            AddReward(-0.005f);

        lastMoveDir = moveDir;
        UpdateDirectionVisual(moveDir);

        // 거리 기반 보상 (골에 가까워지면 +, 멀어지면 -)
        float currDist = Vector2.Distance(transform.position, goal.position);
        float delta = prevDistance - currDist;

        if (delta > 0)
            AddReward(delta * 0.01f);
        else
            AddReward(-0.005f);

        prevDistance = currDist;
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Goal"))
        {
            AddReward(+1.5f);
            EndEpisode();
        }

        if (col.gameObject.CompareTag("Enemy"))
        {
            AddReward(-2f);
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
