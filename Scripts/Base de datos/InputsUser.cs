using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class LoginPanelController : MonoBehaviour
{
    [SerializeField] private TMP_InputField inputUsuario;
    [SerializeField] private TMP_InputField inputPassword;
    [SerializeField] private DBManager dBManager;

    private void Start()
    {
    }

    //Al darle al boton de Iniciar sesion
    public void OnLoginClick()
    {
        //Inputs de usuario y contraseña
        string user = inputUsuario.text;
        string pass = inputPassword.text;

        if (!string.IsNullOrEmpty(user) && !string.IsNullOrEmpty(pass)) //Comprueba que haya algo escrito en los inputs
        {
            int idUser = dBManager.Login(user, pass);
            if (idUser >= 0)
            {
                Debug.Log("Login OK, ID: " + idUser);

                

                if (InventoryManager.Instance != null)
                {
                    InventoryManager.Instance.SetCurrentUser(idUser);
                }

                Stats statsUI = FindObjectOfType<Stats>();
                if (statsUI != null)
                {
                    statsUI.SetUserID(idUser);  // ← MUESTRA NIVEL/FUERZA
                }

                SceneManager.LoadScene("Juego");
            }
            else
            {
                Debug.LogWarning("Login falló: usuario/contraseña incorrectos");
            }
        }
        else
        {
            Debug.LogWarning("¡Faltan datos!");
        }
    }


    //Al darle al boton de registrarse
    public void OnRegisterClick()
    {
        //Inputs del usuario
        string user = inputUsuario.text;
        string pass = inputPassword.text;

        if (!string.IsNullOrEmpty(user) && !string.IsNullOrEmpty(pass)) //Comprueba que haya algo escrito en los inputs
        {
            int newUserID = dBManager.Register(user, pass);
            if (newUserID >= 0)
            {
                Debug.Log("Te has registrado correctamente! ID: " + newUserID);

                if (InventoryManager.Instance != null)
                {
                    InventoryManager.Instance.SetCurrentUser(newUserID);
                }
            }
            else
            {
                Debug.LogWarning("El usuario que estas intentando crear ya existe");
            }
        }
        else
        {
            Debug.LogWarning("¡Faltan datos!");
        }
    }
}
