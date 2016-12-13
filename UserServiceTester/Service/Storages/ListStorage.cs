using System;
using System.Collections.Generic;
using System.Linq;
using Entities;
using Service.Interfaces;

namespace Service.Storages
{
    public class ListStorage : IUserStorage
    {
        private List<User> storage = new List<User>();

        public void Add(User user)
        {
            storage.Add(user);
        }

        public void Delete(int id)
        {
            storage.RemoveAll(u => u.Id == id);
        }

        public void Delete(User user)
        {
            storage.Remove(user);
        }

        public User GetById(int id)
        {
            return storage.FirstOrDefault(x => x.Id == id);
        }

        public User Search(Func<User, bool> criteria)
        {
            return storage.FirstOrDefault(criteria);
        }

        public IEnumerable<User> GetAll()
        {
            return storage;
        }
    }
}
