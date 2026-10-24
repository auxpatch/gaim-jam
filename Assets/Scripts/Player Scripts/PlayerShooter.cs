using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;


public class PlayerShooter : MonoBehaviour
{
    public Transform firePoint;
    public GameObject bulletPrefab;

public void Shoot()
    {    
        GameObject obj = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
    }
}
