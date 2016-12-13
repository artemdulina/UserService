using System;
using Entities;
using Service;
using Service.Storages;

namespace SlaveUserServiceLauncher
{
    public class Program
    {
        static void Main(string[] args)
        {
            //MasterSlavesConfig masterSlaveConfig = MasterSlavesConfig.GetConfig();

            SlaveUserService slave = new SlaveUserService(new ListStorage());
            slave.OnStart();
            //Console.WriteLine(NetworkHelper.IsPortOpenedWithoutTcp(11000));

            //Console.WriteLine(masterSlaveConfig.Slaves[0].Port);
            //Console.WriteLine(masterSlaveConfig.Slaves[1].Port);
            //Func<User, bool> criteria = user => user.Id == 2;
            //Func<User, bool> criteria = delegate(User user) { return user.Id == 58; };
            //Console.WriteLine(criteria.Method.ToString());
            //Console.WriteLine(criteria.Target);

            while (true)
            {
                Console.WriteLine("Write command:");
                string command = Console.ReadLine();
                switch (command)
                {
                    case "stop":
                        slave.OnStop();
                        break;
                    case "search":
                        Console.WriteLine(slave.Search(Criterias.ConcreteId));
                        break;
                    default:
                        Console.WriteLine("Wrong command");
                        break;
                }

            }

        }
    }
}
