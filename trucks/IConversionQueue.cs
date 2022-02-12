namespace Trucks
{
    public interface IConversionJobQueue
    {
        void Add(ConvertState state);
    }
}
