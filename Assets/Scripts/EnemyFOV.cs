using UnityEngine;

public class EnemyFOV : MonoBehaviour
{
    private EnemyController enemy;

    void Start()
    {
        enemy = GetComponentInParent<EnemyController>();
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            enemy.StartChase();
        }
    }
}
