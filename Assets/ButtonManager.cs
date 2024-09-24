using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonManager : MonoBehaviour
{
    public List<Button> buttons; // List of buttons

    // Declare the public event
    public delegate void ButtonRestoreEvent();
    public static event ButtonRestoreEvent OnButtonRestore;

    void Start()
    {
        // Register the click event for each button
        foreach (Button btn in buttons)
        {
            btn.onClick.AddListener(() => OnButtonClick(btn));
        }

        // Subscribe RestoreButtonsAfterDelay to the OnButtonRestore event
        OnButtonRestore += RestoreButtonsAfterDelay;
    }

    // Change the method to public so it can be called externally
    public void OnButtonClick(Button clickedButton)
    {
        // Check if the clicked button has the "NonInteractable" tag
        if (clickedButton.tag != "NonInteractable")
        {
            // Change the tag of the clicked button to "NonInteractable"
            clickedButton.tag = "NonInteractable";
            clickedButton.interactable = false;

            // Make all buttons non-interactable
            foreach (Button btn in buttons)
            {
                btn.interactable = false;
            }
        }
    }

    // Public method to restore buttons
    public void RestoreButtonsAfterDelay()
    {
        // Restore the state of buttons that do not have the "NonInteractable" tag
        foreach (Button btn in buttons)
        {
            if (btn.tag != "NonInteractable")
            {
                btn.interactable = true;
            }
        }
    }

    // Unsubscribe the event to avoid memory leaks
    private void OnDestroy()
    {
        OnButtonRestore -= RestoreButtonsAfterDelay;
    }

    // Static method to trigger the event
    public static void TriggerButtonRestore()
    {
        OnButtonRestore?.Invoke();
    }
}
