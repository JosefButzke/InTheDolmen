using Unity.VisualScripting;
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
    public UIDocument InventoryUI;
    public UIDocument CraftingUI;
    public UIDocument SkillsUI;
    public UIDocument SettingsUI;

    private Button inventoryMenuButton;
    private Button craftingMenuButton;
    private Button categoryToolsButton;
    private Button categorymachinaryButton;
    private Button categoryMachinaryButton;
    private Button categoryBackpackButton;
    private Button categoryGunsButton;

    public VisualTreeAsset slotTemplate;

    public CraftingItem[] craftingItems;

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
        UnityEngine.Cursor.lockState = CursorLockMode.Locked;
        UnityEngine.Cursor.visible = false;
    }

    public void OnEnable()
    {
        inputActions = new InputActions();
        inputActions.UI.Enable();

        if (MenuTabsUI != null)
        {
            MenuTabsUI.enabled = false;
        }
        if (HUDUI != null)
        {
            HUDUI.enabled = true;
        }
        if (InventoryUI != null)
        {
            InventoryUI.enabled = false;
        }
        if (CraftingUI != null)
        {
            CraftingUI.enabled = false;
        }
    }

    private void OnDisable()
    {
        inputActions.UI.Disable();
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
            if (InventoryUI.enabled || CraftingUI.enabled)
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

        InventoryUI.enabled = true;
        Inventory.Instance.UpdateUI();
        BlockPlayerCamera();
    }

    void CloseInventoryUI()
    {
        InventoryUI.enabled = false;
    }

    void OpenCraftingUI(ClickEvent e = null)
    {
        CloseInventoryUI();
        craftingMenuButton.Focus();
        CraftingUI.enabled = true;

        VisualElement root = CraftingUI.rootVisualElement;
        VisualElement categoriesContainer = root.Query<VisualElement>(className: "categories-container");
        categoryToolsButton = root.Query<Button>(name: "CategoryTools");
        categoryMachinaryButton = root.Query<Button>(name: "CategoryMachinary");
        categoryBackpackButton = root.Query<Button>(name: "CategoryBackpack");
        categoryGunsButton = root.Query<Button>(name: "CategoryGuns");

        categoryToolsButton.RegisterCallback<ClickEvent>(OnPressCategoryTools);
        categoryMachinaryButton.RegisterCallback<ClickEvent>(OnPressCategoryMachinary);
        categoryBackpackButton.RegisterCallback<ClickEvent>(OnPressCategoryBackpack);
        categoryGunsButton.RegisterCallback<ClickEvent>(OnPressCategoryGuns);

        UpdateSlots(CraftingCategory.Tools);
        BlockPlayerCamera();
    }

    void CloseCraftingUI()
    {
        CraftingUI.enabled = false;


        categoryToolsButton?.UnregisterCallback<ClickEvent>(OnPressCategoryTools);
        categoryMachinaryButton?.UnregisterCallback<ClickEvent>(OnPressCategoryMachinary);
        categoryBackpackButton?.UnregisterCallback<ClickEvent>(OnPressCategoryBackpack);
        categoryGunsButton?.UnregisterCallback<ClickEvent>(OnPressCategoryGuns);

    }

    void OnPressCategoryTools(ClickEvent e = null)
    {
        UpdateSlots(CraftingCategory.Tools);
    }
    void OnPressCategoryMachinary(ClickEvent e = null)
    {
        UpdateSlots(CraftingCategory.Machinery);
    }
    void OnPressCategoryBackpack(ClickEvent e = null)
    {
        UpdateSlots(CraftingCategory.Backpack);
    }
    void OnPressCategoryGuns(ClickEvent e = null)
    {
        UpdateSlots(CraftingCategory.Guns);
    }

    void UpdateSlots(CraftingCategory category)
    {
        VisualElement root = CraftingUI.rootVisualElement;
        VisualElement categoryItemsContainer = root.Query<VisualElement>(className: "category-items-container");

        // CLEAN CONTAINER
        categoryItemsContainer.Clear();

        for (int i = 0; i < craftingItems.Length; i++)
        {
            if (craftingItems[i].category == category)
            {
                VisualElement slot = slotTemplate.Instantiate();
                VisualElement slotSprite = slot.Query<VisualElement>(className: "item-slot-sprite");
                slotSprite.style.backgroundImage = new StyleBackground(craftingItems[i].icon);
                categoryItemsContainer.Add(slot);
            }
        }
    }

    void OpenUI()
    {
        OpenMenuTabs();
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

    void UnblockPlayerCamera()
    {
        UnityEngine.Cursor.lockState = CursorLockMode.Locked;
        UnityEngine.Cursor.visible = false;

        VisualElement root = HUDUI.rootVisualElement;
        VisualElement aimDot = root.Query<VisualElement>(className: "aim-dot");
        aimDot.style.visibility = Visibility.Visible;
    }

    void BlockPlayerCamera()
    {
        UnityEngine.Cursor.lockState = CursorLockMode.None;
        UnityEngine.Cursor.visible = true;

        VisualElement root = HUDUI.rootVisualElement;
        VisualElement aimDot = root.Query<VisualElement>(className: "aim-dot");
        aimDot.style.visibility = Visibility.Hidden;
    }
}
