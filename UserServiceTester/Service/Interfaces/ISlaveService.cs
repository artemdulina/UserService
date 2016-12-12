using System;

namespace Service.Interfaces
{
    public interface ISlaveService<out T>
    {
        T Search(Func<T, bool> criteria);
    }
}
