using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class Shake : MonoBehaviour
{
    [SerializeField]
    private CinemachineImpulseSource shakeSource;

    [SerializeField]
    private float shakeForce;

    [SerializeField]
    private float shakeDuration;

    void Start()
    {
        shakeSource.GenerateImpulseWithForce(shakeForce);
    }

    
}
