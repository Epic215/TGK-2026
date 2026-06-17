using UnityEngine;

namespace Hexfire.Weapons
{
  [RequireComponent(typeof(Rigidbody))]
  public class Projectile : MonoBehaviour
  {
    static int bulletLayer = -1;
    static int mandalaBulletLayer = -1;
    static bool layersResolved;

    float damage;
    float speed;
    float lifetime;
    Vector3 moveDirection;
    string ownerTag;
    Transform shooter;
    Rigidbody body;
    bool initialized;

    static void ResolveLayers()
    {
      if (layersResolved)
        return;

      bulletLayer = LayerMask.NameToLayer("Bullet");
      mandalaBulletLayer = LayerMask.NameToLayer("MandalaBullet");

      if (bulletLayer >= 0)
        Physics.IgnoreLayerCollision(bulletLayer, bulletLayer, true);

      layersResolved = true;
    }

    void Awake()
    {
      ResolveLayers();
      body = GetComponent<Rigidbody>();
      body.useGravity = false;
      body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

      SphereCollider sphere = GetComponent<SphereCollider>();
      if (sphere == null)
        sphere = gameObject.AddComponent<SphereCollider>();
      sphere.isTrigger = true;
      sphere.radius = 0.35f;

      if (bulletLayer >= 0)
        gameObject.layer = bulletLayer;
    }

    void Start()
    {
      if (initialized)
        ApplyVelocity();
    }

    public void Initialize(
      float damageAmount,
      float moveSpeed,
      Vector3 direction,
      float lifeSeconds,
      string shooterTag,
      Transform shooterTransform)
    {
      if (body == null)
        body = GetComponent<Rigidbody>();

      damage = damageAmount;
      speed = Mathf.Max(0f, moveSpeed);
      moveDirection = direction.sqrMagnitude > 0.0001f ? direction.normalized : transform.forward;
      lifetime = lifeSeconds;
      ownerTag = shooterTag;
      shooter = shooterTransform;
      initialized = true;

      ApplyVelocity();
      Destroy(gameObject, lifetime);
    }

    void FixedUpdate()
    {
      if (!initialized || body == null || speed <= 0f)
        return;

      Vector3 velocity = body.linearVelocity;
      if (velocity.sqrMagnitude < 0.01f)
        body.linearVelocity = moveDirection * speed;
      else
        body.linearVelocity = velocity.normalized * speed;
    }

    void ApplyVelocity()
    {
      if (body == null || speed <= 0f)
        return;

      body.WakeUp();
      body.linearVelocity = moveDirection * speed;
    }

    void OnTriggerEnter(Collider other)
    {
      if (!initialized)
        return;

      if (shooter != null && other.transform.IsChildOf(shooter))
        return;

      if (!string.IsNullOrEmpty(ownerTag) && other.CompareTag(ownerTag))
        return;

      if (IsOtherProjectile(other))
        return;

      if (other.CompareTag("Enemy"))
      {
        TryDealDamage(other.gameObject);
        Destroy(gameObject);
        return;
      }

      if (other.CompareTag("wall") || other.CompareTag("MAP_OBJECT_WALLS"))
      {
        Destroy(gameObject);
        return;
      }

      if (other.GetComponentInParent<IDamageable>() != null)
      {
        TryDealDamage(other.gameObject);
        Destroy(gameObject);
      }
    }

    static bool IsOtherProjectile(Collider other)
    {
      ResolveLayers();

      if (other.GetComponent<Projectile>() != null)
        return true;

      if (other.GetComponent<Bullet>() != null)
        return true;

      if (other.GetComponent<MandalaBullet>() != null)
        return true;

      int layer = other.gameObject.layer;
      if (bulletLayer >= 0 && layer == bulletLayer)
        return true;

      if (mandalaBulletLayer >= 0 && layer == mandalaBulletLayer)
        return true;

      return false;
    }

    void TryDealDamage(GameObject target)
    {
      IDamageable damageable = target.GetComponentInParent<IDamageable>();
      if (damageable != null)
      {
        damageable.TakeDamage(damage, gameObject);
        return;
      }

      if (target.CompareTag("Player"))
      {
        Hexfire.PlayerHealth hexHealth = target.GetComponent<Hexfire.PlayerHealth>();
        if (hexHealth != null)
        {
          hexHealth.TakeDamage(Mathf.RoundToInt(damage));
          return;
        }

        target.GetComponent<PlayerHealth>()?.TakeDamage(Mathf.RoundToInt(damage));
        return;
      }

      if (target.CompareTag("Enemy"))
        target.GetComponent<EnemyHealth>()?.TakeDamage(Mathf.RoundToInt(damage));
    }
  }
}
