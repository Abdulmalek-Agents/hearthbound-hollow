
using Exoa.Events;
using Exoa.Json;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Exoa.Designer
{
    /// <summary>
    /// The BaseSerializable class serves as the base class for deserializing and serializing 
    /// objects in a scene format. It inherits from BaseReader and implements the IDataSerializer 
    /// interface, providing abstract methods that should be implemented in derived classes.
    /// </summary>
    public class BaseSerializable : BaseReader, IDataSerializer
    {
        /// <summary>
        /// Deserializes a given string representation into the scene. 
        /// This method is expected to be overridden in derived classes.
        /// </summary>
        /// <param name="str">The string representation of the scene to deserialize.</param>
        /// <returns>An object representation of the scene.</returns>
        /// <exception cref="NotImplementedException">Thrown when the method is called without being overridden.</exception>
        override public object DeserializeToScene(string str)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Retrieves the file type associated with the serialized data.
        /// This method is expected to be overridden in derived classes.
        /// </summary>
        /// <returns>The file type of the serialized data.</returns>
        /// <exception cref="NotImplementedException">Thrown when the method is called without being overridden.</exception>
        virtual public GameEditorEvents.FileType GetFileType()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the folder name where the serialized data is stored or should be stored.
        /// This method is expected to be overridden in derived classes.
        /// </summary>
        /// <returns>The name of the folder where the serialized data is located.</returns>
        /// <exception cref="NotImplementedException">Thrown when the method is called without being overridden.</exception>
        override public string GetFolderName()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Checks if the scene is empty.
        /// This method is expected to be overridden in derived classes.
        /// </summary>
        /// <returns>True if the scene is empty, otherwise false.</returns>
        /// <exception cref="NotImplementedException">Thrown when the method is called without being overridden.</exception>
        virtual public bool IsSceneEmpty()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Serializes an empty scene with the specified name.
        /// This method is expected to be overridden in derived classes.
        /// </summary>
        /// <param name="name">The name to assign to the serialized empty scene.</param>
        /// <returns>A string representation of the serialized empty scene.</returns>
        /// <exception cref="NotImplementedException">Thrown when the method is called without being overridden.</exception>
        virtual public string SerializeEmpty(string name)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Serializes the current scene to a string format.
        /// This method is expected to be overridden in derived classes.
        /// </summary>
        /// <returns>A string representation of the serialized scene.</returns>
        /// <exception cref="NotImplementedException">Thrown when the method is called without being overridden.</exception>
        virtual public string SerializeScene()
        {
            throw new NotImplementedException();
        }
    }
}
