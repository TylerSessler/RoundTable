using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionTrigger : MonoBehaviour
{
    // Start is called before the first frame update

    public System.Action<Collider> OnTriggerEnter;
    public System.Action<Collider> OnTriggerExit;

    //void OnTriggerEnter(Collider other)
    //{
    //    OnTriggerEnter?.Invoke(other);
    //}

    //void OnTriggerExit(Collider other)
    //{
    //    OnTriggerExit?.Invoke(other);
    //}

}