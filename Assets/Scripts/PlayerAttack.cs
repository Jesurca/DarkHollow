using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    public GameObject fireballPrefab;

    public Transform firePoint;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            Shoot();
        }
    }

    void Shoot()
    {
        Instantiate(
            fireballPrefab,
            firePoint.position,
            firePoint.rotation
        );
    }
}