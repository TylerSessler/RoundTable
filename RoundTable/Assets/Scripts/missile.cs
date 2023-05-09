using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class missile : MonoBehaviour
{
    [SerializeField] int damage;
    [SerializeField] int timer;
    [SerializeField] GameObject explosion;

    // Start is called before the first frame update
    void Start()
    {
        Destroy(gameObject, timer);
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger)
            return;

        IDamage damageable = other.GetComponent<IDamage>();

        Instantiate(explosion, gameObject.transform.position, explosion.transform.rotation);

        if (damageable != null)
        {
            damageable.takeDamage(damage);
        }

        Destroy(gameObject);
    }
}
