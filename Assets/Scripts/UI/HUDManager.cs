using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

enum Menus
{
    SKILLS,
    INVENTORY,
    CRAFTING
}

public class HUDManager : MonoBehaviour
{
    public GameObject playerStatsUI;
    public GameObject menusUI;
    public GameObject constructionUI;

    public GameObject SkillsMenu;
    public GameObject InventoryMenu;
    public GameObject CraftingMenu;

    public Button MenuLeftButton;
    public TextMeshProUGUI MenuTitle;
    public Button MenuRightButton;

    private Menus currentMenu = Menus.INVENTORY;

    void Start()
    {
        playerStatsUI.SetActive(true);
        menusUI.SetActive(false);
        constructionUI.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            bool openMenus = !menusUI.activeSelf;

            if (openMenus)
            {
                playerStatsUI.SetActive(false);
                Cursor.lockState = CursorLockMode.None;
            }
            else
            {
                playerStatsUI.SetActive(true);
                Cursor.lockState = CursorLockMode.Locked;
            }

            menusUI.SetActive(openMenus);
            UpdateMenuTitleText();
        }

        if (Input.GetKeyUp(KeyCode.C))
        {
            menusUI.SetActive(false);
            constructionUI.SetActive(true);
            Cursor.lockState = CursorLockMode.Locked;
        }

        if (Input.GetKeyUp(KeyCode.Q) || Input.GetKeyUp(KeyCode.LeftArrow))
        {
            MenuLeftButton.onClick.Invoke();
            MenuLeftButton.interactable = false;
            Invoke("EnableQButtonInteraction", 0.5f);
        }

        if (Input.GetKeyUp(KeyCode.E) || Input.GetKeyUp(KeyCode.RightArrow))
        {
            MenuRightButton.onClick.Invoke();
            MenuRightButton.interactable = false;
            Invoke("EnableEButtonInteraction", 0.5f);
        }
    }

    void EnableQButtonInteraction()
    {
        MenuLeftButton.interactable = true;
    }
    void EnableEButtonInteraction()
    {
        MenuRightButton.interactable = true;
    }

    public void HandleMenuGoLeft()
    {
        Menus[] menuValues = (Menus[])Enum.GetValues(typeof(Menus));

        // Find the current index of the current menu
        int currentIndex = Array.IndexOf(menuValues, currentMenu);

        // Calculate the index of the previous menu
        int previousIndex = (currentIndex - 1 + menuValues.Length) % menuValues.Length;

        // Set the current menu to the previous one
        currentMenu = menuValues[previousIndex];

        print(currentMenu);
        UpdateMenuTitleText();
    }

    public void HandleMenuGoRight()
    {
        Array menuValues = Enum.GetValues(typeof(Menus));

        // Find the current index of the current menu
        int currentIndex = Array.IndexOf(menuValues, currentMenu);

        // Calculate the index of the next menu
        int nextIndex = (currentIndex + 1) % menuValues.Length;

        // Set the current menu to the next one
        currentMenu = (Menus)menuValues.GetValue(nextIndex);

        print(currentMenu);
        UpdateMenuTitleText();
    }

    private void UpdateMenuTitleText()
    {
        switch (currentMenu)
        {
            case Menus.SKILLS:
                constructionUI.SetActive(false);
                MenuTitle.text = "Skills";
                SkillsMenu.SetActive(true);
                InventoryMenu.SetActive(false);
                CraftingMenu.SetActive(false);
                break;
            case Menus.INVENTORY:
                constructionUI.SetActive(false);
                MenuTitle.text = "Inventory";
                SkillsMenu.SetActive(false);
                InventoryMenu.SetActive(true);
                CraftingMenu.SetActive(false);
                break;
            case Menus.CRAFTING:
                constructionUI.SetActive(false);
                MenuTitle.text = "Crafting";
                SkillsMenu.SetActive(false);
                InventoryMenu.SetActive(false);
                CraftingMenu.SetActive(true);
                break;
            default:
                MenuTitle.text = "Inventory";
                SkillsMenu.SetActive(false);
                InventoryMenu.SetActive(true);
                CraftingMenu.SetActive(true);
                break;
        }
    }
}
