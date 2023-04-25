using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class weaponPickup : MonoBehaviour
{
    [SerializeField] weapon gun;
    [SerializeField] MeshFilter model;
    [SerializeField] MeshRenderer material;

    private void Start()
    {
        
        model.mesh = gun.model.GetComponent<MeshFilter>().sharedMesh;
        material.material = gun.model.GetComponent<MeshRenderer>().sharedMaterial;
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            gameManager.instance.playerScript.addWeapon(gun);
            Destroy(gameObject);
        }

    }
}
