namespace GInterfaceCore.Interfaces
{
    public interface IAuthenticationService
    {
        Task<bool> LoginAsync(string username, string password);
        Task LogoutAsync();
        // Otros métodos relacionados con la autenticación
    }
}
