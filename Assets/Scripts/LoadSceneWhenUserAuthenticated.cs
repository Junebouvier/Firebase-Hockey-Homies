using System;
using System.Collections;
using System.Collections.Generic;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadSceneWhenUserAuthenticated : MonoBehaviour
{
    [SerializeField] private string sceneToLoad = "HockeyLobby";
    void Start()
    {
        FirebaseAuth.DefaultInstance.StateChanged += HandleAuthStateChange;
    }

    private void HandleAuthStateChange(object sender, EventArgs e)
    {
        var currentUser = FirebaseAuth.DefaultInstance.CurrentUser;
        
        if (currentUser != null)
        {
            FirebaseDatabase.DefaultInstance.GetReference("users/" + currentUser.UserId + "/username").GetValueAsync().ContinueWithOnMainThread(task =>
                {
                    if (task.IsFaulted)
                    {
                        return;
                    }
                
                    else if (task.IsCompleted)
                    {
                        string username = (string)task.Result.Value;
                        
                        FirebaseDatabase.DefaultInstance.RootReference.Child("users-online").Child(currentUser.UserId).SetValueAsync(username);
                        
                    }
                    
                    //SceneManager.LoadScene("sceneToLoad");
                    SceneManager.LoadScene(sceneToLoad);
                });
            
            
            
        } 
    }

}
