using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

public enum Families
{
    NonMetal,
    NobleGas,
    Metalloid,
    PostTransitionMetals,
    TransitionMetals,
    AlkalineEarthMetals,
    AlkaliMetals,
};

public class PeriodicTableManager : MonoBehaviour
{
    public UIDocument uiDocument;


    private void OnEnable()
    {
        Element[] scriptableObjects = Resources.LoadAll<Element>("ScriptableObjectsUI");

        Debug.Log(scriptableObjects.Length);

        VisualElement root = uiDocument.rootVisualElement;

        foreach (Element element in scriptableObjects)
        {
            VisualElement elementVisualElement = root.Q<VisualElement>(element.number);

            elementVisualElement.Q<Label>("ElementNumber").text = element.number;
            elementVisualElement.Q<Label>("ElementCode").text = element.code;
            elementVisualElement.Q<Label>("ElementName").text = element.name;
            elementVisualElement.Q<Label>("ElementQuantity").text = element.quantity;
            elementVisualElement.Q<Button>("Element").style.backgroundColor = element.color;
        }
    }
}
