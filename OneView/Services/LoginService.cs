using OneView.Models;

namespace OneView.Services
{
    public class LoginService
    {
        private readonly Login _logdata = new();
        public bool IsLogedIn()
        {
            if (!string.IsNullOrEmpty(_logdata.Username ) && !string.IsNullOrEmpty(_logdata.Password))
            {
                return true;
            }
            return false;
        }
    }
}
