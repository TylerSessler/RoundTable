using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class explode : MonoBehaviour
{
    [SerializeField] int damage;
    [SerializeField] float timer;

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

        if (damageable != null)
        {
            damageable.takeDamage(damage);
        }

        Destroy(gameObject);
    }
}
