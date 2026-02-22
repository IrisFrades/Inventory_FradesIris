using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    public GameObject InventoryMenu;
    private bool menuActivated;
    public ItemSlot[] itemSlot;

    public static InventoryManager Instance { get; private set; }

    //Botones del inventario
    public Button deleteItem;
    public Button addOneItem;
    public Button removeOneItem;

    public ItemSlot selectedSlot;

    //Bases de datos//
    private DBManager dbManager;
    public int currentUserID = -1;

    private void Awake() //Singleton
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        UpdateDBManager();

        if (itemSlot == null || itemSlot.Length == 0)
        {
            itemSlot = FindObjectsOfType<ItemSlot>();
        }

        // 🔥 ASIGNAR ÍNDICE A CADA SLOT (NUEVO)
        for (int i = 0; i < itemSlot.Length; i++)
        {
            itemSlot[i].slotIndex = i;  // ← Slot 0 = índice 0, Slot 1 = índice 1, etc.
        }

        Debug.Log($"Slots asignados: {itemSlot.Length}");
    }


    void Update()
    {
        if (Input.GetButtonDown("Inventory") && menuActivated)
        {
            Time.timeScale = 1f;
            InventoryMenu.SetActive(false);
            menuActivated = false;
        }
        else if (Input.GetButtonDown("Inventory") && !menuActivated)
        {
            Time.timeScale = 0f;
            InventoryMenu.SetActive(true);
            menuActivated = true;
        }
    }

    public int AddItem(string itemName, int quantity, Sprite itemSprite, string itemDescription)
    {
        int remaining = quantity;

        for (int i = 0; i < itemSlot.Length && remaining > 0; i++)
        {
            if (itemSlot[i].quantity == 0 ||
                (!itemSlot[i].isFull && itemSlot[i].itemName == itemName))
            {
                remaining = itemSlot[i].AddItem(itemName, remaining, itemSprite, itemDescription);
            }
        }

        // 🔥 GUARDAR automáticamente después de añadir
        if (remaining == 0 && currentUserID != -1)
        {
            SaveInventoryToDB();
        }

        return remaining;
    }

    public void DeselectAllSlots()
    {
        selectedSlot = null;  // ← AÑADE ESTO

        // Usar el array ACTUALIZADO de slots
        if (itemSlot != null)
        {
            for (int i = 0; i < itemSlot.Length; i++)
            {
                itemSlot[i].selectedShader.SetActive(false);
                itemSlot[i].thisItemSelected = false;
            }
        }
    }


    public ItemSlot GetSelectedSlot()
    {
        foreach (ItemSlot slot in itemSlot)
        {
            if (slot.thisItemSelected) return slot;
        }
        return null;
    }

    public void lDeleteAll()
    {
        ItemSlot slot = GetSelectedSlot();
        if (slot != null)
        {
            slot.DeleteAllItems();
            SaveInventoryToDB();
        }
    }

    public void AddOne()
    {
        ItemSlot slot = GetSelectedSlot();
        if (slot != null)
        {
            slot.AddOneItem();
            SaveInventoryToDB();
        }
    }

    public void RemoveOne()
    {
        ItemSlot slot = GetSelectedSlot();
        if (slot != null)
        {
            slot.RemoveOneItem();
            SaveInventoryToDB();
        }
    }

    // 🔥 MÉTODO CRÍTICO: Obtener DBManager SEGURO
    // Reemplaza TODO el método GetDBManager() por ESTE:
    private DBManager GetDBManager()
    {
        return DBManager.Instance; // ← Directo, nunca null
    }

    // Reemplaza UpdateDBManager() por:
    private void UpdateDBManager()
    {
        // Ya no necesario - Instance siempre funciona
        Debug.Log("DBManager singleton listo");
    }


    //BASE DE DATOS//
    public void SaveInventoryToDB()
    {
        var db = GetDBManager();
        if (db == null || currentUserID == -1)
        {
            Debug.LogWarning("No se puede guardar: DBManager o userID inválido");
            return;
        }

        for (int i = 0; i < itemSlot.Length; i++)
        {
            if (itemSlot[i].quantity > 0)
            {
                db.AddItemToInventory(currentUserID, GetItemID(itemSlot[i].itemName), itemSlot[i].quantity, i);
            }
            else
            {
                db.RemoveItemFromInventory(currentUserID, i);
            }
        }
        Debug.Log("Inventario guardado para usuario " + currentUserID);
    }

    private void LoadInventoryFromDB()
    {
        var db = GetDBManager();
        if (db == null || currentUserID == -1 || itemSlot == null) return;

        for (int i = 0; i < itemSlot.Length; i++)
            itemSlot[i].ClearSlot();

        var inventory = db.LoadInventory(currentUserID);
        Debug.Log($"DB devolvió {inventory.Count} items");  // ← DEBUG

        foreach (var item in inventory)
        {
            if (item.slotIndex >= 0 && item.slotIndex < itemSlot.Length)
            {
                Sprite sprite = Resources.Load<Sprite>("Items/" + item.itemName);
                Debug.Log($"Cargando {item.itemName} en slot {item.slotIndex}, qty {item.quantity}, sprite: {(sprite != null ? "OK" : "NULL")}");  // ← DEBUG

                itemSlot[item.slotIndex].AddItem(
                    item.itemName,
                    item.quantity,
                    sprite ?? itemSlot[item.slotIndex].emptySprite,
                    "Item description"
                );
            }
        }
    }



    private int GetItemID(string itemName)
    {
        return itemName switch
        {
            "Huevos" => 1,
            "Miel" => 2,
            "Manzana" => 3,
            "Poción Fuerza" => 4, 
            "Poción Nivel" => 5,    
            _ => 1
        };
    }


    public void SetCurrentUser(int idUser)
    {
        currentUserID = idUser;
        RefreshItemSlots();
        LoadInventoryFromDB();
    }


    private void RefreshItemSlots()
    {
        itemSlot = FindObjectsOfType<ItemSlot>();  // ← Encuentra los slots NUEVOS

        for (int i = 0; i < itemSlot.Length; i++)
        {
            itemSlot[i].slotIndex = i;
        }
        Debug.Log($"Slots refrescados: {itemSlot.Length}");
    }


    // 🔥 GUARDAR al salir de la app
    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus && currentUserID != -1)
        {
            SaveInventoryToDB();
        }
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus && currentUserID != -1)
        {
            SaveInventoryToDB();
        }
    }

    public void UseSelectedItem()
    {

        if (currentUserID == -1)
        {
            Debug.Log(currentUserID);
            return;
        }

        bool used = DBManager.Instance.UseConsumable(currentUserID, selectedSlot.slotIndex);

        if (used)
        {
            LoadInventoryFromDB();
            Stats statsUI = FindObjectOfType<Stats>();
            if (statsUI != null) statsUI.UpdateStatsUI();
        }
    }
}




