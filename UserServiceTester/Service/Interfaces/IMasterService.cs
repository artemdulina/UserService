using System.Collections.Generic;
using FluentValidation;

namespace Service.Interfaces
{
    public interface IMasterService<T> : ISlaveService<T>
    {
        void Add(T user, AbstractValidator<T> validator);

        void Delete(int id);

        void Delete(T user);

        IEnumerable<T> GetAll();
    }
}
