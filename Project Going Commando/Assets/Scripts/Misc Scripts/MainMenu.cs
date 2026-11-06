using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{

    [Header("Scene Settings")]
    [SerializeField] private string sceneName;

    public void StartGame()
    { SceneManager.LoadScene(sceneName); }

    public void ExitGame()
    { Application.Quit(); }

    public void OpenSocial(string url)
    { Application.OpenURL(url); }

}
