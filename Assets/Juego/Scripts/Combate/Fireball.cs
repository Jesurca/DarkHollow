
using UnityEngine;

public class Fireball : MonoBehaviour
{
    public float speed = 20f;

    public float lifeTime = 2.5f;
    public Color magicColor = new Color(0.55f, 0.25f, 1f, 1f);
    public float lightIntensity = 2.5f;
    public float lightRange = 4f;
    public Material projectileMaterial;

    static Material magicMaterial;
    Rigidbody rb;

    void Start()
    {
        ConfigureVisuals();

        rb = GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.linearVelocity = transform.forward * speed;
            rb.useGravity = false;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        }

        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            Destroy(other.gameObject);

            Destroy(gameObject);
        }
    }

    void ConfigureVisuals()
    {
        Material material = GetMagicMaterial();

        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();

        if (meshRenderer != null)
        {
            meshRenderer.sharedMaterial = material;
            meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            meshRenderer.receiveShadows = false;
        }

        TrailRenderer trail = GetComponent<TrailRenderer>();

        if (trail != null)
        {
            trail.sharedMaterial = material;
            trail.time = 0.35f;
            trail.widthMultiplier = 0.22f;
            trail.minVertexDistance = 0.03f;
            trail.numCapVertices = 4;
            trail.numCornerVertices = 4;
            trail.emitting = true;

            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new[]
                {
                    new GradientColorKey(magicColor, 0f),
                    new GradientColorKey(new Color(0.2f, 0.9f, 1f), 1f)
                },
                new[]
                {
                    new GradientAlphaKey(0.9f, 0f),
                    new GradientAlphaKey(0f, 1f)
                }
            );

            trail.colorGradient = gradient;

            AnimationCurve width = new AnimationCurve();
            width.AddKey(0f, 1f);
            width.AddKey(1f, 0f);
            trail.widthCurve = width;
        }

        Light pointLight = GetComponent<Light>();

        if (pointLight == null)
        {
            pointLight = gameObject.AddComponent<Light>();
        }

        pointLight.type = LightType.Point;
        pointLight.color = magicColor;
        pointLight.intensity = lightIntensity;
        pointLight.range = lightRange;
        pointLight.shadows = LightShadows.None;
    }

    Material GetMagicMaterial()
    {
        if (projectileMaterial != null)
        {
            return projectileMaterial;
        }

        if (magicMaterial != null)
        {
            return magicMaterial;
        }

        Shader shader = Shader.Find("DarkHollow/MagicProjectile");

        if (shader == null)
        {
            shader = Shader.Find("Universal Render Pipeline/Unlit");
        }

        magicMaterial = new Material(shader)
        {
            name = "Magia_Proyectil_Runtime"
        };

        if (magicMaterial.HasProperty("_Color"))
        {
            magicMaterial.SetColor("_Color", magicColor);
        }

        if (magicMaterial.HasProperty("_EmissionIntensity"))
        {
            magicMaterial.SetFloat("_EmissionIntensity", 3f);
        }

        return magicMaterial;
    }
}

