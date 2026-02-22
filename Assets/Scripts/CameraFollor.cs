using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollor : MonoBehaviour
{
    public Transform player;
    public Vector3 offset = new Vector3(0f, 3f, -8f);

    public float smoothTime = 0.3f;  // ← Reemplaza smoothSpeed
    private Vector3 velocity = Vector3.zero;

    void LateUpdate()
    {
        if (player == null) return;

        Vector3 targetPosition = player.position + offset;
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
        transform.LookAt(player);
    }
}
