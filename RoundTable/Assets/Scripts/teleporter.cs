using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class teleporter : MonoBehaviour
{
    [SerializeField] scenesManager.Scene dest;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            scenesManager.instance.LoadScene(dest);
        }
    }
}
