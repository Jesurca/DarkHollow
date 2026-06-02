
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class Invisibility : MonoBehaviour
{
    public Renderer[] renderers;

    public float invisibleTime = 5f;

    private bool invisible = false;

    void Update()
    {
        if (Keyboard.current != null &&
            Keyboard.current.hKey.wasPressedThisFrame)
        {
            if (!invisible)
            {
                StartCoroutine(BecomeInvisible());
            }
        }
    }

    IEnumerator BecomeInvisible()
    {
        invisible = true;

        foreach (Renderer rend in renderers)
        {
            Color color = rend.material.color;

            color.a = 0.2f;

            rend.material.color = color;
        }

        Debug.Log("Harry invisible");

        yield return new WaitForSeconds(invisibleTime);

        foreach (Renderer rend in renderers)
        {
            Color color = rend.material.color;

            color.a = 1f;

            rend.material.color = color;
        }

        invisible = false;

        Debug.Log("Harry visible");
    }

    public bool IsInvisible()
    {
        return invisible;
    }
}
