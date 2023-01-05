using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;


public class ProgressBarScript : MonoBehaviour
{
    [SerializeField] private Color color;
    [SerializeField] private float maxValue = 100;
    [SerializeField] private float value = 0;
    private RectTransform valueRect;

    private void Awake()
    {
        var rect = transform.Find("ValueRect");
        valueRect = rect.GetComponent<RectTransform>();
        rect.GetComponent<Image>().color = color;
        valueRect.localScale = new Vector3(2* value / maxValue, valueRect.localScale.y, valueRect.localScale.z);
    }

    public void SetValue(float newValue)
    {
        value = newValue;
        valueRect.localScale = new Vector3(2* value / maxValue, valueRect.localScale.y, valueRect.localScale.z);
    }
}
