using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service
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

        public override string ToString()
        {
            return $"Type is: {Type}, User is: {User}";
        }
    }
}
