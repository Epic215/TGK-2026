using UnityEngine;

public class MandalaBullet : MonoBehaviour
{
    public int damage = 10;
    public float curveRate = 20f;

    [HideInInspector]
    public string ownerTag = "";

    private Rigidbody rb;
    private Vector3 startPosition;
    private bool hasLeft = false;
    private const float MIN_TRAVEL_DISTANCE = 1.5f;
    private const float RETURN_THRESHOLD = 0.4f;

    private void Start()
    {
        startPosition = transform.position;
        rb = GetComponent<Rigidbody>();
        gameObject.layer = LayerMask.NameToLayer("MandalaBullet");
        Physics.IgnoreLayerCollision(
            LayerMask.NameToLayer("MandalaBullet"),
            LayerMask.NameToLayer("MandalaBullet")
        );
    }

    private void Update()
    {
        Vector3 velocity = Quaternion.AngleAxis(-curveRate * Time.deltaTime, Vector3.up) * rb.linearVelocity;
        rb.linearVelocity = velocity;
        transform.rotation = Quaternion.LookRotation(velocity);

        float dist = Vector3.Distance(transform.position, startPosition);
        if (!hasLeft && dist > MIN_TRAVEL_DISTANCE) hasLeft = true;
        if (hasLeft && dist < RETURN_THRESHOLD) Destroy(gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        string hitTag = collision.gameObject.tag;

        if (!string.IsNullOrEmpty(ownerTag) && hitTag == ownerTag) return;
        if (collision.gameObject.GetComponent<MandalaBullet>() != null) return;
        if (collision.gameObject.GetComponent<Bullet>() != null) return;

        if (hitTag == "Player")
        {
          var hexHealth = collision.gameObject.GetComponent<Hexfire.PlayerHealth>();
          if (hexHealth != null)
            hexHealth.TakeDamage(damage);
          else
            collision.gameObject.GetComponent<PlayerHealth>()?.TakeDamage(damage);
        }
        // else if (hitTag == "Enemy")
        //     collision.gameObject.GetComponent<EnemyHealth>()?.TakeDamage(damage);

        Destroy(gameObject);
    }
}