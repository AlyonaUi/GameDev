using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    public void startGame()
    {
        SceneManager.LoadScene("Game");
    }
    
    public void options() 
    {
        SceneManager.LoadScene("Settings");
    }
    
    public void exitGame() 
    {
        Application.Quit();
        Debug.Log("Мы вышли!");
    }
}