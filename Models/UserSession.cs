using System.Collections.Concurrent;

namespace ApiGateway.Models
{
    public class UserSession
    {
        public ConcurrentDictionary<string, long> Sessions { get; set; }

        public UserSession()
        {
            this.Sessions = new();
        }
    }
}
