using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public float moveSpeed = 2.5f;
    public float chaseSpeed = 3.5f;

    public Transform player;

    public GameObject up;
    public GameObject down;
    public GameObject left;
    public GameObject right;

    private Rigidbody2D rb;
    private Vector2 moveDir = Vector2.left;
    private bool isChasing = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        SetDirection(moveDir);
    }

    void FixedUpdate()
    {
        if (isChasing)
        {
            Vector2 dir = (player.position - transform.position).normalized;
            rb.velocity = dir * chaseSpeed;
            SetDirection(dir);
        }
        else
        {
            rb.velocity = moveDir * moveSpeed;
        }
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
}
