//using UnityEditor.Experimental.GraphView;
//using UnityEngine;

//public class EnemyHitbox : MonoBehaviour
//{
//    [Header("Damage Settings")]
//    public int damage = 20;
//    public string targetTag = "Enemy"; // Chỉ gây sát thương cho object có tag này
//    public bool destroyAfterHit = false; // Nếu là đạn 1 lần bắn thì xóa sau va chạm

//    private void OnTriggerEnter(Collider other)
//    {
//        if (!other.CompareTag(targetTag)) return;

//        EnemyDamageHandler damageHandler = other.GetComponent<EnemyDamageHandler>();
//        if (damageHandler != null)
//        {
//            damageHandler.TakeDamage(damage, attacker);
//        }

//        if (destroyAfterHit)
//        {
//            Destroy(gameObject);
//        }
//    }
//}
