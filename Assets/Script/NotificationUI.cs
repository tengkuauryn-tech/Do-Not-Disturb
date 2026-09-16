using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NotificationUI : MonoBehaviour
{
    [Header("UI Panel & Text References")]
    public GameObject popupPanel;
    public TMP_Text notificationText; 

    [Header("Option Buttons")]
    public Button button1;
    public TMP_Text button1Text;

    public Button button2;
    public TMP_Text button2Text;

    public Button button3;
    public TMP_Text button3Text;

    
    public void DisplayNotification(
        string message, 
        string option1Text, UnityEngine.Events.UnityAction action1,
        string option2Text, UnityEngine.Events.UnityAction action2,
        string option3Text, UnityEngine.Events.UnityAction action3)
    {
        
        if (notificationText != null) notificationText.text = message;

        
        if (button1 != null)
        {
            if (button1Text != null) button1Text.text = option1Text;
            button1.onClick.RemoveAllListeners();
            if (action1 != null) button1.onClick.AddListener(action1);
            button1.onClick.AddListener(CloseNotification);
        }

        
        if (button2 != null)
        {
            if (button2Text != null) button2Text.text = option2Text;
            button2.onClick.RemoveAllListeners();
            if (action2 != null) button2.onClick.AddListener(action2);
            button2.onClick.AddListener(CloseNotification);
        }

        
        if (button3 != null)
        {
            if (button3Text != null) button3Text.text = option3Text;
            button3.onClick.RemoveAllListeners();
            if (action3 != null) button3.onClick.AddListener(action3);
            button3.onClick.AddListener(CloseNotification);
        }

        
        if (popupPanel != null) popupPanel.SetActive(true);
    }

    
    public void CloseNotification()
    {
        if (popupPanel != null) popupPanel.SetActive(false);
    }
}