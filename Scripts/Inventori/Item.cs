using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    //INFORMACION DEL ITEM//
    [SerializeField]
    private string itemName;
    [SerializeField]
    private int quantity;
    [SerializeField]
    private Sprite sprite;

    [TextArea]
    [SerializeField]
    private string itemDescription;

    private InventoryManager inventoryManager;

    private void Start()
    {
        inventoryManager = GameObject.Find("InventoryCanvas").GetComponent<InventoryManager>(); //Coge el componente "InventoryManager" que tiene el canvas en el inspector
    }

    //Cuando el objeto colosiona con algo
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag==("Player")) //Si ese algo tiene el tag "Player"
        {
            int leftOverItems = inventoryManager.AddItem(itemName, quantity, sprite, itemDescription); //Llama a la funcion AddItem
            if(leftOverItems <= 0)
            {
                Destroy(gameObject); //Destruyeme el gameObject
            }
            else
            {
                quantity = leftOverItems; //Guardame en la cantidad los items
            }
                
        }
    }
}
