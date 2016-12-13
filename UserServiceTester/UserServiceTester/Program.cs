using System;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Service;
using Service.IdGenerators;
using Service.Storages;
using System.Configuration;

namespace UserServiceTester
{
    class Program
    {
        static void Main(string[] args)
        {
            XmlWriterSettings settings = new XmlWriterSettings
            {
                Encoding = Encoding.UTF8,
                Indent = true
            };

            MasterUserService master = new MasterUserService(new XmlStorage("users.xml", settings), new DefaultIdGenerator());
            master.OnStart();
            /*User a = new User("artem", "one", DateTime.Today);
            a.VisaRecords.Add(new Visa() { Country = "Belarus", End = DateTime.Now, Start = DateTime.Now });
            a.VisaRecords.Add(new Visa() { Country = "Usa", End = DateTime.Now, Start = DateTime.Now });
            storage.Add(a, new CustomValidation());
            storage.Add(new User("dazzle", "two", DateTime.Today), new CustomValidation());
            storage.Add(new User("ivan", "three", DateTime.Today), new CustomValidation());
            storage.Add(new User("dmitry", "four", DateTime.Today), new CustomValidation());*/

            bool stop = false;
            while (!stop)
            {
                Console.WriteLine("Write command:");
                string command = Console.ReadLine();
                switch (command)
                {
                    case "savestate":
                        master.SaveState();
                        break;
                    case "stop":
                        master.OnStop();
                        break;
                    case "exit":
                        stop = true;
                        break;
                    default:
                        Console.WriteLine("Wrong command");
                        break;
                }

            }
        }
    }
}
