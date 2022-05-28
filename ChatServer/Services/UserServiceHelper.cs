using ChatServer.Models;

namespace ChatServer.Services
{
    public static class UserServiceHelper
    {
        public static void AddUserService(this IServiceCollection service)
        {
            service.AddSingleton<List<User>>();
        }
    }
}
