using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;

public class SearchUsernameBefriend : MonoBehaviour
{
    DatabaseReference rDatabase;
    DatabaseReference rUsuariosTotal;
    DatabaseReference rUsuariosOnline; //No lo usamos por ahora
    
    [SerializeField] private TMP_Text currentUsernameLabel;
    [SerializeField] private TMP_Text userBuscado;
    [SerializeField] private TMP_Text feedbackText;
    
    private DataSnapshot datosUsers;
    public async void Verification()
    {
        string userBuscadoSinExtra = userBuscado.text.Trim().Remove(userBuscado.text.Length-1); //Estoy definiendo una variable para el user sin el carácter invisible
        
        
        if (userBuscadoSinExtra == currentUsernameLabel.text)
        {
            feedbackText.text = "You can't add yourself!";
        }
        
        else
        {
            rDatabase = FirebaseDatabase.DefaultInstance.RootReference;
            
            //Consulto si hay un usuario cuyo username sea el digitado
            var consultaUsers= rDatabase.Child("users").OrderByChild("username").EqualTo(userBuscadoSinExtra);
            
            //Obtengo los datos de la consulta
            datosUsers = await consultaUsers.GetValueAsync();
            
            if (datosUsers.HasChildren)//Si sí encontré un usuario que cumple con la consulta...
            {
                Debug.Log("El usuario " + userBuscadoSinExtra + " existe!");
                SendFriendRequest();
            }
            else
            {
                feedbackText.text = ("User " + "'" + userBuscadoSinExtra + "' doesn't exist");
                
            }
        }
        
    }

    public async void SendFriendRequest()
    {
        try
        {
            rDatabase = FirebaseDatabase.DefaultInstance.RootReference;
    
            string userIdActual = FirebaseAuth.DefaultInstance.CurrentUser.UserId; //Obtengo ID usuario actual
            string userBuscadoId = datosUsers.Children.First().Key; // Obtengo ID usuario buscado, suponiendo que solo se encuentra un usuario
            
            var usuarioActual = await rDatabase.Child("users").Child(userIdActual).GetValueAsync(); //Obtengo ref usuario actual
            var usernameActual = usuarioActual.Child("username").Value.ToString();
            
            var usuarioBuscado = await rDatabase.Child("users").Child(userBuscadoId).GetValueAsync(); //Obtengo ref usuario buscado
            var usernameBuscado = usuarioBuscado.Child("username").Value.ToString();
            
            //EMISOR
            
            var solicitudesAmistadActual = usuarioActual.Child("solicitudesAmistadSalientes").Value as Dictionary<string, object>; //Obtengo diccionario de solicitudes salientes del usuario actual
            
            if (solicitudesAmistadActual == null) //Si el usuario no tiene solicitudes de amistad aún, creo el diccionario
            {
                solicitudesAmistadActual = new Dictionary<string, object>();
            }
            
            solicitudesAmistadActual.Add(userBuscadoId, usernameBuscado); //Agrego el ID del usuario buscado como clave en el diccionario de solicitudes de amistad salientes del usuario actual
            
            //Actualizo el diccionario de solicitudes de amistad del usuario actual en la base de datos, EL QUE MANDÓ SOLICITUD
            var updateSolicitudesAmistadTask = rDatabase.Child("users").Child(userIdActual).Child("solicitudesAmistadSalientes").SetValueAsync(solicitudesAmistadActual);
            await updateSolicitudesAmistadTask;
            
            
            //RECEPTOR
            
            var solicitudesAmistadBuscado = usuarioBuscado.Child("solicitudesAmistadEntrantes").Value as Dictionary<string, object>; //Obtengo diccionario de solicitudes entrantes del usuario buscado
            
            if (solicitudesAmistadBuscado == null)
            {
                solicitudesAmistadBuscado = new Dictionary<string, object>();
            }
            
            solicitudesAmistadBuscado.Add(userIdActual, usernameActual); //Agrego el ID del usuario actual como clave en el diccionario de solicitudes de amistad entrantes del usuario buscado
            
            //Actualizo el diccionario de solicitudes de amistad del usuario buscado en la base de datos, EL QUE RECIBIÓ SOLICITUD
            var updateSolicitudesAmistadBuscadoTask = rDatabase.Child("users").Child(userBuscadoId)
                .Child("solicitudesAmistadEntrantes").SetValueAsync(solicitudesAmistadBuscado);
            await updateSolicitudesAmistadBuscadoTask;
            
            feedbackText.text = "Solicitud de amistad enviada a " + usernameBuscado;
        }
        catch (ArgumentException e)
        {
            // Mostramos un mensaje de error al usuario indicando que la solicitud de amistad ya ha sido enviada previamente
            feedbackText.text = "Ya has enviado una solicitud de amistad a este usuario previamente.";
        }
        catch (Exception e)
        {
            // Mostramos un mensaje de error genérico en caso de que ocurra alguna otra excepción
            feedbackText.text = "Ha ocurrido un error al enviar la solicitud de amistad. Por favor, inténtalo de nuevo más tarde.";
        }
        
    }
    
}
