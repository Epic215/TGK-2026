using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float smoothTime = 0.3f;
    public Vector3 offset;

    private Vector3 velocity = Vector3.zero;

    // SHAKE
    public float shakeDuration = 0.2f;
    public float shakeMagnitude = 0.2f;
    private float currentShakeTime = 0f;

    void Update()
    {
        if (target != null)
        {
            Vector3 targetPosition = target.position + offset;

            // Jeśli trwa shake → dodaj losowe przesunięcie
            if (currentShakeTime > 0)
            {
                targetPosition += Random.insideUnitSphere * shakeMagnitude;
                currentShakeTime -= Time.deltaTime;
            }

            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
        }
    }

    public void shake()
    {
        currentShakeTime = shakeDuration;
    }
}