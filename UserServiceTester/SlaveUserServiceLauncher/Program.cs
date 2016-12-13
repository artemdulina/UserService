using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Service;
using Service.CustomSections;
using Service.IdGenerators;
using Service.Storages;

namespace SlaveUserServiceLauncher
{
    class Program
    {
        static void Main(string[] args)
        {
            //MasterSlavesConfig masterSlaveConfig = MasterSlavesConfig.GetConfig();

            SlaveUserService slave = new SlaveUserService(new ListStorage());
            slave.OnStart();
            //Console.WriteLine(NetworkHelper.IsPortOpenedWithoutTcp(11000));

            //Console.WriteLine(masterSlaveConfig.Slaves[0].Port);
            //Console.WriteLine(masterSlaveConfig.Slaves[1].Port);

            bool stop = false;
            while (!stop)
            {
                Console.WriteLine("Write command:");
                string command = Console.ReadLine();
                switch (command)
                {
                    case "stop":
                        slave.OnStop();
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
