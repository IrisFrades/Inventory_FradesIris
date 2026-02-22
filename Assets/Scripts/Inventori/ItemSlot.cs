using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemSlot : MonoBehaviour, IPointerClickHandler
{

    //ITEM DATA//
    public string itemName;
    public int quantity;
    public Sprite itemSprite;
    public bool isFull;
    public string itemDescription;
    public Sprite emptySprite;

    [SerializeField]
    private int maxNumberOfItems;

    [HideInInspector] public int slotIndex;

    //ITEM SLOT//
    [SerializeField]
    private TextMeshProUGUI quantityText;

    [SerializeField]
    private Image itemImage;

    //ITEM DESCRIPTION SLOT//
    public Image itemDescriptionImage;
    public TextMeshProUGUI ItemDescriptionNameText;
    public TextMeshProUGUI ItemDescriptionText;


    public GameObject selectedShader;
    public bool thisItemSelected;

    private InventoryManager inventoryManager;

    private void Start()
    {
        inventoryManager = InventoryManager.Instance;  // ← Singleton, no Find
    }


    //Añade item
    public int AddItem(string ItemName, int quantity, Sprite itemSprite, string itemDescription)
    {
        if (itemImage == null || quantityText == null)
        {
            return quantity;
        }
        //Ver si el slot esta lleno

        if (isFull)
        {
            return quantity;
        }

        //Actualizar NAME
        this.itemName = ItemName;

        //Actualizar Image
        this.itemSprite = itemSprite;
        itemImage.sprite = itemSprite;

        //Actualizar Description
        this.itemDescription = itemDescription;

        //Actualizar QUANTITY
        this.quantity += quantity;

        if (this.quantity >= maxNumberOfItems)
        {
            quantityText.text = maxNumberOfItems.ToString();
            quantityText.enabled = true;
            isFull = true;


            //Return leftovers
            int extraItems = this.quantity - maxNumberOfItems;
            this.quantity = maxNumberOfItems;
            return extraItems;
        }

        //Actualizar QUANTITY TEXT
        quantityText.text = this.quantity.ToString();
        quantityText.enabled = true;
        isFull = false;
        return 0;
    }

    //Cuando le da click a un slot
    public void OnPointerClick(PointerEventData eventData)
    {
        if(eventData.button == PointerEventData.InputButton.Left)
        {
            OnLeftClick();
        }
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            OnRightClick();
        }
    }

    //Al darle al boton izquierdo encima del slot
    private void OnLeftClick()
    {
        inventoryManager.DeselectAllSlots(); //Deseleciona todos los slots
        inventoryManager.selectedSlot = this; 

        selectedShader.SetActive(true); //Activa el shader como slot seleccionado
        thisItemSelected = true; //Marca como true que ese slot o item estan seleccionados

        ItemDescriptionNameText.text = itemName; //te muestra el nombre del item
        ItemDescriptionText.text = itemDescription; //te muestra la descripcion del itme
        itemDescriptionImage.sprite = itemSprite; //te muestra el sprite del item

    }

    //Al darle al boton derecho encima del slot seleccionado
    private void OnRightClick()
    {
        inventoryManager.DeselectAllSlots(); //los deselecciona todos
    }

    //Al darle al boton de eliminar item completamente
    public void DeleteAllItems()
    {
        quantity = 0;
        itemName = "";
        itemSprite = emptySprite;
        itemImage.sprite = emptySprite;
        quantityText.text = "";
        quantityText.enabled = false;
        isFull = false;
    }

    //Al darle al boton de añadir solo un item del stack de items
    public void AddOneItem()
    {
        if (!isFull && quantity > 0) // Solo si ya tiene items
        {
            inventoryManager.AddItem(itemName, 1, itemSprite, itemDescription);
        }
    }

    //Al darle al boton de eliminar solo un item del stack de items
    public void RemoveOneItem()
    {
        if (quantity > 1)
        {
            quantity--;
            quantityText.text = quantity.ToString();
            isFull = false; // Ya no está lleno
        }
        else if (quantity == 1)
        {
            DeleteAllItems(); // Si era el último, vacía slot
        }
    }

    private void OnApplicationFocus(bool focus)
    {
        if (focus && inventoryManager != null)
        {
            inventoryManager.SaveInventoryToDB();
        }
    }

    private void OnApplicationPause(bool pause)
    {
        if (pause && inventoryManager != null)
        {
            inventoryManager.SaveInventoryToDB();
        }
    }

    // En ItemSlot.cs
    public void ClearSlot()
    {
        quantity = 0;
        itemName = "";
        itemImage.sprite = emptySprite;
        quantityText.text = "0";
    }










}
