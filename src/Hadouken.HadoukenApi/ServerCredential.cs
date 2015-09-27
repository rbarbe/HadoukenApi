using System;

namespace Hadouken.HadoukenApi
{
    public class ServerCredential
    {
        public ServerCredential(Uri argUri, string argUsername, string argPassword)
        {
            Uri = argUri;
            Username = argUsername;
            Password = argPassword;
        }

        public Uri Uri { get; }
        public string Username { get; }
        public string Password { get; }
    }
}