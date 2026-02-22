using Mono.Data.Sqlite;
using System.Collections.Generic;
using System.Data;
using System.IO;
using UnityEngine;

public class DBManager : MonoBehaviour
{
    public static DBManager Instance { get; private set; }

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
        InitializeDatabase();
        PopulateItemsTable();
    }

    // Llegir tots els valors de la taula
    public int Login(string user, string password)
    {
        int idUser = -1; //si no existe en la base de datos 

        // Creació/Obertura de la connexió amb la base de dades sobre un fitxer a Assets/MyDatabase.sqlite
        string dbUri = GetDatabaseURI();

        // Creació de la connexió
        IDbConnection dbConnection = new SqliteConnection(dbUri); // 5

        // Obertura de la connexió
        dbConnection.Open(); // 6

        // Creació del comandament de lectura
        IDbCommand dbCommandReadValues = dbConnection.CreateCommand(); // 15
        dbCommandReadValues.CommandText = "SELECT * FROM USERS"; // 16

        // Executem el query i rebem a un IDataReader el resultat
        IDataReader dataReader = dbCommandReadValues.ExecuteReader();

        // Mentre hi hagin linies per llegir
        while (dataReader.Read())
        {
            // Llegim l'entrada y la mostrem per consola
            // Llegim específicament la segona columna com a Integer32
            if (user == dataReader.GetString(1) && password == dataReader.GetString(2))
            {
                idUser = dataReader.GetInt32(0);
            }

        }

        dataReader.Dispose();

        // Recordeu tancar la connexió sempre
        dbConnection.Close();

        return idUser;
    }

    string GetDatabasePath()
    {
        return Application.streamingAssetsPath + "/users.sqlite";
    }

    string GetDatabaseURI()
    {
        return "URI=file:" + GetDatabasePath();
    }

    public int Register(string user, string password)
    {
        int idUser = -1; //en caso q exista ese mismo usuario que se esta creando en una base de datos se devuelve error (de que ya existe)

        // Creació/Obertura de la connexió amb la base de dades sobre un fitxer a Assets/MyDatabase.sqlite
        //string dbUri = "URI=file:" + Application.dataPath + "../StreamingAssets/users.sqlite"; // 4

        if (!File.Exists(GetDatabasePath()))
        {
            Debug.LogWarning("No Database file found at path " + GetDatabasePath());
        }

        string dbUri = GetDatabaseURI();

        // Creació de la connexió
        IDbConnection dbConnection = new SqliteConnection(dbUri); // 5

        // Obertura de la connexió
        dbConnection.Open(); // 6

        // Creació del comandament de lectura
        IDbCommand dbCommandReadValues = dbConnection.CreateCommand(); // 15
        dbCommandReadValues.CommandText = "SELECT * FROM USERS"; // 16

        // Executem el query i rebem a un IDataReader el resultat
        IDataReader dataReader = dbCommandReadValues.ExecuteReader();

        // Mentre hi hagin linies per llegir
        while (dataReader.Read())
        {
            // Llegim l'entrada y la mostrem per consola
            // Llegim específicament la segona columna com a Integer32
            if (user == dataReader.GetString(1))
            {
                idUser = dataReader.GetInt32(0); //devuelve el id del usuario que ya esta creado
            }

        }

        if (idUser != -1)
        {
            return -1;
        }

        dbCommandReadValues = dbConnection.CreateCommand(); // 15
        dbCommandReadValues.CommandText = $"INSERT INTO USERS (username, password) VALUES ('{user}','{password}')"; // 16

        dataReader = dbCommandReadValues.ExecuteReader();

        dataReader.Dispose();

        // Recordeu tancar la connexió sempre
        dbConnection.Close();

        return Login(user, password);
    }

    //Metode per inicialitzar la base de dades
    private void InitializeDatabase()
    {
        string dbUri = GetDatabaseURI();
        using (IDbConnection dbConnection = new SqliteConnection(dbUri))
        {
            dbConnection.Open();

            // Creem totes les taules
            string[] createTables = {
                @"CREATE TABLE IF NOT EXISTS USERS (
                    userID INTEGER PRIMARY KEY AUTOINCREMENT,
                    Username TEXT NOT NULL UNIQUE,
                    Password TEXT NOT NULL)",

                @"CREATE TABLE IF NOT EXISTS ITEMS (
                    itemID INTEGER PRIMARY KEY AUTOINCREMENT,
                    name TEXT NOT NULL,
                    description TEXT,
                    item_type TEXT NOT NULL,
                    max_stack_size INTEGER DEFAULT 5)",

                @"CREATE TABLE IF NOT EXISTS USER_INVENTORY (
                    inventoryID INTEGER PRIMARY KEY AUTOINCREMENT,
                    userID INTEGER NOT NULL,
                    itemID INTEGER NOT NULL,
                    slot_index INTEGER NOT NULL,
                    quantity INTEGER NOT NULL DEFAULT 1,
                    FOREIGN KEY (userID) REFERENCES USERS(userID),
                    FOREIGN KEY (itemID) REFERENCES ITEMS(itemID),
                    UNIQUE(userID, slot_index))",

                @"CREATE TABLE IF NOT EXISTS STATS (
                    statID INTEGER PRIMARY KEY AUTOINCREMENT,
                    userID INTEGER NOT NULL,
                    level INTEGER DEFAULT 1,
                    strength INTEGER DEFAULT 0,
                    FOREIGN KEY (userID) REFERENCES USERS(userID),
                    UNIQUE(userID))"
            };

            using (IDbCommand cmd = dbConnection.CreateCommand())
            {
                foreach (string sql in createTables)
                {
                    cmd.CommandText = sql;
                    cmd.ExecuteNonQuery();
                }
            }
        }
        Debug.Log("DB inicializada");
    }

    //Metodo para cargar el inventario
    public List<InventoryItem> LoadInventory(int userID)
    {
        List<InventoryItem> inventory = new List<InventoryItem>();
        string dbUri = GetDatabaseURI();

        using (IDbConnection dbConnection = new SqliteConnection(dbUri))
        {
            dbConnection.Open();
            using (IDbCommand cmd = dbConnection.CreateCommand())
            {
                cmd.CommandText = @"
                    SELECT ui.slot_index, ui.quantity, i.name, i.item_type, i.max_stack_size 
                    FROM USER_INVENTORY ui 
                    JOIN ITEMS i ON ui.itemID = i.itemID 
                    WHERE ui.userID = @userID 
                    ORDER BY ui.slot_index";
                cmd.Parameters.Add(new SqliteParameter("@userID", userID));

                using (IDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        inventory.Add(new InventoryItem
                        {
                            slotIndex = reader.GetInt32(0),
                            quantity = reader.GetInt32(1),
                            itemName = reader.GetString(2),
                            itemType = reader.GetString(3),
                            maxStack = reader.GetInt32(4)
                        });
                    }
                }
            }
        }
        return inventory;
    }

    //Metode per afegir item al inventari
    public bool AddItemToInventory(int userID, int itemID, int quantity, int slotIndex)
    {
        string dbUri = GetDatabaseURI();
        using (IDbConnection dbConnection = new SqliteConnection(dbUri))
        {
            dbConnection.Open();
            using (IDbCommand cmd = dbConnection.CreateCommand())
            {
                //Añadir los parametros a la base de datos (los atributos que tiene la tabla)
                cmd.CommandText = @"
                    INSERT OR REPLACE INTO USER_INVENTORY (userID, itemID, slot_index, quantity)
                    VALUES (@userID, @itemID, @slot, @quantity)";
                cmd.Parameters.Add(new SqliteParameter("@userID", userID));
                cmd.Parameters.Add(new SqliteParameter("@itemID", itemID));
                cmd.Parameters.Add(new SqliteParameter("@slot", slotIndex));
                cmd.Parameters.Add(new SqliteParameter("@quantity", quantity));
                return cmd.ExecuteNonQuery() > 0;
            }
        }
    }

    //Metode per eliminar un item del inventari
    public void RemoveItemFromInventory(int userID, int slotIndex)
    {
        string dbUri = GetDatabaseURI();
        using (IDbConnection dbConnection = new SqliteConnection(dbUri))
        {
            dbConnection.Open();
            using (IDbCommand cmd = dbConnection.CreateCommand())
            {
                cmd.CommandText = "DELETE FROM USER_INVENTORY WHERE userID = @userID AND slot_index = @slot";
                cmd.Parameters.Add(new SqliteParameter("@userID", userID));
                cmd.Parameters.Add(new SqliteParameter("@slot", slotIndex));
                cmd.ExecuteNonQuery();
            }
        }
    }

    // Metode per afegir items a la taula de items
    private void PopulateItemsTable()
    {
        string dbUri = GetDatabaseURI();
        using (IDbConnection dbConnection = new SqliteConnection(dbUri))
        {
            dbConnection.Open();
            using (IDbCommand cmd = dbConnection.CreateCommand())
            {
                //Inserim els items que ja tenim fets predeterminadament
                cmd.CommandText = @"
                INSERT OR IGNORE INTO ITEMS (itemID, name, description, item_type, max_stack_size) 
                VALUES 
                (1, 'Huevos', 'Comida básica', 'Comida', 5),
                (2, 'Miel', 'Energía extra', 'Comida', 5),
                (3, 'Poción Fuerza', '+1 fuerza', 'Poción', 3),
                (4, 'Poción Nivel', '+1 nivel', 'Poción', 3)";


                cmd.ExecuteNonQuery();
            }
        }
        Debug.Log("Items pobrados: Huevos(1), Miel(2)");
    }

    //Metode por consumir l'objecte consumible
    public bool UseConsumable(int userID, int slotIndex)
    {
        string dbUri = GetDatabaseURI();
        using (IDbConnection dbConnection = new SqliteConnection(dbUri))
        {
            dbConnection.Open();

            // Obtenim el nom del item
            string itemName = "";
            using (IDbCommand cmdGetName = dbConnection.CreateCommand())
            {
                cmdGetName.CommandText = @"
                SELECT i.name FROM USER_INVENTORY ui 
                JOIN ITEMS i ON ui.itemID = i.itemID 
                WHERE ui.userID = @userID AND ui.slot_index = @slot";
                cmdGetName.Parameters.Add(new SqliteParameter("@userID", userID));
                cmdGetName.Parameters.Add(new SqliteParameter("@slot", slotIndex));

                using (IDataReader reader = cmdGetName.ExecuteReader())
                {
                    if (!reader.Read()) return false;
                    itemName = reader.GetString(0);
                }
            }

            int bonus = 0;
            string effect = "";
            switch (itemName)
            {
                case "Poción Fuerza": effect = "strength"; bonus = 1; break;
                case "Poción Nivel": effect = "level"; bonus = 1; break;
                case "Huevos": effect = "strength"; bonus = 1; break;
                case "Miel": effect = "strength"; bonus = 2; break;
                default: return false; // No consumible
            }

            // REstem la quantitat i apliquem l'efecte
            using (IDbCommand cmd = dbConnection.CreateCommand())
            {
                // Aqui restem la quantitat
                cmd.CommandText = @"
                UPDATE USER_INVENTORY SET quantity = quantity - 1 
                WHERE userID = @userID AND slot_index = @slot AND quantity > 1;
                DELETE FROM USER_INVENTORY WHERE userID = @userID AND slot_index = @slot AND quantity <= 1";
                cmd.Parameters.Add(new SqliteParameter("@userID", userID));
                cmd.Parameters.Add(new SqliteParameter("@slot", slotIndex));
                cmd.ExecuteNonQuery();

                // Aqui apliquem l'efecte
                cmd.Parameters.Clear();
                if (effect == "strength")
                    cmd.CommandText = "UPDATE STATS SET strength = strength + @bonus WHERE userID = @userID";
                else
                    cmd.CommandText = "UPDATE STATS SET level = level + @bonus WHERE userID = @userID";

                cmd.Parameters.Add(new SqliteParameter("@userID", userID));
                cmd.Parameters.Add(new SqliteParameter("@bonus", bonus));
                cmd.ExecuteNonQuery();
            }
            return true;
        }
    }


    public (int strength, int level) LoadUserStats(int userID)  // Canviar el nom
    {
        string dbUri = GetDatabaseURI();
        using (IDbConnection dbConnection = new SqliteConnection(dbUri))
        {
            dbConnection.Open();
            using (IDbCommand cmd = dbConnection.CreateCommand())
            {
                cmd.CommandText = "SELECT strength, level FROM STATS WHERE userID = @userID";  // 
                cmd.Parameters.Add(new SqliteParameter("@userID", userID));

                using (IDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                        return (reader.GetInt32(0), reader.GetInt32(1));
                }
            }
        }
        return (0, 1);
    }

    //Métode per afegir la força
    public bool AddStrength(int userID, int bonus = 1)
    {
        string dbUri = GetDatabaseURI();
        using (var conn = new SqliteConnection(dbUri))
        {
            conn.Open();
            using (var cmd = conn.CreateCommand())
            {
                // Crear stats si no existe
                cmd.CommandText = @"
                INSERT OR IGNORE INTO STATS (statID, userID, level, strength) 
                VALUES (NULL, @userID, 1, 0)";
                cmd.Parameters.Add(new SqliteParameter("@userID", userID));
                cmd.ExecuteNonQuery();

                // Sumem la quantitat de força
                cmd.Parameters.Clear();
                cmd.CommandText = "UPDATE STATS SET strength = strength + @bonus WHERE userID = @userID";
                cmd.Parameters.Add(new SqliteParameter("@userID", userID));
                cmd.Parameters.Add(new SqliteParameter("@bonus", bonus));
                return cmd.ExecuteNonQuery() > 0;
            }
        }
    }




}

