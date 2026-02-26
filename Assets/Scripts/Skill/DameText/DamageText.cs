using UnityEngine;
using TMPro;

public class DamageText : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float lifeTime = 1f;

    private TextMeshPro text;
    private float timer;

    void Awake()
    {
        text = GetComponent<TextMeshPro>();
    }

    public void Setup(int damage)
    {
        text.text = damage.ToString();
    }

    void Update()
    {
        transform.position += Vector3.up * moveSpeed * Time.deltaTime;

        timer += Time.deltaTime;
        if (timer >= lifeTime)
        {
            Destroy(gameObject);
        }
    }
}