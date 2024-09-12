using UnityEngine;
using UnityEngine.UI;

public class MouseSensitivityManager : MonoBehaviour
{
    public Slider sensitivitySlider;
    public float sensitivityMultiplier = 1.0f; 

    private void Start()
    {
        sensitivitySlider.onValueChanged.AddListener(HandleSensitivityChange);
    }

    private void HandleSensitivityChange(float value)
    {
        sensitivityMultiplier = value;
    }

    public float GetSensitivity()
    {
        return sensitivityMultiplier;
    }
}
