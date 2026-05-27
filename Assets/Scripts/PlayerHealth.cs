
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int maxLives = 5;

    public int currentLives;

    private bool isDead = false;

    void Start()
    {
        currentLives = maxLives;
    }

    public void TakeDamage(int damage)
    {
        if (isDead)
            return;

        currentLives -= damage;

        Debug.Log("Harry recibio daño. Vidas: " + currentLives);

        if (currentLives <= 0)
        {
            Die();
        }
    }

    public void Heal(int amount)
    {
        currentLives += amount;

        if (currentLives > maxLives)
        {
            currentLives = maxLives;
        }
    }

    void Die()
    {
        isDead = true;

        Debug.Log("Harry murio");

        // Aqui luego pondremos animacion de muerte
    }
}

