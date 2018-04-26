namespace WorldsGame.Saving
{
    // ContainerName is clumsy here, should be better as static method.
    public interface ISaveDataSerializable<T> where T : class, ISaveDataSerializable<T>
    {
        string FileName { get; }

        string ContainerName { get; }

        string Name { get; }

        SaverHelper<T> SaverHelper();

        void Delete();
    }
}