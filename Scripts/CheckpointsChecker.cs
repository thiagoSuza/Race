using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckpointsChecker : MonoBehaviour
{
    public CarController theCar;

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Checkpoints")
        {
            // Debug.Log("Hit Cp " + other.gameObject.GetComponent<CheclPoint>().cpNumber);
            theCar.CheckPointHit(other.gameObject.GetComponent<CheclPoint>().cpNumber);
        }
    }
}
