namespace Service.Interfaces
{
    public interface IIdGenerator
    {
        int GenerateNext();
        int GetCurrentId();
        void SetCurrentId(int id);
    }
}
