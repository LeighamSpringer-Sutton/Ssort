using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    // Start is called before the first frame update

    

    public void LoadNextScene()
    {
        
        var currentSceneIndex = SceneManager.GetActiveScene().buildIndex;

        SceneManager.LoadScene(currentSceneIndex + 1);
       
    }

    public void LoadGameScene()
    {
        //FindObjectOfType<GameSession>().ResetGame();
        SceneManager.LoadScene(1);
    }

    public void QuitGame()
    {
        Application.Quit(); Application.Quit();
    }
}
