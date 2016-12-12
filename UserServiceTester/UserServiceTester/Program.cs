using System;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Service;
using Service.IdGenerators;
using Service.Storages;
using System.Configuration;
using System.Net;
using Service.CustomSections;

namespace UserServiceTester
{
    class Program
    {
        public interface ISome
        {
            void Do();
        }

        public class T : ISome
        {
            void ISome.Do() { }
        }

        public class V : T
        {

        }
        static void Main(string[] args)
        {
            var serverStatePath = ConfigurationManager.AppSettings["serverStatePathFile"];

            var masterSlaveConfig = MasterSlavesConfig.GetConfig();
            /*foreach (Slave slave in masterSlaveConfig.Slaves)
            {
                Console.WriteLine(slave.Ip + " " + slave.Port);
            }*/

            IPAddress ipAddress = IPAddress.Parse(masterSlaveConfig.Master.Ip);
            int port = int.Parse(masterSlaveConfig.Master.Port);
            Console.WriteLine(ipAddress + ":" + port);

            XmlWriterSettings settings = new XmlWriterSettings
            {
                Encoding = Encoding.UTF8,
                Indent = true
            };

            string stateFilePath = AppDomain.CurrentDomain.BaseDirectory + @"..\..\state\" + serverStatePath;

            XDocument xmlXDocument = XDocument.Load(stateFilePath);
            int startId = Convert.ToInt32(xmlXDocument.Root?.Element(XName.Get("lastGeneratedId"))?.Value);

            MasterUserService storage = new MasterUserService(new XmlStorage("users.xml", settings), new DefaultIdGenerator());

            /*User a = new User("artem", "one", DateTime.Today);
            a.VisaRecords.Add(new Visa() { Country = "Belarus", End = DateTime.Now, Start = DateTime.Now });
            a.VisaRecords.Add(new Visa() { Country = "Usa", End = DateTime.Now, Start = DateTime.Now });
            storage.Add(a, new CustomValidation());
            storage.Add(new User("dazzle", "two", DateTime.Today), new CustomValidation());
            storage.Add(new User("ivan", "three", DateTime.Today), new CustomValidation());
            storage.Add(new User("dmitry", "four", DateTime.Today), new CustomValidation());*/

            /*foreach (var item in storage.GetAll())
            {
                Console.WriteLine(item);
            }*/

            string command = "";
            bool stop = false;
            while (!stop)
            {
                Console.WriteLine("Write command:");
                command = Console.ReadLine();
                switch (command)
                {
                    case "start":
                        storage.OnStart();
                        break;
                    case "savestate":
                        storage.SaveState();
                        break;
                    case "stop":
                        storage.OnStop();
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
