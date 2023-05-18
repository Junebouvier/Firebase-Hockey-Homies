using System.Collections;
using Firebase.Auth;
using Firebase.Database;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FriendosController : MonoBehaviour
{
    DatabaseReference databaseReference;
    DatabaseReference rFriends;
    DatabaseReference usersOnlineReference;

    [SerializeField] TMP_Text friendsListText;
    [SerializeField] TMP_Text onlineFriendsListText;

    private Dictionary<string, string> friends = new Dictionary<string, string>();
    private List<string> onlineFriendsIds = new List<string>();

    void Start()
    {
        databaseReference = FirebaseDatabase.DefaultInstance.RootReference;
        
        var currentUserID = FirebaseAuth.DefaultInstance.CurrentUser.UserId;
        rFriends = databaseReference.Child("users").Child(currentUserID).Child("Friends");
        
        usersOnlineReference = databaseReference.Child("users-online");

        rFriends.ValueChanged += HandleFriendsValueChanged;
        usersOnlineReference.ValueChanged += HandleUsersOnlineValueChanged;
    }

    private void HandleFriendsValueChanged(object sender, ValueChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }

        friends.Clear();

        if (args.Snapshot != null && args.Snapshot.Value != null)
        {
            var friendsDictionary = (Dictionary<string, object>)args.Snapshot.Value;

            foreach (var friendId in friendsDictionary.Keys)
            {
                var friendUsername = friendsDictionary[friendId].ToString();
                friends.Add(friendId, friendUsername);
            }
        }

        UpdateFriendsList();
    }

    private void HandleUsersOnlineValueChanged(object sender, ValueChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }

        onlineFriendsIds.Clear();

        if (args.Snapshot != null && args.Snapshot.Value != null)
        {
            var usersOnlineDictionary = (Dictionary<string, object>)args.Snapshot.Value;

            //Ver si hay amigos en línea que no estén en la lista de amigos actual
            foreach (var friendId in friends.Keys)
            {
                if (usersOnlineDictionary.ContainsKey(friendId))
                {
                    onlineFriendsIds.Add(friendId);
                }
            }
        }

        UpdateOnlineFriendsList();
    }

    private void UpdateFriendsList()
    {
        string friendsList = "";

        foreach (var friend in friends)
        {
            friendsList += friend.Value + "\n\n";
        }

        if (friendsListText != null)
        {
            friendsListText.text = friendsList;
        }
    }

    private void UpdateOnlineFriendsList()
    {
        string onlineFriendsList = "";

        foreach (var friendId in onlineFriendsIds)
        {
            var friendUsername = friends[friendId];
            onlineFriendsList += friendUsername + "\n\n";
        }

        if (onlineFriendsListText != null)
        {
            onlineFriendsListText.text = onlineFriendsList;
        }
    }
}

