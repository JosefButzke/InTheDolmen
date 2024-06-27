using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatsManager : MonoBehaviour
{
    public GameObject statsBattery;

    public GameObject statsStamina;

    public static StatsManager Instance { get; private set; }

    private void Start()
    {
        Instance = this;
    }

    public void ReduceStamina(int value)
    {
        statsStamina.GetComponent<StatsBar>().DecreaseValue(value);
    }

    public void ReduceBattery(int value)
    {
        statsBattery.GetComponent<StatsBar>().DecreaseValue(value);
    }
}
