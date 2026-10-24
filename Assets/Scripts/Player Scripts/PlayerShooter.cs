using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;


public class PlayerShooter : MonoBehaviour
{
    public Transform firePoint;
    public GameObject bulletPrefab;

   [SerializeField] private int currentUpgradeLevel = 1;


    public void SetUpgradeLevel(int level)
    {
        currentUpgradeLevel = level;

    }
    public void Shoot()
    {
        int level = ScoreManager.Instance !=null
        ? ScoreManager.Instance.CurrentLevel
        : currentUpgradeLevel;
        switch (level)
        {
            case 1:
                SpawnBullet(0f);
                break;
            case 2:
                SpawnBullet(-10f);
                SpawnBullet(10f);
                break;
            case 3:
                SpawnBullet(-20f);
                SpawnBullet(0f);
                SpawnBullet(20f);
                break;
            default:
                SpawnBullet(-30f);
                SpawnBullet(-15f);
                SpawnBullet(0f);
                SpawnBullet(15f);
                SpawnBullet(30f);
                break;
        }

        void SpawnBullet(float angleOffset)
        {
            Quaternion rotation = firePoint.rotation * Quaternion.Euler(0, 0, angleOffset);
            Instantiate(bulletPrefab, firePoint.position, rotation);
        }

    }
}
