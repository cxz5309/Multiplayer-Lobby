using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LobbyScene : MonoBehaviour
{
    public static LobbyScene instance;

    private void Start()
    {
        instance = this;
    }

    public void OnClickCreateAccount()
    {
        DisableInputs();

        string username = GameObject.Find("CreateUserName").GetComponent<TMP_InputField>().text;
        string password = GameObject.Find("CreatePassword").GetComponent<TMP_InputField>().text;
        string email = GameObject.Find("CreateEmail").GetComponent<TMP_InputField>().text;

        Client.instance.SendCreateAccount(username, password, email);
    }
    public void OnClickLoginRequest()
    {
        DisableInputs();

        string userNameOrPassword = GameObject.Find("LoginUserNameEmail").GetComponent<TMP_InputField>().text;
        string password = GameObject.Find("LoginPassword").GetComponent<TMP_InputField>().text;

        Client.instance.SendLoginRequest(userNameOrPassword, password);
    }

    public void ChangeWelcomeMessage(string msg)
    {
        GameObject.Find("WelcomeMessage").GetComponent<TextMeshProUGUI>().text = msg;
    }
    public void ChangeAuthenticationMessage(string msg)
    {
        GameObject.Find("AuthenticationMessage").GetComponent<TextMeshProUGUI>().text = msg;
    }

    public void EnableInputs()
    {
        GameObject.Find("Canvas").GetComponent<CanvasGroup>().interactable = true;
    }
    public void DisableInputs()
    {
        GameObject.Find("Canvas").GetComponent<CanvasGroup>().interactable = false;
    }
}
