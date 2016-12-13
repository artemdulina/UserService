using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Entitites
{
    public enum Command
    {
        Add,
        DeleteById,
        DeleteByUser,
        Search,
        SlaveCreated
    }

    [Serializable]
    public class Message
    {
        public Command Type { get; set; }
        public User User { get; set; }
        public IPEndPoint IpEndPoint { get; set; }
        public Func<User, bool> Criteria { get; set; }

        public override string ToString()
        {
            return $"Type is: {Type}, User is: {User}, IpEndPoint is: {IpEndPoint}";
        }
    }
}
