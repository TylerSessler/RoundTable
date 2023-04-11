using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public int damage;
    public float range;
    public float rate;
    public int ammo;
    public int clipSize;
    public int maxAmmo;
    public int maxClip;
    public float reloadTime;


    

    // Start is called before the first frame update
    void Start()
    {
        

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    Weapon generate(int dmg, float range, float rate, int clip, int ammo, float reload)
    {
        Weapon gun = new Weapon();
        gun.damage = dmg;
        gun.range = range;
        gun.rate = rate;
        gun.clipSize = clip;
        gun.ammo = ammo;
        gun.maxAmmo = ammo;
        gun.reloadTime = reload;
        gun.maxClip = clip;

        return gun;
    }

    public Weapon[] generateInventory()
    {
        Weapon[] Inventory = new Weapon[5];
        // Dmg, range, fire-rate, clip size, ammo-max, reload time
        // Tweak as needed
        Inventory[0] = generate(2, 15, 0.6f, 9, 30, 2);
        Inventory[1] = generate(1, 20, 0.25f, 21, 30, 3);
        Inventory[2] = generate(8, 60, 3, 4, 12, 5);
        Inventory[3] = generate(12, 10, 6, 1, 3, 6);
        Inventory[4] = generate(2, 1.25f, 2, 1, 1, 0);
        return Inventory;
    }
}
