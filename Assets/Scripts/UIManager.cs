using Unity.VisualScripting;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.UIElements;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance
    {
        get; private set;
    }

    private InputActions inputActions;
    public UIDocument HUDUI;
    public UIDocument MenuTabsUI;
    public UIDocument InventoryTabUI;
    public UIDocument CraftingTabUI;
    public UIDocument SkillsTabUI;
    public UIDocument SettingsTabUI;

    // Tables
    public UIDocument CraftingTableUI;

    // Buttons
    private Button inventoryMenuButton;
    private Button craftingMenuButton;

    // FUNCTIONS

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.Log("There is more than one UIManager instance");
        }
        Instance = this;
    }

    void Start()
    {
        UnblockPlayerCamera();
    }

    private void OnEnable()
    {
        inputActions = new InputActions();
        inputActions.Enable();
    }

    private void OnDisable()
    {
        inputActions.Disable();
    }

    public void OnValidate()
    {
        if (MenuTabsUI != null)
        {
            MenuTabsUI.enabled = false;
        }
        if (HUDUI != null)
        {
            HUDUI.enabled = true;
        }
        if (InventoryTabUI != null)
        {
            InventoryTabUI.enabled = false;
        }
        if (CraftingTabUI != null)
        {
            CraftingTabUI.enabled = false;
        }
        if (CraftingTableUI != null)
        {
            CraftingTableUI.enabled = false;
        }

        UnblockPlayerCamera();
    }

    void Update()
    {
        if (inputActions.UI.Close.triggered)
        {
            UnityEngine.Cursor.lockState = UnityEngine.Cursor.lockState == CursorLockMode.Locked ? CursorLockMode.None : CursorLockMode.Locked;
            UnityEngine.Cursor.visible = UnityEngine.Cursor.lockState != CursorLockMode.Locked;
        }

        if (inputActions.UI.InventoryToggle.triggered)
        {
            // CLOSING MENU
            if (InventoryTabUI.enabled || CraftingTabUI.enabled)
            {
                CloseUI();
            }
            else // OPEN MENU
            {
                OpenUI();
            }
        }
    }

    void OpenInventoryUI(ClickEvent e = null)
    {
        // close ithers UI
        CloseCraftingUI();

        // Focus button
        inventoryMenuButton.Focus();

        InventoryTabUI.enabled = true;
        Inventory.Instance.UpdateInventoryUI();
    }

    void CloseInventoryUI()
    {
        InventoryTabUI.enabled = false;
    }

    void OpenCraftingUI(ClickEvent e = null)
    {
        CloseInventoryUI();
        craftingMenuButton.Focus();
        CraftingTabUI.enabled = true;
    }

    void CloseCraftingUI()
    {
        CraftingTabUI.enabled = false;
    }

    void OpenUI()
    {
        OpenMenuTabs();
        BlockPlayerCamera();
        // Buttons Actions
        VisualElement root = MenuTabsUI.rootVisualElement;

        inventoryMenuButton = root.Query<Button>(className: "button-inventory");

        if (inventoryMenuButton == null)
        {
            Debug.Log("inventoryMenuButton Not Found");
        }
        craftingMenuButton = root.Query<Button>(className: "button-crafting");
        if (craftingMenuButton == null)
        {
            Debug.Log("craftingMenuButton Not Found");
        }

        inventoryMenuButton.RegisterCallback<ClickEvent>(OpenInventoryUI);
        craftingMenuButton.RegisterCallback<ClickEvent>(OpenCraftingUI);

        // DEFAULT UI
        OpenInventoryUI();
    }

    void OpenMenuTabs()
    {
        MenuTabsUI.enabled = true;
    }

    void CloseMenuTabs()
    {
        MenuTabsUI.enabled = false;
    }

    void CloseUI()
    {
        CloseInventoryUI();
        CloseCraftingUI();
        CloseMenuTabs();
        UnblockPlayerCamera();

        inventoryMenuButton.UnregisterCallback<ClickEvent>(OpenInventoryUI);
        craftingMenuButton.UnregisterCallback<ClickEvent>(OpenCraftingUI);
    }

    public void ToggleInteractableUI(bool status, string text = null, Color? bgColor = null)
    {
        VisualElement root = HUDUI.rootVisualElement;
        if (root == null)
        {
            Debug.Log("Aim dot root null");
            return;
        }
        VisualElement interactableContainer = root.Query<VisualElement>(className: "interactable-container");
        interactableContainer.style.visibility = status ? Visibility.Visible : Visibility.Hidden;
        interactableContainer.style.backgroundColor = bgColor ?? Color.green;

        if (status)
        {
            Label textLabel = root.Query<Label>(className: "text");
            textLabel.text = text;
        }
    }

    public void ToggleCraftingTableUI(bool status)
    {
        CraftingTableUI.enabled = status;
    }

    void UnblockPlayerCamera()
    {
        UnityEngine.Cursor.lockState = CursorLockMode.Locked;
        UnityEngine.Cursor.visible = false;

        VisualElement root = HUDUI.rootVisualElement;
        if (root == null)
        {
            Debug.Log("Aim dot root null");
            return;
        }
        VisualElement aimDot = root.Query<VisualElement>(className: "aim-dot");
        aimDot.style.visibility = Visibility.Visible;

        root.style.backgroundColor = Color.clear;

        VisualElement interactableContainer = root.Query<VisualElement>(className: "interactable-container");
        interactableContainer.style.visibility = Visibility.Hidden;
    }

    void BlockPlayerCamera()
    {
        UnityEngine.Cursor.lockState = CursorLockMode.None;
        UnityEngine.Cursor.visible = true;

        VisualElement root = HUDUI.rootVisualElement;
        VisualElement aimDot = root.Query<VisualElement>(className: "aim-dot");
        aimDot.style.visibility = Visibility.Hidden;


        Color bgColor = Color.white;
        bgColor.a = 0.15f;

        root.style.backgroundColor = bgColor;
    }
}
