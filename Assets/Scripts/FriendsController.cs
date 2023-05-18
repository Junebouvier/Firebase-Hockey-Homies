using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Auth;
using Firebase.Database;
using TMPro;

public class FriendsController : MonoBehaviour
{
    DatabaseReference rDatabase;
    DatabaseReference rFriends;
    DatabaseReference rUsersOnline;
    
    [SerializeField] GameObject friendsObject;
    [SerializeField] GameObject friendsOnlineObject;
    
    private List<string> friendList = new List<string>();
    private List<string> friendsOnlineList = new List<string>();
    
    // Start is called before the first frame update
    void Start()
    {
        rDatabase = FirebaseDatabase.DefaultInstance.RootReference;
        
        var currentUserID = FirebaseAuth.DefaultInstance.CurrentUser.UserId;
        rFriends = rDatabase.Child("users").Child(currentUserID).Child("Friends");
        
        rFriends.ChildAdded += HandleChildAdded;
        rFriends.ChildRemoved += HandleChildRemoved;
        //refer.ChildAdded += HandleChildChanged;
        
        // Escucha cambios en la ubicación "users-online"
        rUsersOnline = rDatabase.Child("users-online");
        rUsersOnline.ChildAdded += HandleUserOnline;
        rUsersOnline.ChildRemoved += HandleUserOffline;
        
    }
    
    private void HandleChildAdded(object sender, ChildChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }
        //Hacer pregunta si ese key no está contenido ya
        friendList.Add(args.Snapshot.Value.ToString());
        
        UpdateFriendList();
    }

    private void HandleChildRemoved(object sender, ChildChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }
        
        friendList.Remove(args.Snapshot.Value.ToString());
        
        UpdateFriendList();
    }

    void UpdateFriendList()
    {
        string userFriends = string.Join("\n\n", friendList);
    
        //Verificar si el objeto usersOnlineObject es nulo antes de actualizarlo
        if (friendsObject != null)
        {
            //Obtengo el componente Text de mi GameObject
            TMP_Text textComponent = friendsObject.GetComponent<TMP_Text>();
            textComponent.text = userFriends;
        }
    }
    
    void HandleUserOnline(object sender, ChildChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }

        // Agrega el usuario en línea a la lista de amigos en línea si es un amigo del usuario actual
        string userID = args.Snapshot.Key;
        Debug.Log(userID);
        if (friendList.Contains(userID))
        {
            Debug.Log(userID + " is online and is a friend of the current user");
            friendsOnlineList.Add(userID);
            UpdateFriendsOnlineList();
        }
    }

    void HandleUserOffline(object sender, ChildChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }

        // Quita el usuario desconectado de la lista de amigos en línea si es un amigo del usuario actual
        string userID = args.Snapshot.Key;
        if (friendList.Contains(userID))
        {
            Debug.Log(userID + " is offline and is a friend of the current user");
            friendsOnlineList.Remove(userID);
            UpdateFriendsOnlineList();
        }
    }
    
    void UpdateFriendsOnlineList()
    {
        string friendsOnline = string.Join("\n\n", friendsOnlineList);

        if (friendsOnlineObject != null)
        {
            TMP_Text textComponent = friendsOnlineObject.GetComponent<TMP_Text>();
            textComponent.text = friendsOnline;
        }
    }
}
