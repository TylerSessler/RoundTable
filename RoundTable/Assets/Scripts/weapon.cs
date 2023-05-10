using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class weapon : ScriptableObject
{
    [Header("Gun Transforms")]
    public Vector3 gunPosition;
    public Vector3 gunRotation;
    public Vector3 gunScale;
    public Vector3 gunModeSightsPos;
    public Vector3 shootPos;

    [Header("Gun Aiming")]
    public float zoomMaxFov;
    public int zoomInFovSpeed;
    public int zoomOutFovSpeed;
    public int adsSpeed;
    public int nonADSSpeed;

    [Header("Gun Stats")]
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
    public AudioClip gunShot;
    public Texture sprite;
    public AudioClip gunShotAud;
    public AudioClip audReload;
    [Range(0, 1)] public float gunShotAudVol;
    [Range(0, 1)] public float audReloadVol;
}
