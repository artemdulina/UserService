using System;
using System.Collections.Generic;
using FluentValidation;
using FluentValidation.Results;
using Service.IdGenerators;
using Service.Interfaces;
using System.Configuration;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Service.CustomSections;

namespace Service
{
    public class MasterUserService : IMasterService<User>
    {
        private readonly IUserStorage userStorage;
        private readonly IIdGenerator idGenerator;
        private Task ListnerTask { get; set; }
        private CancellationTokenSource TokenSource { get; set; }
        private CancellationToken CancelToken { get; set; }
        private int port;
        private IPAddress ip;

        public MasterUserService(IUserStorage userStorage) : this(userStorage, new DefaultIdGenerator())
        {

        }

        public MasterUserService(IUserStorage userStorage, IIdGenerator idGenerator)
        {
            this.userStorage = userStorage;
            this.idGenerator = idGenerator;
            idGenerator.SetCurrentId(GetId());
            SetSettingsFromConfig();
        }

        private void SetSettingsFromConfig()
        {
            var masterSlaveConfig = MasterSlavesConfig.GetConfig();

            ip = IPAddress.Parse(masterSlaveConfig.Master.Ip);
            port = int.Parse(masterSlaveConfig.Master.Port);
        }

        public void Add(User user, AbstractValidator<User> validator)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            ValidationResult results = validator.Validate(user);

            if (!results.IsValid)
            {
                throw new ValidationException(results.Errors);
            }

            user.Id = idGenerator.GenerateNext();
            userStorage.Add(user);
        }

        public void Delete(int id)
        {
            userStorage.Delete(id);
        }

        public void Delete(User user)
        {
            userStorage.Delete(user);
        }

        public User Search(Func<User, bool> criteria)
        {
            if (criteria == null)
            {
                throw new ArgumentNullException(nameof(criteria));
            }

            return userStorage.Search(criteria);
        }

        public IEnumerable<User> GetAll()
        {
            return userStorage.GetAll();
        }

        public int GetId()
        {
            var serverStatePath = ConfigurationManager.AppSettings["serverStatePathFile"];
            string stateFilePath = AppDomain.CurrentDomain.BaseDirectory + @"..\..\state\" + serverStatePath;

            XDocument xmlXDocument = XDocument.Load(stateFilePath);
            int startId = Convert.ToInt32(xmlXDocument.Root?.Element(XName.Get("lastGeneratedId"))?.Value);

            return startId;
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

        private void ListenerFunction()
        {
            // Устанавливаем для сокета локальную конечную точку
            IPHostEntry ipHost = Dns.GetHostEntry("localhost");
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint ipEndPoint = new IPEndPoint(ip, port);

            // Создаем сокет Tcp/Ip
            Socket sListener = new Socket(ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

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
                        Message received;
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

        public void SaveState()
        {
            var serverStatePath = ConfigurationManager.AppSettings["serverStatePathFile"];
            string stateFilePath = AppDomain.CurrentDomain.BaseDirectory + @"..\..\state\" + serverStatePath;

            XDocument xmlXDocument = XDocument.Load(stateFilePath);
            var xElement = xmlXDocument.Root?.Element(XName.Get("lastGeneratedId"));
            if (xElement != null)
                xElement.Value = idGenerator.GetCurrentId().ToString();
            xmlXDocument.Save(stateFilePath);
        }
    }
}
