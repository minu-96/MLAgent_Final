using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 3f;

    public GameObject up;
    public GameObject down;
    public GameObject left;
    public GameObject right;

    private Rigidbody2D rb;
    private Vector2 moveDir = Vector2.down;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        SetDirection(Vector2.down);
    }

    void Update()
    {
        HandleInput();
        RaycastCheck();
    }

    void FixedUpdate()
    {
        rb.velocity = moveDir * moveSpeed;
    }

    void HandleInput()
    {
        Vector2 input = Vector2.zero;

        if (Input.GetKey(KeyCode.W)) input = Vector2.up;
        if (Input.GetKey(KeyCode.S)) input = Vector2.down;
        if (Input.GetKey(KeyCode.A)) input = Vector2.left;
        if (Input.GetKey(KeyCode.D)) input = Vector2.right;

        if (input != Vector2.zero)
        {
            moveDir = input;
            SetDirection(input);
        }
    }

    void SetDirection(Vector2 dir)
    {
        up.SetActive(dir == Vector2.up);
        down.SetActive(dir == Vector2.down);
        left.SetActive(dir == Vector2.left);
        right.SetActive(dir == Vector2.right);
    }

    void RaycastCheck()
    {
        RaycastHit2D hit = Physics2D.Raycast(
            transform.position,
            moveDir,
            1.5f,
            LayerMask.GetMask("EnemyFOV", "Wall")
        );

        Debug.DrawRay(transform.position, moveDir * 1.5f, Color.red);

        if (hit.collider != null)
        {
            // 여기서 ML-Agent라면 Observation으로 사용
            // 지금은 디버그용
        }
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Enemy"))
        {
            Debug.Log("패배!");
            Time.timeScale = 0;
        }
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Goal"))
        {
            Debug.Log("승리!");
            Time.timeScale = 0;
        }
    }
}
