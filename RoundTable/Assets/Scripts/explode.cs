using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class explode : MonoBehaviour
{
    [SerializeField] int damage;
    [SerializeField] float timer;
    [SerializeField] float range;
    [SerializeField] ParticleSystem explosion;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(explodes());
    }


    IEnumerator explodes()
    {

        // Deal damage to nearby objects.
        Collider[] hitColliders = Physics.OverlapSphere(gameObject.transform.position, range);
        foreach (Collider hitCollider in hitColliders)
        {
            IDamage damageable = hitCollider.GetComponent<IDamage>();
            if (damageable != null)
            {
                // Explosion does less damage to player
                if (hitCollider.CompareTag("Player"))
                {
                    damageable.takeDamage(damage);
                }

            }
        }

        // Play explosion effect
        explosion.Play();
        yield return new WaitForSeconds(0.75f);
        Destroy(gameObject);
    }
}
