
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Exoa.Designer
{
    /// <summary>
    /// BaseReader class is responsible for reading and handling serialized data 
    /// related to scene objects. It implements the IDataReader interface.
    /// </summary>
    public class BaseReader : MonoBehaviour, IDataReader
    {
        /// <summary>
        /// A list of GameObject instances used to manage floor containers in the scene.
        /// </summary>
        protected List<GameObject> floorContainers;

        /// <summary>
        /// Deserializes the provided string into a scene. This method must be overridden
        /// in derived classes.
        /// </summary>
        /// <param name="str">The string to deserialize.</param>
        /// <returns>An object representing the deserialized data.</returns>
        /// <exception cref="NotImplementedException">Always thrown in this base implementation.</exception>
        virtual public object DeserializeToScene(string str)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Retrieves the folder name used for storing scene data. This method must be overridden 
        /// in derived classes.
        /// </summary>
        /// <returns>A string representing the folder name.</returns>
        /// <exception cref="NotImplementedException">Always thrown in this base implementation.</exception>
        virtual public string GetFolderName()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Replaces and loads a specific scene by name. This method is intended to be implemented 
        /// in derived classes as needed.
        /// </summary>
        /// <param name="name">The name of the scene to load.</param>
        /// <param name="sendLoadedEvent">A boolean indicating whether to send a loaded event after loading.</param>
        /// <exception cref="NotImplementedException">Always thrown in this base implementation.</exception>
        virtual public void ReplaceAndLoad(string name, bool sendLoadedEvent = true)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Creates a new floor container GameObject at the specified vertical position.
        /// Searches for a prefab in resources and instantiates it; if not found, 
        /// it attempts to find an existing GameObject.
        /// </summary>
        /// <param name="yPos">The vertical position to place the floor container.</param>
        /// <returns>A FloorController component from the created or found GameObject.</returns>
        protected FloorController CreateFloorContainer(float yPos)
        {
            GameObject prefab = null;
            GameObject floorContainer = null;

            prefab = Resources.Load<GameObject>("Room/FloorController_Prefab");

            if (prefab != null)
            {
                floorContainer = Instantiate(prefab, Vector3.zero, Quaternion.identity, transform);
                floorContainer.name = "FloorContainer";
            }
            else
            {
                floorContainer = GameObject.Find("FloorContainer");
            }
            
            if (floorContainers == null)
                floorContainers = new List<GameObject>();

            floorContainer.transform.localPosition = new Vector3(0, yPos, 0);

            floorContainers.Add(floorContainer);
            return floorContainer.GetComponent<FloorController>();
        }

        /// <summary>
        /// Unloads the currently loaded scene or resources. This method must be 
        /// overridden in derived classes to provide the specific unloading functionality.
        /// </summary>
        /// <exception cref="NotImplementedException">Always thrown in this base implementation.</exception>
        virtual public void Unload()
        {
            throw new NotImplementedException();
        }
    }
}
