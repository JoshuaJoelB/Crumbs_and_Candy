using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatBar : MonoBehaviour
{
    public Slider slider;
    public TMP_Text label;
    public string title = "HP";

    public void Set(int value, int max)
    {
        slider.wholeNumbers = true;     // int lang, walang decimal
        slider.minValue = 0;
        slider.maxValue = max;
        slider.value = value;
        if (label != null) label.text = title + " " + value + "/" + max;
    }
}