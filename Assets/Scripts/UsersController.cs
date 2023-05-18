using System;
using System.Collections;
using System.Collections.Generic;
using Firebase.Auth;
using Firebase.Database;
using TMPro;
using UnityEngine;

public class UsersController : MonoBehaviour
{
    DatabaseReference rDatabase;
    DatabaseReference rUsuariosOnline;
    
    [SerializeField] GameObject usersOnlineObject;
    
    private List<string> userOnList = new List<string>();
    
    // Start is called before the first frame update
    void Start()
    {
        rDatabase = FirebaseDatabase.DefaultInstance.RootReference;
        
        //var refer = FirebaseDatabase.DefaultInstance.GetReference("users-online");
        rUsuariosOnline = rDatabase.Child("users-online");
        
        rUsuariosOnline.ChildAdded += HandleChildAdded;
        rUsuariosOnline.ChildRemoved += HandleChildRemoved;
        //refer.ChildAdded += HandleChildChanged;

    }
    private void HandleChildAdded(object sender, ChildChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }
        
        Debug.Log("Usuario conectado: " + args.Snapshot.Value);
        userOnList.Add(args.Snapshot.Value.ToString());
        
        UpdateUserList();
    }
    private void HandleChildRemoved(object sender, ChildChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }
        
        Debug.Log("Usuario desconectado: " + args.Snapshot.Value);
        userOnList.Remove(args.Snapshot.Value.ToString());
        
        UpdateUserList();
    }

    void UpdateUserList()
    {
        string usersOnline = string.Join("\n\n", userOnList);
    
        //Verificar si el objeto usersOnlineObject es nulo antes de actualizarlo
        if (usersOnlineObject != null)
        {
            //Obtengo el componente Text de mi GameObject
            TMP_Text textComponent = usersOnlineObject.GetComponent<TMP_Text>();
            textComponent.text = usersOnline;
        }
    }
    private void OnApplicationQuit()
    {
        rUsuariosOnline.Child(FirebaseAuth.DefaultInstance.CurrentUser.UserId).SetValueAsync(null); //Dejo de estar online
        FirebaseAuth.DefaultInstance.SignOut(); //Cierra sesión
        Debug.Log("Su sesión se cerró");
    }
}
