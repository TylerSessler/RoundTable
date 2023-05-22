using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bullet : MonoBehaviour
{
    [SerializeField] int damage;
    [SerializeField] int timer;

    // Start is called before the first frame update
    void Start()
    {
        if (gameObject.layer == 8)
        {
            damage = gameManager.instance.playerScript.weaponDamage;
        }
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
