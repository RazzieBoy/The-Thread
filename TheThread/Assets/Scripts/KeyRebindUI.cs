using System.Collections;
using TMPro;
using UnityEngine;

public class KeyRebindUI : MonoBehaviour
{

    [Header("UI Elements")]
    public TMP_Text jumpKeyText;
    public TMP_Text crouchKeyText;
    public TMP_Text slideKeyText;

    private string keyToRebind = null;
    private TMP_Text currentLabel;
    private bool isWaitingForKey = false;

    void Start()
    {
        UpdateKeyLabels();
    }

    void OnGUI()
    {
        if (isWaitingForKey && Event.current.isKey)
        {
            KeyCode pressedKey = Event.current.keyCode;

            if (pressedKey != KeyCode.None)
            {
                KeybindManager.Instance.SetKey(keyToRebind, pressedKey);
                Debug.Log($"{keyToRebind} bound to {pressedKey}");

                isWaitingForKey = false;
                keyToRebind = null;
                currentLabel = null;

                UpdateKeyLabels();
            }

            // Use the event so Unity doesn't propagate it further
            Event.current.Use();
        }
    }

    public void RebindJump() => StartCoroutine(StartRebinding("Jump", jumpKeyText));
    public void RebindCrouch() => StartCoroutine(StartRebinding("Crouch", crouchKeyText));
    public void RebindSlide() => StartCoroutine(StartRebinding("Slide", slideKeyText));

    private IEnumerator StartRebinding(string action, TMP_Text label)
    {
        yield return new WaitForSeconds(0.1f); // Delay to avoid capturing the click that opened the UI

        keyToRebind = action;
        currentLabel = label;
        currentLabel.text = "Press any key...";
        isWaitingForKey = true;
    }

    private void UpdateKeyLabels()
    {
        jumpKeyText.text = KeybindManager.Instance.GetKey("Jump").ToString();
        crouchKeyText.text = KeybindManager.Instance.GetKey("Crouch").ToString();
        slideKeyText.text = KeybindManager.Instance.GetKey("Slide").ToString();
    }
}
