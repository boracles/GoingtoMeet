using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move : MonoBehaviour
{
    public Vector3 localDir;
    public float speed = 1;

    void Update()
    {
        transform.Translate(localDir.normalized * speed * Time.deltaTime, Space.Self);
    }
}
