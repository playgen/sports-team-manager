using System;

namespace SUGAR.Unity
{
    public class LoginEventArgs : EventArgs
    {
        public string Username { get; private set; }
        public string Password { get; private set; }

        public LoginEventArgs(string username, string password)
        {
            Username = username;
            Password = password;
        }
    }
}
