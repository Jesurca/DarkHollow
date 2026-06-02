
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Invisibility : MonoBehaviour
{
    public Renderer[] renderers;

    public float invisibleTime = 5f;

    bool invisible;
    Coroutine invisibilityRoutine;
    readonly List<MaterialColorState> originalColors = new List<MaterialColorState>();

    public void Activate(float duration)
    {
        invisibleTime = duration;

        if (invisibilityRoutine != null)
        {
            StopCoroutine(invisibilityRoutine);
            RestoreOriginalColors();
        }

        invisibilityRoutine = StartCoroutine(BecomeInvisible());
    }

    IEnumerator BecomeInvisible()
    {
        invisible = true;
        CacheRenderers();

        foreach (Renderer rend in renderers)
        {
            SetRendererAlpha(rend, 0.35f);
        }

        Debug.Log("Harry invisible. Los enemigos dejan de seguirlo.");

        yield return new WaitForSeconds(invisibleTime);

        RestoreOriginalColors();
        invisible = false;
        invisibilityRoutine = null;

        Debug.Log("Harry visible.");
    }

    void CacheRenderers()
    {
        if (renderers == null || renderers.Length == 0)
        {
            renderers = GetComponentsInChildren<Renderer>(true);
        }

        originalColors.Clear();

        foreach (Renderer rend in renderers)
        {
            if (rend == null)
            {
                continue;
            }

            foreach (Material material in rend.materials)
            {
                originalColors.Add(new MaterialColorState(material));
            }
        }
    }

    void SetRendererAlpha(Renderer rend, float alpha)
    {
        if (rend == null)
        {
            return;
        }

        foreach (Material material in rend.materials)
        {
            if (material == null)
            {
                continue;
            }

            Color color = Color.white;

            if (material.HasProperty("_BaseColor"))
            {
                color = material.GetColor("_BaseColor");
                color.a = alpha;
                material.SetColor("_BaseColor", color);
            }

            if (material.HasProperty("_Color"))
            {
                color = material.GetColor("_Color");
                color.a = alpha;
                material.SetColor("_Color", color);
            }
        }
    }

    void RestoreOriginalColors()
    {
        foreach (MaterialColorState colorState in originalColors)
        {
            colorState.Restore();
        }
    }

    class MaterialColorState
    {
        readonly Material material;
        readonly bool hasBaseColor;
        readonly bool hasColor;
        readonly Color baseColor;
        readonly Color color;

        public MaterialColorState(Material material)
        {
            this.material = material;

            if (material == null)
            {
                return;
            }

            hasBaseColor = material.HasProperty("_BaseColor");
            hasColor = material.HasProperty("_Color");

            if (hasBaseColor)
            {
                baseColor = material.GetColor("_BaseColor");
            }

            if (hasColor)
            {
                color = material.GetColor("_Color");
            }
        }

        public void Restore()
        {
            if (material == null)
            {
                return;
            }

            if (hasBaseColor)
            {
                material.SetColor("_BaseColor", baseColor);
            }

            if (hasColor)
            {
                material.SetColor("_Color", color);
            }
        }
    }

    public bool IsInvisible()
    {
        return invisible;
    }
}
