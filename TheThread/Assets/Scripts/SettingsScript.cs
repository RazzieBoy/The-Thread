using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SettingsScript : MonoBehaviour{
    public GameObject mainMenu;
    public GameObject settingsMenu;
    
    public void BackButton(){
        mainMenu.SetActive(true);
        settingsMenu.SetActive(false);
    }
}
