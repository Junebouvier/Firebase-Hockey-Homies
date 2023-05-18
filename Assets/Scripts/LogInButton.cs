using System;
using System.Collections;
using System.Collections.Generic;
using Firebase.Auth;
using Firebase.Database;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LogInButton : MonoBehaviour
{
    [SerializeField]
    private Button logInButton;
    [SerializeField]
    private TMP_InputField emailInputField;
    [SerializeField]
    private TMP_InputField emailPasswordField;
    
    private Coroutine _logInCoroutine;

    private void Reset()
    {
        logInButton = GetComponent<Button>();
        emailInputField = GameObject.Find("InputEmail").GetComponent<TMP_InputField>();
        emailPasswordField = GameObject.Find("InputPw").GetComponent<TMP_InputField>();
    }

    void Start()
    {
        logInButton.onClick.AddListener(HandleRegistrationButtonClicked);
    }

    private void HandleRegistrationButtonClicked()
    {
        if (_logInCoroutine == null)
        {
            _logInCoroutine = StartCoroutine(LogInCoroutine(emailInputField.text, emailPasswordField.text));
        }
    }

    private IEnumerator LogInCoroutine(string email, string password)
    {
        var auth = FirebaseAuth.DefaultInstance;
        var loginTask = auth.SignInWithEmailAndPasswordAsync(email, password);

        yield return new WaitUntil((() => loginTask.IsCompleted));

        if (loginTask.Exception != null)
        {
            Debug.LogWarning($"Login failed with {loginTask.Exception}");
        }

        else
        {
            Debug.LogWarning($"Login succeeded with {loginTask.Result}");
            
            SceneManager.LoadScene("HockeyLobby");
        }

        _logInCoroutine = null;
    }

}
