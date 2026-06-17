using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float lifetime = 2f;
    public int damage = 10;

    [HideInInspector]
    public string ownerTag = "";

    private void Start()
    {
        Destroy(gameObject, lifetime);
        gameObject.layer = LayerMask.NameToLayer("Bullet");
        Physics.IgnoreLayerCollision(
            LayerMask.NameToLayer("Bullet"),
            LayerMask.NameToLayer("Bullet")
        );
    }

    private void OnCollisionEnter(Collision collision)
    {
        string hitTag = collision.gameObject.tag;


        if (collision.gameObject.CompareTag("Bullet"))
        {
            Physics.IgnoreCollision(GetComponent<Collider>(), collision.collider);
            return;
        }

        // Ignoruj właściciela
        if (!string.IsNullOrEmpty(ownerTag) && hitTag == ownerTag) return;
        // PlayerDash dash = collision.gameObject.GetComponent<PlayerDash>();
        // if (dash != null && dash.IsDashing) return;

        // Ignoruj inne pociski
        if (collision.gameObject.GetComponent<Bullet>() != null) return;
        if (collision.gameObject.GetComponent<Hexfire.Weapons.Projectile>() != null) return;
        if (collision.gameObject.GetComponent<MandalaBullet>() != null) return;

        // Zadaj obrażenia
        if (hitTag == "Player")
        {
          var hexHealth = collision.gameObject.GetComponent<Hexfire.PlayerHealth>();
          if (hexHealth != null)
            hexHealth.TakeDamage(damage);
          else
            collision.gameObject.GetComponent<PlayerHealth>()?.TakeDamage(damage);
        }
        else if (hitTag == "Enemy")
            collision.gameObject.GetComponent<EnemyHealth>()?.TakeDamage(damage);

        Destroy(gameObject);
    }
}