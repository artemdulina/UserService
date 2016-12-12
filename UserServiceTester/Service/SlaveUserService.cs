using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using FluentValidation;
using FluentValidation.Results;
using Service.IdGenerators;
using Service.Interfaces;

namespace Service
{
    public class SlaveUserService : ISlaveService<User>
    {
        private readonly IUserStorage userStorage;
        private Task ListnerTask { get; set; }
        private CancellationTokenSource TokenSource { get; set; }
        private CancellationToken CancelToken { get; set; }
        private int MasterPort { get; }

        public SlaveUserService(IUserStorage userStorage, int masterPort)
        {
            this.userStorage = userStorage;
            MasterPort = masterPort;
            //SendCreationMessage();
        }

        private void SendMessage()
        {
            IPHostEntry ipHost = Dns.GetHostEntry("localhost");
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint ipEndPoint = new IPEndPoint(ipAddr, MasterPort);

            Socket sender = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            using (MemoryStream stream = new MemoryStream())
            {
                try
                {
                    BinaryFormatter formatter = new BinaryFormatter();

                    formatter.Serialize(stream, new Message().Type = Command.SlaveCreated);

                    sender.Send(stream.ToArray());
                }
                catch (SerializationException e)
                {
                    Console.WriteLine("Failed to deserialize. Reason: " + e.Message);
                    throw;
                }
            }

        }

        public void OnStart()
        {
            try
            {
                TokenSource = new CancellationTokenSource();
                CancelToken = TokenSource.Token;
                Task.Factory.StartNew(ListenerFunction, CancelToken);
                Console.WriteLine("Master succesfully started");
            }
            catch (Exception exception)
            {

            }
        }

        public void OnStop()
        {
            try
            {
                TokenSource.Cancel();
                Console.WriteLine("Master succesfully stopped");
            }
            catch (Exception exception)
            {

            }
        }

        public User Search(Func<User, bool> criteria)
        {
            if (criteria == null)
            {
                throw new ArgumentNullException(nameof(criteria));
            }

            return userStorage.Search(criteria);
        }

        private void Add(User user, IValidator<User> validator)
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
            // Устанавливаем для сокета локальную конечную точку
            IPHostEntry ipHost = Dns.GetHostEntry("localhost");
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint ipEndPoint = new IPEndPoint(ipAddr, MasterPort);

            // Создаем сокет Tcp/Ip
            Socket sListener = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            // Назначаем сокет локальной конечной точке и слушаем входящие сокеты
            try
            {
                sListener.Bind(ipEndPoint);
                sListener.Listen(10);

                // Начинаем слушать соединения
                while (true)
                {
                    // Программа приостанавливается, ожидая входящее соединение
                    Socket handler = sListener.Accept();
                    try
                    {
                        string data = null;

                        // Мы дождались клиента, пытающегося с нами соединиться

                        byte[] bytes = new byte[1024];
                        int bytesRec = handler.Receive(bytes);

                        data += Encoding.UTF8.GetString(bytes, 0, bytesRec);

                        // Показываем данные на консоли
                        Message received = null;
                        using (MemoryStream stream = new MemoryStream(bytes))
                        {
                            try
                            {
                                BinaryFormatter formatter = new BinaryFormatter();

                                received = (Message)formatter.Deserialize(stream);

                                Console.WriteLine(received.ToString());
                            }
                            catch (SerializationException e)
                            {
                                Console.WriteLine("Failed to deserialize. Reason: " + e.Message);
                                throw;
                            }
                        }

                        // Отправляем ответ клиенту
                        //byte[] msg = Encoding.UTF8.GetBytes(received.ToString());
                        //handler.Send(msg);

                        if (data.IndexOf("<TheEnd>", StringComparison.Ordinal) > -1)
                        {
                            break;
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
            }
            finally
            {
                sListener.Shutdown(SocketShutdown.Both);
                sListener.Close();
            }
        }

    }
}
