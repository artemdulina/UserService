using System;
using System.Text;
using System.Xml;
using Entities;
using Service;
using Service.IdGenerators;
using Service.Storages;
using Service.Validations;

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
            storage.Add(new User("dmitry", "four", DateTime.Today), new CustomValidation());*/

            while (true)
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
                    case "add":
                        master.Add(new User("abrakadabra", "four", DateTime.Today), new CustomValidation());
                        break;
                    case "all":
                        foreach (var user in master.GetAll())
                        {
                            Console.WriteLine(user);
                        }
                        break;
                    case "delete1":
                        master.Delete(1);
                        break;
                    case "delete":
                        master.Delete(new User("dmitry", "four", DateTime.Today));
                        break;
                    default:
                        Console.WriteLine("Wrong command");
                        break;
                }

            }
        }
    }
}
