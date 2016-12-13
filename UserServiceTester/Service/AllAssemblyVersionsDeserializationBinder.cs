using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Entities;

namespace Service
{
    sealed class AllAssemblyVersionsDeserializationBinder : SerializationBinder
    {
        public override Type BindToType(string assemblyName, string typeName)
        {
            Type typeToDeserialize = null;
            Console.WriteLine("TYPENAME: " + typeName);

            string currentAssembly = typeof(Message).Assembly.FullName;

            typeToDeserialize = Type.GetType($"{typeName}, {currentAssembly}");

            return typeToDeserialize;
        }
    }
}
