using UnityEngine;

public class EnemyFOV : MonoBehaviour
{
    private EnemyController enemy;

    void Awake()
    {
        // 🔑 Trigger보다 항상 먼저 실행되도록 Awake 사용
        enemy = GetComponentInParent<EnemyController>();

        if (enemy == null)
        {
            Debug.LogError($"[EnemyFOV] EnemyController not found on parent of {name}");
        }
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (enemy == null) return;

        if (col.CompareTag("Player"))
        {
            enemy.StartChase();
        }
    }

    void OnTriggerExit2D(Collider2D col)
    {
        if (enemy == null) return;

        if (col.CompareTag("Player"))
        {
            //enemy.StopChase();
        }
    }
}
