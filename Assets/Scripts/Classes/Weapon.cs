using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public float reloadTime;
    public int ammoCapacity;
    public float fireRate;
    public float damage;
    public virtual void Attack() { }
    public virtual void Special() { }
    public virtual void SwitchIn() { }
    public virtual void SwitchOut() { }
}
