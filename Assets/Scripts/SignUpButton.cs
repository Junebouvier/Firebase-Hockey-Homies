using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Schema;
using Firebase.Auth;
using Firebase.Database;
using Google.MiniJSON;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SignUpButton : MonoBehaviour
{
    [SerializeField]
    private Button signUpButton;
    private Coroutine _signUpCoroutine;

    private void Reset()
    {
        signUpButton = GetComponent<Button>();
    }

    void Start()
    { 
        signUpButton.onClick.AddListener(HandleRegistrationButtonClicked);
    }

    private void HandleRegistrationButtonClicked()
    {
        string email = GameObject.Find("InputEmail").GetComponent<TMP_InputField>().text;
        string password = GameObject.Find("InputPw").GetComponent<TMP_InputField>().text;

        _signUpCoroutine = StartCoroutine(SignUpUser(email, password));
    }

    private IEnumerator SignUpUser(string email, string password)
    {
        var auth = FirebaseAuth.DefaultInstance;
        var registerTask = auth.CreateUserWithEmailAndPasswordAsync(email,password);

        yield return new WaitUntil((() => registerTask.IsCompleted));

        if (registerTask.Exception != null)
        {
            Debug.LogWarning($"Failed to register task {registerTask.Exception}");
        }

        else
        {
            Debug.Log($"Succesfully registered user {registerTask.Result.Email}");

            UserData data = new UserData();
            
            data.username = GameObject.Find("InputUsername").GetComponent<TMP_InputField>().text;
            string json = JsonUtility.ToJson(data);
            
            FirebaseDatabase.DefaultInstance.RootReference.Child("users").Child(registerTask.Result.UserId).SetRawJsonValueAsync(json);
            //Registrar datos adicionales del usuario en Database
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}

public class UserData
{
    public string username;
    
}
