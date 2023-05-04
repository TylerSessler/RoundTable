using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class weapon : ScriptableObject
{
    public string label;
    public int damage;
    public float rate;
    public float range;
    public int ammo;
    public int maxAmmo;
    public int clipSize;
    public int maxClip;
    public bool canZoom;
    public float reloadTime;
    public GameObject model;
    public GameObject barrelPos;
    public AudioClip gunShot;
    public Texture sprite;
    public AudioClip gunShotAud;
    public AudioClip audReload;
    [Range (0, 1)] public float gunShotAudVol;
    [Range (0, 1)] public float audReloadVol;
}
