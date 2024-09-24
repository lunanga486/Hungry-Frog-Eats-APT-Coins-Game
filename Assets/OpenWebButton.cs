using UnityEngine;
using UnityEngine.UI;

public class OpenWebButton : MonoBehaviour
{
    public Button yourButton;

    void Start()
    {
        yourButton.onClick.AddListener(OpenWebPage);
    }

    void OpenWebPage()
    {
        Application.OpenURL("https://www.aptosfaucet.com/");
    }
}
