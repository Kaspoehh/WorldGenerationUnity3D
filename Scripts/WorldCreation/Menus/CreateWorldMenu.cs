using ScriptableObjectArchitecture;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CreateWorldMenu : MonoBehaviour
{
    [Header("Values")]
    [SerializeField] private IntReference renderDistance = default(IntReference);
    [SerializeField] private Vector2Reference seed = default(Vector2Reference);

    [Header("UI")]
    [SerializeField] private TMP_InputField renderDistanceText; 
    [SerializeField] private TMP_InputField seedText;
    
    public void CreateWorld()
    {
        int rendDistance = 5;
        
        int.TryParse(renderDistanceText.text, out rendDistance);
        Debug.Log(rendDistance);
        
        if (rendDistance == 0)
            rendDistance = 3;
        
        renderDistance.Value = rendDistance;

        float x = 0;
        float y = 0;
        
        float.TryParse(seedText.text, out x);
        float.TryParse(seedText.text, out y);
        
        seed.Value = new Vector2(x, y);
        
        OpenNewWorld();
    }

    private void OpenNewWorld()
    {
        SceneManager.LoadScene("NewWorld", LoadSceneMode.Single);
    }
}
