using UnityEngine;
using UnityEngine.SceneManagement;

public class OpenSceneSteal : MonoBehaviour
{
    public void OpenStealScene()
    {
        SceneManager.LoadScene("StealGame");
    }
}