using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SunCycle : MonoBehaviour
{
    public GameObject sunLight;

    private void Update()
    {
        sunLight.transform.Rotate(10 * Time.deltaTime, 0, 0, Space.World);


        sunLight.GetComponent<Light>().intensity = Mathf.Lerp(0, 1, transform.rotation.eulerAngles.x);


    }
}