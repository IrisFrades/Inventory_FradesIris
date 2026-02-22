using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeCanvas : MonoBehaviour
{
    [SerializeField] GameObject[] canvas;

    public void ActiveCanvas(GameObject canvas)
    {
        canvas.SetActive(true);     
    }

    public void DesactiveCanvas(GameObject canvas)
    {
        canvas.SetActive(false);
    }

    public void QuitApplication()
    {
        Application.Quit();
    }

    public void ChangeToRegisterScene()
    {
        SceneManager.LoadScene("SampleScene");
    }
}
