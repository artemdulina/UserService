using System;

namespace MasterServiceClient
{
    class Program
    {
        static void Main(string[] args)
        {
            Service1Client client = new Service1Client();

            Console.WriteLine(client.GetData(10));

            client.Close();
        }
    }
}
