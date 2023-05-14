using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class weapon : ScriptableObject
{
    [Header("Weapon Transforms")]
    public Vector3 weaponHolderPos;
    public Vector3 weaponRot;
    public Vector3 weaponScale;
    public Vector3 weaponSightsPos;
    public Vector3 shootPos;

    [Header("Weapon Aiming")]
    public float zoomMaxFov;
    public int zoomInFOVSpeed;
    public int zoomOutFOVSpeed;
    public int ADSSpeed;

    [Header("Weapon Stats")]
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
    public Texture sprite;
    public AudioClip weaponShootAud;
    public AudioClip weaponReloadAud;
    [Range(0, 1)] public float weaponShotVol;
    [Range(0, 1)] public float weaponReloadVol;
}
