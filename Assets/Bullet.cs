using UnityEngine;

public class Bullet : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        // Ignoruj ściany
        if (collision.gameObject.CompareTag("wall"))
            return;

        // Ignoruj gracza
        if (collision.gameObject.CompareTag("Player"))
            return;

        Destroy(gameObject);
    }
}