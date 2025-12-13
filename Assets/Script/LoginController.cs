using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoginController : MonoBehaviour
{
    public Button appleButton;
    public Text errorText;

    void Start()
    {
        errorText.gameObject.SetActive(false);
        appleButton.onClick.AddListener(HandleLogin);
    }

    void HandleLogin()
    {
        bool loginSuccess = true; // mock success

        if (loginSuccess)
            SceneManager.LoadScene("HumanoidScene");
        else
            errorText.gameObject.SetActive(true);
    }
}
