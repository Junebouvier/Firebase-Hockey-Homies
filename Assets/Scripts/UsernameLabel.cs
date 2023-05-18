using System;
using System.Collections;
using System.Collections.Generic;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UsernameLabel : MonoBehaviour
{
    [SerializeField] private TMP_Text label;
    [SerializeField] private TMP_Text labelTitle;
    private void Reset()
    {
        label = GetComponent<TMP_Text>();
        labelTitle = GetComponent<TMP_Text>();
    }
    void Start()
    {
        FirebaseAuth.DefaultInstance.StateChanged += HandleAuthChange;
    }

    private void HandleAuthChange(object sender, EventArgs e)
    {
        var currentUser = FirebaseAuth.DefaultInstance.CurrentUser;
        
        if (currentUser != null)
        {
            SetLabelUsername(currentUser.UserId);
            string name = currentUser.DisplayName;
            string email = currentUser.Email;
            Debug.Log("Email:" + email);
        }
    }

    private void SetLabelUsername(string userID)
    {
        FirebaseDatabase.DefaultInstance
            .GetReference("users/" + userID + "/username")
            .GetValueAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted)
                {
                    Debug.Log(task.Exception);
                    label.text = "NULL";
                }
                
                else if (task.IsCompleted)
                {
                    DataSnapshot snapshot = task.Result;
                    //Debug.Log(snapshot.Value);
                    labelTitle.text = "Logged as: ";
                    label.text = (string)snapshot.Value;
                }
                
            });
        //label.text = "Logged as:" + userID;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
