using UnityEngine;
using System.Collections;

public class Rotater : MonoBehaviour
{
    [SerializeField]
    private float angleVelocity = 90.0f;

    // Update is called once per frame
    void Update()
    {
        transform.rotation *= Quaternion.AngleAxis(angleVelocity * Time.deltaTime, Vector3.back);
    }
}
