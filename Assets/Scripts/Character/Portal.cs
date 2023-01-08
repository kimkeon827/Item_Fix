using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class Portal : MonoBehaviour
{
    [SerializeField]
    private string playerTag;

    [SerializeField]
    private Portal target;

    [SerializeField]
    private Transform portalPoint;

    public Transform PortalPoint { get { return portalPoint; } }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            other.transform.position = target.PortalPoint.position;
        }
    }

}


