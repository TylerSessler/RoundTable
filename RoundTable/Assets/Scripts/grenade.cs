using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class grenade : MonoBehaviour
{
    [SerializeField] Rigidbody projectile;
    [SerializeField] MeshRenderer model;
    [SerializeField] ParticleSystem explosion;
    [SerializeField] float explosionDelay;
    [SerializeField] float explosionRange;
    int explosionDamage;

    private void Start()
    {
        projectile = this.GetComponent<Rigidbody>();
        model = this.GetComponent<MeshRenderer>();
        StartCoroutine(explode());
        explosionDamage = gameManager.instance.playerScript.activeWeapon.damage;

    }


    IEnumerator explode()
    {
        // Wait for a delay before exploding
        yield return new WaitForSeconds(gameManager.instance.playerScript.activeWeapon.delay);
        // Stop grenade from moving
        projectile.isKinematic = true;
        // Turn off model
        model.enabled = false;
        // Play explosion audio

        // Deal damage to nearby objects.
        Collider[] hitColliders = Physics.OverlapSphere(projectile.transform.position, explosionRange);
        foreach (Collider hitCollider in hitColliders)
        {
            IDamage damageable = hitCollider.GetComponent<IDamage>();
            if (damageable != null)
            {
                // Explosion does less damage to player
                if (hitCollider.CompareTag("Player"))
                {
                    damageable.takeDamage(explosionDamage);
                }
                else
                {
                    damageable.takeDamage(explosionDamage*2);
                }
            }
        }
        
        // Play explosion effect
        explosion.Play();
        yield return new WaitForSeconds(0.75f);
        Destroy(gameObject);
    }


}
