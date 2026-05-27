
namespace Exoa.Designer
{
    /// <summary>
    /// Interface for reading data related to scenes.
    /// </summary>
    public interface IDataReader
    {
        /// <summary>
        /// Retrieves the folder name associated with the data reader.
        /// </summary>
        /// <returns>A string representing the folder name.</returns>
        string GetFolderName();

        /// <summary>
        /// Deserializes a given string representation of a scene into an object.
        /// </summary>
        /// <param name="str">The string to deserialize.</param>
        /// <returns>An object representing the deserialized scene.</returns>
        object DeserializeToScene(string str);
    }
}
