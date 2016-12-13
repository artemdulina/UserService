using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Entities;
using Service.Interfaces;

namespace Service.Storages
{
    public class XmlStorage : IUserStorage
    {
        private string fileName;
        private readonly string filePath;
        public XmlWriterSettings XmlSettings { get; set; }

        public string FileName
        {
            get
            {
                return fileName;
            }
            private set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentException("Null or empty", nameof(value));
                }

                fileName = value;
            }
        }

        public XmlStorage(string fileName, XmlWriterSettings settings)
        {
            FileName = fileName;
            XmlSettings = settings;
            filePath = AppDomain.CurrentDomain.BaseDirectory + @"..\..\storages\" + FileName;
        }

        public void Add(User user)
        {
            var visarecord = from item in user.VisaRecords
                             select new XElement("visarecord",
                                 new XElement("country", item.Country),
                                 new XElement("start", item.Start),
                                 new XElement("end", item.End));


            XDocument xmlXDocument = XDocument.Load(filePath);
            xmlXDocument.Root?.Add(
          new XElement("user",
              new XElement("id", user.Id),
              new XElement("firstname", user.FirstName),
              new XElement("lastname", user.LastName),
              new XElement("birthday", user.DateOfBirth),
              new XElement("visarecords", visarecord)
              )
              );

            xmlXDocument.Save(filePath);
        }

        public void Delete(int id)
        {
            XDocument xmlXDocument = XDocument.Load(filePath);

            xmlXDocument.Root.Elements("user").FirstOrDefault(element => element.Element("id").Value == id.ToString()).Remove();

            xmlXDocument.Save(filePath);
        }

        public void Delete(User user)
        {
            throw new NotImplementedException();
        }

        public User Search(Func<User, bool> criteria)
        {
            XDocument xmlXDocument = XDocument.Load(filePath);

            var users = xmlXDocument.Root.Elements("user").Select(elem => new User(
                elem.Element("firstname").Value, elem.Element("lastname").Value,
                DateTime.Parse(elem.Element("birthday").Value), Convert.ToInt32(elem.Element("id").Value))
                );

            return users.FirstOrDefault(criteria);
        }

        public IEnumerable<User> GetAll()
        {
            XDocument xmlXDocument = XDocument.Load(filePath);

            var users = xmlXDocument.Root.Elements("user").Select(elem => new User(
                elem.Element("firstname").Value, elem.Element("lastname").Value,
                DateTime.Parse(elem.Element("birthday").Value), Convert.ToInt32(elem.Element("id").Value))
                );

            return users;
        }
    }
}
