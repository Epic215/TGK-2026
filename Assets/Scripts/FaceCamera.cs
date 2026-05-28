using UnityEngine;

public class FaceCamera : MonoBehaviour
{
    public Transform enemy;
    public Vector3 offset = new Vector3(0, 2f, 0);

    void Update()
    {
        transform.position = enemy.position + offset;
        transform.rotation = Camera.main.transform.rotation;
    }
}