using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class PlayerAgent : Agent
{
    public float moveSpeed = 3f;
    private Rigidbody2D rb;

    public Transform goal;
    public LayerMask enemyFOVLayer;
    public LayerMask wallLayer;

    private Vector2 moveDir;
    private Vector2 lastPos;

    public EnemyController[] enemy;

    public Transform spawn;

    private float prevDistance;
    private int sameDirCount;
    private Vector2 lastMoveDir;

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

        enemy[Random.Range(0, enemy.Length)].SpawnEnemy();

        prevDistance = Vector2.Distance(transform.position, goal.position);
        lastPos = transform.position;
        lastMoveDir = Vector2.zero;
        sameDirCount = 0;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        Vector2 toGoal = goal.position - transform.position;
        sensor.AddObservation(toGoal.normalized);

        Vector2[] dirs = { Vector2.up, Vector2.down, Vector2.left, Vector2.right };

        foreach (var dir in dirs)
        {
            bool wall = Physics2D.Raycast(transform.position, dir, 0.6f, wallLayer);
            sensor.AddObservation(wall ? 1f : 0f);

            // 🔑 핵심: 이 방향으로 갔을 때 거리 변화
            Vector2 futurePos = (Vector2)transform.position + dir * 0.5f;
            float futureDist = Vector2.Distance(futurePos, goal.position);
            sensor.AddObservation(prevDistance - futureDist);
        }
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        int action = actions.DiscreteActions[0];
        moveDir = Vector2.zero;

        if (action == 1) moveDir = Vector2.up;
        else if (action == 2) moveDir = Vector2.down;
        else if (action == 3) moveDir = Vector2.left;
        else if (action == 4) moveDir = Vector2.right;

        rb.velocity = moveDir * moveSpeed;

        AddReward(-0.001f);

        float currDist = Vector2.Distance(transform.position, goal.position);

        if (moveDir != Vector2.zero)
        {
            bool wallAhead = Physics2D.Raycast(transform.position, moveDir, 0.5f, wallLayer);

            if (wallAhead)
                AddReward(-0.1f);
            else
                AddReward((prevDistance - currDist) * 0.2f);
        }

        // 🌀 같은 방향 반복 패널티
        if (moveDir == lastMoveDir)
            sameDirCount++;
        else
            sameDirCount = 0;

        if (sameDirCount > 15)
            AddReward(-0.08f);

        lastMoveDir = moveDir;
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
}
