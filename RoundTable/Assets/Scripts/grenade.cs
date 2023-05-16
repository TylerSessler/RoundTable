using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class grenade : MonoBehaviour
{
    [SerializeField] Rigidbody projectile;
    [SerializeField] float explosionDelay;
    [SerializeField] float throwStrength;
    public grenade instance;

    private void Start()
    {
        instance = this;
        projectile = this.GetComponent<Rigidbody>();    
    }


    public void launchGrenade()
    {

        projectile.AddForce(Camera.main.transform.forward * throwStrength, ForceMode.Impulse);
        StartCoroutine(explode());
    }

    IEnumerator explode()
    {
        yield return new WaitForSeconds(explosionDelay);


    }
}
