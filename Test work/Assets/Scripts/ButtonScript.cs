using UnityEngine;
using UnityEngine.UI;

public class ButtonScript : MonoBehaviour
{
    private Color baseColor;
    private string baseText;

    void Start ()
    {
        baseColor = gameObject.GetComponent<Image>().color;
        baseText = gameObject.GetComponentInChildren<Text>().text;
    }

    public void ChangeColor() // Изменение цвета и текста кнопки
    {
        if (gameObject.GetComponent<Image>().color != baseColor)
        {
            gameObject.GetComponentInChildren<Text>().text = baseText;
            gameObject.GetComponent<Image>().color = baseColor;
        }
        else
        {
            gameObject.GetComponentInChildren<Text>().text = "On";
            gameObject.GetComponent<Image>().color = new Vector4(baseColor.r * 0.75f, baseColor.g * 0.75f, baseColor.b * 0.75f, baseColor.a);
        }
    }

}
