using Fusion;
using UnityEngine;

public class EnemyController : NetworkBehaviour
{
    public float moveDistance = 5f; // số bước
    public float moveSpeed = 2f;    // tốc độ
    private float startX;

    [Networked] private float direction { get; set; } // 1 hoặc -1
    [Networked] private float movedSoFar { get; set; } // khoảng cách đã di chuyển

    private Rigidbody2D rb;

    public override void Spawned()
    {
        rb = GetComponent<Rigidbody2D>();
        startX = transform.position.x;
        direction = 1;
        movedSoFar = 0;

        Debug.Log($"[Spawned] Enemy spawned at X={startX}. Direction={direction}");
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority) return;

        float moveStep = moveSpeed * Runner.DeltaTime;
        transform.position += Vector3.right * direction * moveStep;
        movedSoFar += moveStep;

        Debug.Log($"[Move] Position: {transform.position.x:F2}, MovedSoFar: {movedSoFar:F2}, Direction: {direction}");

        if (movedSoFar >= moveDistance)
        {
            direction *= -1;
            movedSoFar = 0;
            Debug.Log($"[Turn Around] New direction: {direction}");
        }
    }
}
