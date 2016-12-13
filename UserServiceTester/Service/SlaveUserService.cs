using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Entities;
using Service.CustomSections;
using Service.Interfaces;

namespace Service
{
    public class SlaveUserService : ISlaveService<User>
    {
        private readonly IUserStorage userStorage;
        private Task ListnerTask { get; set; }
        private CancellationTokenSource TokenSource { get; set; }
        private CancellationToken CancelToken { get; set; }
        private IPEndPoint MasterIpEndPoint { get; set; }
        private IPEndPoint ipEndPoint;

        public SlaveUserService(IUserStorage userStorage)
        {
            this.userStorage = userStorage;
            SetSettingsFromConfig();
        }

        private void SetSettingsFromConfig()
        {
            MasterSlavesConfig masterSlaveConfig = MasterSlavesConfig.GetConfig();

            IPAddress ipMaster = IPAddress.Parse(masterSlaveConfig.Master.Ip);
            int portMaster = int.Parse(masterSlaveConfig.Master.Port);
            MasterIpEndPoint = new IPEndPoint(ipMaster, portMaster);

            foreach (Slave slave in masterSlaveConfig.Slaves)
            {
                IPAddress ipSlave = IPAddress.Parse(slave.Ip);
                int portSlave = int.Parse(slave.Port);
                if (!NetworkHelper.IsPortOpened(ipSlave, portSlave))
                {
                    ipEndPoint = new IPEndPoint(ipSlave, portSlave);
                    break;
                }
            }
        }

        private Message SendMessageToMaster(Message message)
        {
            Socket sender = new Socket(ipEndPoint.Address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            Message received = null;
            try
            {
                sender.Connect(MasterIpEndPoint);
                using (MemoryStream stream = new MemoryStream())
                {
                    try
                    {
                        BinaryFormatter formatter = new BinaryFormatter();

                        //Console.WriteLine("start serialize");
                        formatter.Serialize(stream, message);
                        //Console.WriteLine("end serialize");

                        sender.Send(stream.ToArray());

                        byte[] bytes = new byte[10000];
                        int bytesRec = sender.Receive(bytes);
                        //Console.WriteLine(bytesRec);

                        using (MemoryStream receivedStream = new MemoryStream(bytes))
                        {
                            received = (Message)formatter.Deserialize(receivedStream);
                        }

                    }
                    catch (SerializationException e)
                    {
                        Console.WriteLine("Failed to serialize. Reason: " + e.Message);
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }
            finally
            {
                sender.Shutdown(SocketShutdown.Both);
                sender.Close();
            }

            return received;
        }

        public User Search(Func<User, bool> criteria)
        {
            //Console.WriteLine(criteria.Method.ToString());
            //Console.WriteLine(criteria.Target);
            if (criteria == null)
            {
                throw new ArgumentNullException(nameof(criteria));
            }
            User found = userStorage.Search(criteria);

            Message answer = null;
            if (found == null)
            {
                answer = SendMessageToMaster(new Message()
                {
                    Type = Command.Search,
                    Criteria = criteria
                });
            }

            if (answer?.User != null)
            {
                found = answer.User;
                userStorage.Add(found);
            }

            return found;
        }

        private void SendCreationMessage()
        {
            Socket sender = new Socket(ipEndPoint.Address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                sender.Connect(MasterIpEndPoint);
                using (MemoryStream stream = new MemoryStream())
                {
                    try
                    {
                        BinaryFormatter formatter = new BinaryFormatter();

                        formatter.Serialize(stream, new Message()
                        {
                            Type = Command.SlaveCreated,
                            IpEndPoint = ipEndPoint
                        });

                        sender.Send(stream.ToArray());
                    }
                    catch (SerializationException e)
                    {
                        Console.WriteLine("Failed to serialize. Reason: " + e.Message);
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }
            finally
            {
                sender.Shutdown(SocketShutdown.Both);
                sender.Close();
            }
        }

        public void OnStart()
        {
            try
            {
                TokenSource = new CancellationTokenSource();
                CancelToken = TokenSource.Token;
                Task.Factory.StartNew(ListenerFunction, CancelToken);
                Console.WriteLine("Slave succesfully started on: " + ipEndPoint.Address + ":" + ipEndPoint.Port);
                SendCreationMessage();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
            }
        }

        public void OnStop()
        {
            try
            {
                TokenSource.Cancel();
                Console.WriteLine("Slave succesfully stopped");
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
            }
        }

        private void Add(User user)
        {
            userStorage.Add(user);
        }

        private void Delete(int id)
        {
            userStorage.Delete(id);
        }

        private void Delete(User user)
        {
            userStorage.Delete(user);
        }

        private IEnumerable<User> GetAll()
        {
            return userStorage.GetAll();
        }

        private int GetId()
        {
            var serverStatePath = ConfigurationManager.AppSettings["serverStatePathFile"];
            string stateFilePath = AppDomain.CurrentDomain.BaseDirectory + @"..\..\state\" + serverStatePath;

            XDocument xmlXDocument = XDocument.Load(stateFilePath);
            int startId = Convert.ToInt32(xmlXDocument.Root?.Element(XName.Get("lastGeneratedId"))?.Value);

            return startId;
        }

        private void ListenerFunction()
        {
            Socket sListener = new Socket(ipEndPoint.Address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                sListener.Bind(ipEndPoint);
                sListener.Listen(10);

                while (true)
                {
                    Socket handler = sListener.Accept();
                    try
                    {
                        byte[] bytes = new byte[10000];
                        int bytesRec = handler.Receive(bytes);
                        if (bytesRec == 0)
                        {
                            continue;
                        }

                        Message received = null;
                        using (MemoryStream stream = new MemoryStream(bytes))
                        {
                            try
                            {
                                BinaryFormatter formatter = new BinaryFormatter();

                                received = (Message)formatter.Deserialize(stream);

                                switch (received.Type)
                                {
                                    case Command.Add:
                                        userStorage.Add(received.User);
                                        break;
                                    case Command.DeleteById:
                                        userStorage.Delete(received.User.Id);
                                        break;
                                    case Command.DeleteByUser:
                                        userStorage.Delete(received.User);
                                        break;
                                    default:
                                        Console.WriteLine("Not supported command: " + received.Type);
                                        break;
                                }
                            }
                            catch (SerializationException e)
                            {
                                Console.WriteLine("Failed to deserialize. Reason: " + e.StackTrace);
                                throw;
                            }
                        }
                    }
                    finally
                    {
                        handler.Shutdown(SocketShutdown.Both);
                        handler.Close();
                    }
                }
            }
            catch (Exception exception)
            {
                //Logger.Error(exception);
                Console.WriteLine(exception.Message);
            }
            finally
            {
                sListener.Shutdown(SocketShutdown.Both);
                sListener.Close();
            }
        }

    }
}
