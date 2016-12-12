using System;
using System.Collections.Generic;

namespace Service.Interfaces
{
    public interface IUserStorage
    {
        void Add(User user);
        void Delete(int id);
        void Delete(User user);
        User Search(Func<User, bool> criteria);
        IEnumerable<User> GetAll();
    }
}
