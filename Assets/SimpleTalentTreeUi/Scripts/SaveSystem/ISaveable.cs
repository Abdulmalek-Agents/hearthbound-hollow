namespace SimpleTalentTreeUi
{
    /// <summary>
    /// Interface for components that want to participate in the simple SaveManager system.
    /// </summary>
    public interface ISaveable
    {
        object Save();
        void Load(object data);
    }
}
