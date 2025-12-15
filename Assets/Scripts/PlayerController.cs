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

    public EnemyController[] enemy;

    public GameObject up;
    public GameObject down;
    public GameObject left;
    public GameObject right;

    public Transform spawn;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public override void OnEpisodeBegin()
    {
        // 위치 리셋
        rb.velocity = Vector2.zero;
        transform.localPosition = spawn.localPosition;

        int j = enemy.Length;
        int r = Random.Range(0, j);
        //for (int i = 0; i < j; i++)
        //{
            enemy[r].ResetEnemy();
            enemy[r].Spawn();
        //}
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // 목표 방향
        Vector2 toGoal = goal.position - transform.position;
        sensor.AddObservation(toGoal.normalized);

        // Raycast (앞 방향 위험 감지)
        RaycastHit2D hit = Physics2D.Raycast(
            transform.position,
            moveDir == Vector2.zero ? Vector2.down : moveDir,
            2f,
            enemyFOVLayer | wallLayer
        );

        sensor.AddObservation(hit.collider != null ? 1f : 0f);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        int action = actions.DiscreteActions[0];
        moveDir = Vector2.zero;

        // 🔑 입력 없으면 이동 X
        switch (action)
        {
            case 1: moveDir = Vector2.up; break;
            case 2: moveDir = Vector2.down; break;
            case 3: moveDir = Vector2.left; break;
            case 4: moveDir = Vector2.right; break;
        }

        rb.velocity = moveDir * moveSpeed;

        // 기본 시간 패널티 (가만히 있어도 손해)
        AddReward(-0.001f);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var actions = actionsOut.DiscreteActions;
        actions[0] = 0;

        if (Input.GetKey(KeyCode.W)) actions[0] = 1;
        else if (Input.GetKey(KeyCode.S)) actions[0] = 2;
        else if (Input.GetKey(KeyCode.A)) actions[0] = 3;
        else if (Input.GetKey(KeyCode.D)) actions[0] = 4;

        if (actions[0] != 0)
        {
            SetDirection(actions[0]);
        }
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Goal"))
        {
            AddReward(+1.0f);
            EndEpisode();
        }

        if (col.gameObject.CompareTag("Enemy"))
        {
            Debug.Log("end");
            AddReward(-1.0f);
            EndEpisode();
        }
    }
    void SetDirection(int dir)
    {
        up.SetActive(dir == 1);
        down.SetActive(dir == 2);
        left.SetActive(dir == 3);
        right.SetActive(dir == 4);
    }

}
