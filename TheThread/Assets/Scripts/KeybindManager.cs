using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeybindManager : MonoBehaviour{

    public static KeybindManager Instance;
    private Dictionary<string, KeyCode> keybinds = new();

    private void Awake(){
        if (Instance == null){ 
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadDefaultKeybinds();
        }
        else{
            Destroy(gameObject);
        }
    }

    private void LoadDefaultKeybinds(){
        SetKey("Jump", GetSavedKey("Jump", KeyCode.Space));
        SetKey("Crouch", GetSavedKey("Crouch", KeyCode.C));
        SetKey("Slide", GetSavedKey("Slide", KeyCode.LeftControl));
    }

    public void SetKey(string action, KeyCode key){
        keybinds[action] = key;
        PlayerPrefs.SetString(action, key.ToString());
    }

    public KeyCode GetKey(string action){
        return keybinds.ContainsKey(action) ? keybinds[action] : KeyCode.None;
    }

    private KeyCode GetSavedKey(string action, KeyCode defaultKey) { 
        string saved = PlayerPrefs.GetString(action, defaultKey.ToString());
        return System.Enum.TryParse(saved, out KeyCode result) ? result : defaultKey;
    } 
}
