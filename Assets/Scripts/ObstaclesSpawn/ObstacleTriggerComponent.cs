using System;
using UnityEngine;

public class ObstacleTriggerComponent : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            print("hit");
        }
    }
}
