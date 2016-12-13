using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities
{
    public static class Criterias
    {
        public static Func<User,bool> ConcreteId = user => user.Id == 58;
    }
}
