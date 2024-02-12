using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatsBar : MonoBehaviour
{
    public GameObject barContainer;
    public GameObject bar;
    public int value = 10;
    public Color color = Color.green;

    private List<GameObject> bars = new List<GameObject>();

    void Start()
    {
        CleanUI();
        UpdateUI();
    }

    public void IncreaseValue(int increaseValue)
    {
        value = value - increaseValue;
        UpdateUI();
    }

    public void DecreaseValue(int drecreaseValue)
    {
        value = value - drecreaseValue;
        UpdateUI();
    }

    private void UpdateUI()
    {
        CleanUI();
        for (int i = 0; i < value; i++)
        {
            GameObject instantiatedPrefab = Instantiate(bar, barContainer.transform);

            Image instantiatedPrefabImage = instantiatedPrefab.GetComponent<Image>();

            instantiatedPrefabImage.color = color;

            bars.Add(instantiatedPrefab);
        }
    }

    private void CleanUI()
    {
        foreach (Transform child in barContainer.transform)
        {
            // Destroy the child GameObject
            Destroy(child.gameObject);
        }
    }
}
