using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VirtualCamInfo : MonoBehaviour
{
    [SerializeField]
    private bool mUseFollow;

    [SerializeField]
    private bool mUseLookAt;

    public bool UseFollow() { return mUseFollow; }
    public bool UseLookAt() { return mUseLookAt; }
}