namespace GInterfaceCore.Interfaces
{
    public interface IAuthenticationService
    {
        Task<bool> LoginAsync(string username, string password);
    }
}
