using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public float chaseSpeed = 3f;
    public Transform player;

    private Rigidbody2D rb;
    private bool isChasing = false;


    public GameObject up;
    public GameObject down;
    public GameObject left;
    public GameObject right;
    public Transform spawn;

    public GameObject enemy;

    public GameObject defaltDir;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Spawn()
    {
        transform.localPosition = spawn.localPosition;
    }

    void FixedUpdate()
    {
        if (!isChasing)
        {
            rb.velocity = Vector2.zero;

            return;
        }

        Vector2 dir = (player.position - transform.position).normalized;
        rb.velocity = dir * chaseSpeed;
        SetDirection(dir);
    }

    void SetDirection(Vector2 dir)
    {
        up.SetActive(Mathf.Abs(dir.y) > Mathf.Abs(dir.x) && dir.y > 0);
        down.SetActive(Mathf.Abs(dir.y) > Mathf.Abs(dir.x) && dir.y < 0);
        left.SetActive(Mathf.Abs(dir.x) > Mathf.Abs(dir.y) && dir.x < 0);
        right.SetActive(Mathf.Abs(dir.x) > Mathf.Abs(dir.y) && dir.x > 0);
    }

    public void StartChase()
    {
        isChasing = true;
    }

    public void StopChase()
    {
        isChasing = false;
    }

    public void SpawnEnemy()
    {
        defaltDir.SetActive(true);
        enemy.SetActive(true);
    }

    public void ResetEnemy()
    {
        isChasing = false;
        rb.velocity = Vector2.zero;
        transform.localPosition = spawn.localPosition;

        up.SetActive(false);
        down.SetActive(false);
        left.SetActive(false);
        right.SetActive(false);

        enemy.SetActive(false);
    }
}
