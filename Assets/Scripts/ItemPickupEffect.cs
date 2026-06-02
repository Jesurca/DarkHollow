using UnityEngine;

public enum ItemPickupType
{
    HealthPotion,
    Clock,
    Book
}

public class ItemPickupEffect : MonoBehaviour
{
    [Tooltip("Tipo de efecto que aplica este item al recogerlo.")]
    public ItemPickupType itemType = ItemPickupType.HealthPotion;

    [Tooltip("Cantidad de vida que recupera la pocion.")]
    public int healAmount = 1;

    [Tooltip("Duracion del efecto del reloj o libro en segundos.")]
    public float effectDuration = 6f;

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        if (itemType == ItemPickupType.HealthPotion)
        {
            PlayerHealth health = other.GetComponent<PlayerHealth>();

            if (health != null)
            {
                health.Heal(healAmount);
            }
        }
        else if (itemType == ItemPickupType.Clock)
        {
            EnemyAI.FreezeAll(effectDuration);
        }
        else if (itemType == ItemPickupType.Book)
        {
            Invisibility invisibility = other.GetComponent<Invisibility>();

            if (invisibility == null)
            {
                invisibility = other.gameObject.AddComponent<Invisibility>();
            }

            invisibility.Activate(effectDuration);
        }

        Destroy(gameObject);
    }
}
