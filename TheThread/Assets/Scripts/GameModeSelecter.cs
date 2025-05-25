using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameModeSelecter : MonoBehaviour{
    public void LoadSceneByName(){
        SceneManager.LoadScene("Scenes/MainMenu");
    }

    public void LoadNextInBuild(){
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}
