using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MenuButtonVisualStyle : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [Tooltip("Color normal del fondo del boton.")]
    public Color colorNormal = new Color(0.03f, 0.025f, 0.04f, 0.86f);

    [Tooltip("Color del fondo cuando el mouse pasa por encima.")]
    public Color colorHover = new Color(0.10f, 0.085f, 0.13f, 0.94f);

    [Tooltip("Color del fondo cuando el boton esta presionado.")]
    public Color colorPresionado = new Color(0.015f, 0.012f, 0.02f, 0.98f);

    [Tooltip("Color normal del texto.")]
    public Color colorTextoNormal = new Color(0.92f, 0.86f, 0.68f, 1f);

    [Tooltip("Color del texto en hover.")]
    public Color colorTextoHover = new Color(1f, 0.94f, 0.74f, 1f);

    [Tooltip("Color del texto cuando el boton esta presionado.")]
    public Color colorTextoPresionado = new Color(0.72f, 0.62f, 0.44f, 1f);

    Image fondo;
    TMP_Text texto;
    Button boton;
    bool mouseEncima;

    void Awake()
    {
        fondo = GetComponent<Image>();
        texto = GetComponentInChildren<TMP_Text>(true);
        boton = GetComponent<Button>();

        if (boton != null)
        {
            boton.transition = Selectable.Transition.None;
        }

        ApplyNormal();
    }

    void OnEnable()
    {
        ApplyNormal();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        mouseEncima = true;
        ApplyHover();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        mouseEncima = false;
        ApplyNormal();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        ApplyPressed();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (mouseEncima)
        {
            ApplyHover();
            return;
        }

        ApplyNormal();
    }

    void ApplyNormal()
    {
        SetColors(colorNormal, colorTextoNormal);
    }

    void ApplyHover()
    {
        SetColors(colorHover, colorTextoHover);
    }

    void ApplyPressed()
    {
        SetColors(colorPresionado, colorTextoPresionado);
    }

    void SetColors(Color backgroundColor, Color textColor)
    {
        if (fondo != null)
        {
            fondo.color = backgroundColor;
        }

        if (texto != null)
        {
            texto.color = textColor;
        }
    }
}
