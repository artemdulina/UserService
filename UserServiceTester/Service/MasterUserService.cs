﻿using System;
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
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Entities;
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
        private IPEndPoint ipEndPoint;
        private List<IPEndPoint> slavesIpEndPoints = new List<IPEndPoint>();

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
            MasterSlavesConfig masterSlaveConfig = MasterSlavesConfig.GetConfig();

            IPAddress ip = IPAddress.Parse(masterSlaveConfig.Master.Ip);
            int port = int.Parse(masterSlaveConfig.Master.Port);
            ipEndPoint = new IPEndPoint(ip, port);
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

            SendNotificationToAllSlaves(new Message()
            {
                Type = Command.Add,
                User = user
            });
        }

        public void Delete(int id)
        {
            userStorage.Delete(id);

            SendNotificationToAllSlaves(new Message()
            {
                Type = Command.DeleteById,
                User = new User { Id = id }
            });
        }

        public void Delete(User user)
        {
            userStorage.Delete(user);

            SendNotificationToAllSlaves(new Message()
            {
                Type = Command.DeleteByUser,
                User = user
            });
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

        private void SendNotificationToAllSlaves(Message message)
        {


            using (MemoryStream stream = new MemoryStream())
            {
                try
                {
                    BinaryFormatter formatter = new BinaryFormatter();

                    formatter.Serialize(stream, message);
                    foreach (var slave in slavesIpEndPoints)
                    {
                        using (Socket sender = new Socket(ipEndPoint.Address.AddressFamily, SocketType.Stream, ProtocolType.Tcp))
                        {
                            sender.Connect(slave.Address, slave.Port);
                            sender.Send(stream.ToArray());
                        }
                    }
                }
                catch (SerializationException e)
                {
                    Console.WriteLine("Failed to serialize. Reason: " + e.Message);
                    throw;
                }
            }

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
                Console.WriteLine("Master succesfully started on: " + ipEndPoint.Address + ":" + ipEndPoint.Port);
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
                Console.WriteLine("Master succesfully stopped");
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
            }
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
                        //Console.WriteLine(bytesRec);

                        using (MemoryStream stream = new MemoryStream(bytes))
                        {
                            try
                            {
                                BinaryFormatter formatter = new BinaryFormatter();
                                //formatter.Binder = new AllAssemblyVersionsDeserializationBinder();
                                formatter.AssemblyFormat = FormatterAssemblyStyle.Simple;
                                Message received = (Message)formatter.Deserialize(stream);
                                //Console.WriteLine("'''''''''''''''''''''''''''''''''''");

                                if (received.Type == Command.SlaveCreated)
                                {
                                    slavesIpEndPoints.Add(received.IpEndPoint);
                                    Console.WriteLine("slave " + received.IpEndPoint.Address + ":" + received.IpEndPoint.Port + " was created");
                                }
                                else if (received.Type == Command.Search)
                                {
                                    User found = userStorage.Search(received.Criteria);
                                    //Console.WriteLine(found);

                                    Message msgToSend = new Message()
                                    {
                                        User = found,
                                        Type = Command.Search
                                    };

                                    using (MemoryStream streamSend = new MemoryStream())
                                    {
                                        formatter.Serialize(streamSend, msgToSend);
                                        handler.Send(streamSend.ToArray());
                                    }
                                    Console.WriteLine("One of the slaves requested user search");
                                }
                            }
                            catch (SerializationException e)
                            {
                                Console.WriteLine("Serialization failed in Master. Reason: " + e.Message);
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
                Console.WriteLine(exception.Message);
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
