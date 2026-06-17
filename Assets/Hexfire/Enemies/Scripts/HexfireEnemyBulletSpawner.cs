using UnityEngine;

namespace Hexfire.Enemies
{
  public static class HexfireEnemyBulletSpawner
  {
    public static void Spawn(
      GameObject prefab,
      Vector3 spawnPos,
      Vector3 velocity,
      MonoBehaviour owner,
      bool mandalaLayer = false,
      float lifetime = 5f)
    {
      if (prefab == null || owner == null)
        return;

      velocity.y = 0f;
      if (velocity.sqrMagnitude < 0.0001f)
        return;

      GameObject bullet = Object.Instantiate(prefab, spawnPos, Quaternion.LookRotation(velocity));
      bullet.transform.SetParent(null);

      if (mandalaLayer)
      {
        int layer = LayerMask.NameToLayer("MandalaBullet");
        if (layer >= 0)
          bullet.layer = layer;
      }

      Bullet normal = bullet.GetComponent<Bullet>();
      if (normal != null)
        normal.ownerTag = "Enemy";

      MandalaBullet mandala = bullet.GetComponent<MandalaBullet>();
      if (mandala != null)
        mandala.ownerTag = "Enemy";

      Collider bulletCol = bullet.GetComponent<Collider>();
      if (bulletCol != null)
      {
        foreach (Collider ownerCol in owner.GetComponents<Collider>())
          Physics.IgnoreCollision(bulletCol, ownerCol);
      }

      Rigidbody rb = bullet.GetComponent<Rigidbody>();
      if (rb != null)
      {
        rb.isKinematic = false;
        rb.linearVelocity = velocity;
      }

      Object.Destroy(bullet, lifetime);
    }
  }
}
