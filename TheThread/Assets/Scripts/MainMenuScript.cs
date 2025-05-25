using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuScript : MonoBehaviour{

    public GameObject mainMenu;
    public GameObject gameMode;
    public void PlayButton(){
        mainMenu.SetActive(false);
        gameMode.SetActive(true);
    }

}
