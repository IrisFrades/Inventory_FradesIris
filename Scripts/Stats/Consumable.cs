using UnityEngine;

public class Consumable : MonoBehaviour
{
    [Header("Consumable Data")]
    [SerializeField] private string consumableName = "Poción Fuerza";
    [SerializeField] private string effectType = "strength";  // "strength" o "level"
    [SerializeField] private int bonusAmount = 1;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // AÑADIR A DB DIRECTAMENTE como consumible
            int itemID = GetItemID(consumableName);
            DBManager.Instance.AddItemToInventory(
                InventoryManager.Instance.currentUserID,
                itemID, 1, GetFreeSlot()
            );

            Debug.Log($"{consumableName} recogido!");
            Destroy(gameObject);
        }
    }

    private int GetItemID(string name)
    {
        return name switch
        {
            "Poción Fuerza" => 3,
            "Poción Nivel" => 4,
            _ => 1
        };
    }

    private int GetFreeSlot()
    {
        // Lógica para encontrar slot vacío
        return 0; // Simplificado
    }
}
