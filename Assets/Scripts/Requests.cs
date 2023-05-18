using System.Collections;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine.UI;
using UnityEngine;

public class Requests : MonoBehaviour
{
    [SerializeField] private TMP_Text feedbackRequest;
    public GameObject solicitudPrefab;
    
    DatabaseReference rDatabase;

    private List<string> listSenders = new List<string>();
    
    List<GameObject> solicitudGameObjects = new List<GameObject>();
    public void Start()
    {
        rDatabase = FirebaseDatabase.DefaultInstance.RootReference;
        var currentUserID = FirebaseAuth.DefaultInstance.CurrentUser.UserId;
        var currentUserRef = rDatabase.Child("users").Child(currentUserID);//Ref usuario actual

        //Escucha cambios en la lista de solicitudes entrantes
        currentUserRef.Child("solicitudesAmistadEntrantes").ValueChanged += HandleValueChanged;
    }

    private async void HandleValueChanged(object sender, ValueChangedEventArgs e)
    {
        if (gameObject != null)
        {
            var startPosition = transform.position;
            var yOffset = 20.0f;
        
            if (e.DatabaseError != null)
            {
                Debug.LogError("Error al obtener las solicitudes de amistad entrantes: " + e.DatabaseError.Message);
                return;
            }

            listSenders.Clear();

            if (e.Snapshot.Value != null)//Sí hay solicitudes
            {
                foreach (var solicitud in e.Snapshot.Children)
                {
                
                    string userID = solicitud.Key;
                    var userSnapshot = await rDatabase.Child("users").Child(userID).GetValueAsync();
                    string userName = null;

                    if (userSnapshot != null && userSnapshot.Value != null)
                    {
                        userName = userSnapshot.Child("username").Value.ToString();
                    }

                    var solicitudGameObject = Instantiate(solicitudPrefab, transform);
                    solicitudGameObject.transform.position = startPosition + new Vector3(-10f, yOffset, 0.0f);
                    solicitudGameObjects.Add(solicitudGameObject);

                    var userSenderText = solicitudGameObject.transform.Find("userSenderText").GetComponent<TMP_Text>();
                    var aceptarButton = solicitudGameObject.transform.Find("AcceptButton").GetComponent<Button>();
                    var rechazarButton = solicitudGameObject.transform.Find("DeclineButton").GetComponent<Button>();

                    userSenderText.text = userName;

                    aceptarButton.onClick.AddListener(() => AcceptRequest(userID));
                    rechazarButton.onClick.AddListener(() => DeclineRequest(userID));

                    feedbackRequest.text = "Incoming from:";
                    listSenders.Add(userName);
                    Debug.Log(userName);

                    yOffset -= 60.0f;
                }

            
            }
            else
            {
                feedbackRequest.text = "You don't have any incoming friend requests";
            }
        }
        
    }

    public async void AcceptRequest(string userSenderID)
    {
        rDatabase = FirebaseDatabase.DefaultInstance.RootReference;
    
        string userIdActual = FirebaseAuth.DefaultInstance.CurrentUser.UserId; //Obtengo ID usuario actual
        var usuarioActual = await rDatabase.Child("users").Child(userIdActual).GetValueAsync(); //Obtengo ref usuario actual
        var usernameActual = usuarioActual.Child("username").Value.ToString();

        // Obtengo ID usuario buscado, suponiendo que solo se encuentra un usuario
        var usuarioAmistado = await rDatabase.Child("users").Child(userSenderID).GetValueAsync(); //Obtengo ref usuario buscado
        var usernameAmistado = usuarioAmistado.Child("username").Value.ToString();
        
        //ACEPTADOR
        
        var amigosActual = usuarioActual.Child("Friends").Value as Dictionary<string, object>; //Obtengo diccionario de amix del usuario actual
        
        if (amigosActual == null) //Si el usuario no tiene amix aún, creo el diccionario
        {
            amigosActual = new Dictionary<string, object>();
        }
        
        amigosActual.Add(userSenderID, usernameAmistado); //Agrego el ID del usuario buscado como clave en el diccionario de amix del usuario actual
        
        //Actualizo el diccionario de amix
        var updateFriendsTask = rDatabase.Child("users").Child(userIdActual).Child("Friends").SetValueAsync(amigosActual);
        await updateFriendsTask;
        
        //ACEPTADO
        
        var amigosAmistado = usuarioAmistado.Child("Friends").Value as Dictionary<string, object>; //Obtengo diccionario de amix del usuario buscado
        
        if (amigosAmistado == null)
        {
            amigosAmistado = new Dictionary<string, object>();
        }
        
        amigosAmistado.Add(userIdActual, usernameActual); //Agrego el ID del usuario actual como clave en el diccionario de amix del usuario amistado
        
        var updateSolicitudesAmistadBuscadoTask = rDatabase.Child("users").Child(userSenderID).Child("Friends").SetValueAsync(amigosAmistado);
        await updateSolicitudesAmistadBuscadoTask;

        await CleanRequests(userSenderID);
    }

    public async void DeclineRequest(string userSenderID)
    {
        CleanRequests(userSenderID);

    }
    
    private async Task CleanRequests(string userSenderID)
    {
        var currentUserID = FirebaseAuth.DefaultInstance.CurrentUser.UserId;
        var currentUserRequestsRef = rDatabase.Child("users").Child(currentUserID).Child("solicitudesAmistadEntrantes");
        var senderUserRequestsRef = rDatabase.Child("users").Child(userSenderID).Child("solicitudesAmistadSalientes");

        // Eliminar la solicitud entrante del remitente
        await currentUserRequestsRef.Child(userSenderID).RemoveValueAsync();
        // Eliminar la solicitud saliente del usuario actual
        await senderUserRequestsRef.Child(currentUserID).RemoveValueAsync();
    
        // Obtener la referencia al nodo "requestsSent" del usuario actual
        var currentUserRequestsSentRef = rDatabase.Child("users").Child(currentUserID).Child("requestsSent");
        var requestsSentSnapshot = await currentUserRequestsSentRef.GetValueAsync();

        if (requestsSentSnapshot.HasChild(userSenderID)) // Verificar si hay una solicitud enviada correspondiente al remitente
        {
            await currentUserRequestsSentRef.Child(userSenderID).RemoveValueAsync(); // Eliminar la solicitud enviada
        }
        
        
    }

}
