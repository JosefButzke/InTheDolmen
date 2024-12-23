using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SunCycle : MonoBehaviour
{
    public Light sunLight;
    private float dayDuration = 20f; // Duration of a full day in seconds
    private float timeOfDay = 0.5f;

    private void Update()
    {

        timeOfDay += Time.deltaTime / dayDuration;
        if (timeOfDay >= 1) timeOfDay = 0;
        sunLight.transform.rotation = Quaternion.Euler((timeOfDay * 360f) - 90, 0, 0);
        sunLight.intensity = Mathf.Clamp01(1 - Mathf.Abs(0.5f - timeOfDay) * 2) * 2;

        float color = Mathf.Clamp01(1 - Mathf.Abs(0.5f - timeOfDay) * 2);

        RenderSettings.skybox.SetColor("_Tint", new Color(0, color, color));
    }
}