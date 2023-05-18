using Firebase.Auth;
using Firebase.Database;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LogOutButton : MonoBehaviour, IPointerClickHandler
{ 
    public void OnPointerClick(PointerEventData eventData)
    {
        FirebaseDatabase.DefaultInstance.RootReference.Child("users-online").Child(FirebaseAuth.DefaultInstance.CurrentUser.UserId).SetValueAsync(null);
        FirebaseAuth.DefaultInstance.SignOut();
        SceneManager.LoadScene("Main menu");
    }
    
   
}
