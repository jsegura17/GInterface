﻿using RestSharp;
using System.Data;

using System.Data.SqlClient;
using System.Configuration;
namespace GInterface.Interfaces
{
    public class AuthenticationService : IAuthenticationService
    {
        //Get Global Core Settings
        private Core.AppCore _appCore = Core.AppCore.Instance;

        // Dependencias, como un servicio para acceder a la base de datos, etc.
       
       

        public async Task<bool> LoginAsync(string username, string password)
        {
            bool response =_appCore.SqlVerificationUser(username,password);
            // Aquí implementarías la lógica de autenticación real
            // Por ejemplo, verificar las credenciales contra una base de datos
            return await Task.FromResult(response);
        }

        public async Task LogoutAsync()
        {
            // Aquí implementarías la lógica para cerrar la sesión del usuario
        }
    }
}
