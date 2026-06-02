using UnityEngine;

public class ItemVisualEffect : MonoBehaviour
{
    public float rotationSpeed = 75f;
    public float bobHeight = 0.18f;
    public float bobSpeed = 2f;
    public float pulseAmount = 0.08f;
    public float pulseSpeed = 2.5f;

    Vector3 startLocalPosition;
    Vector3 startLocalScale;
    float timeOffset;

    void Awake()
    {
        startLocalPosition = transform.localPosition;
        startLocalScale = transform.localScale;
        timeOffset = Random.value * Mathf.PI * 2f;
    }

    void Update()
    {
        float time = Time.time + timeOffset;

        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);

        float bob = Mathf.Sin(time * bobSpeed) * bobHeight;
        transform.localPosition = startLocalPosition + Vector3.up * bob;

        float pulse = 1f + Mathf.Sin(time * pulseSpeed) * pulseAmount;
        transform.localScale = startLocalScale * pulse;
    }
}
