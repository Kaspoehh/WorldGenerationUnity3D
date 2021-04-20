using UnityEngine;
using UnityEngine.SceneManagement;

public class StartMenu : MonoBehaviour
{
    public void OpenCreateNewWorldScene()
    {
        SceneManager.LoadScene("CreateNewWorldMenu", LoadSceneMode.Single);
    }
    
    public void OpenExistingWorld()
    {
        SceneManager.LoadScene("LoadWorld", LoadSceneMode.Single);
    }

    public void Quit()
    {
        Application.Quit();
    }
}
