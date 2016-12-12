using Service.Interfaces;

namespace Service.IdGenerators
{
    public class DefaultIdGenerator : IIdGenerator
    {
        private int previousId;

        public int GenerateNext()
        {
            return ++previousId;
        }

        public int GetCurrentId()
        {
            return previousId;
        }

        public void SetCurrentId(int id)
        {
            previousId = id;
        }
    }
}
