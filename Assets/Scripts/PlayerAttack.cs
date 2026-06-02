using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerAttack : MonoBehaviour
{
    public GameObject fireballPrefab;

    public Transform firePoint;
    public bool hasWand;

    void Start()
    {
        ConfigureWandPickup();
    }

    void Update()
    {
        if (hasWand && IsShootPressed())
        {
            Shoot();
        }
    }

    bool IsShootPressed()
    {
        return Keyboard.current != null &&
            Keyboard.current.spaceKey.wasPressedThisFrame;
    }

    public void EnableShooting()
    {
        hasWand = true;
    }

    void Shoot()
    {
        if (fireballPrefab == null || firePoint == null)
        {
            return;
        }

        Instantiate(
            fireballPrefab,
            firePoint.position,
            firePoint.rotation
        );
    }

    void ConfigureWandPickup()
    {
        GameObject wand = FindSceneObject("Varita");

        if (wand == null)
        {
            return;
        }

        wand.SetActive(true);

        if (wand.GetComponent<WandPickup>() == null)
        {
            wand.AddComponent<WandPickup>();
        }

        Collider collider = wand.GetComponent<Collider>();

        if (collider == null)
        {
            BoxCollider boxCollider = wand.AddComponent<BoxCollider>();
            boxCollider.size = Vector3.one * 1.2f;
            collider = boxCollider;
        }

        collider.isTrigger = true;
    }

    GameObject FindSceneObject(string objectName)
    {
        foreach (GameObject gameObject in Resources.FindObjectsOfTypeAll<GameObject>())
        {
            if (!gameObject.scene.IsValid())
            {
                continue;
            }

            if (gameObject.scene != SceneManager.GetActiveScene())
            {
                continue;
            }

            if (gameObject.name == objectName)
            {
                return gameObject;
            }
        }

        return null;
    }
}
