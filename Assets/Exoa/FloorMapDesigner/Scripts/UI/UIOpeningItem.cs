
namespace Exoa.Designer
{
    /// <summary>
    /// Represents a UI item for openings in the designer. 
    /// Inherits from UIBaseItem and includes methods for destruction, initialization, 
    /// data handling, and responding to width changes.
    /// </summary>
    public class UIOpeningItem : UIBaseItem
    {
        /// <summary>
        /// Handles the cleanup of resources when the UIOpeningItem is destroyed.
        /// Calls base class's OnDestroy method.
        /// </summary>
        override public void OnDestroy()
        {
            base.OnDestroy();
        }

        /// <summary>
        /// Initializes the UIOpeningItem.
        /// Calls base class's Start method for any necessary setup.
        /// </summary>
        override public void Start()
        {
            base.Start();
        }

        /// <summary>
        /// Responds to changes in width by broadcasting change notifications.
        /// 
        /// <param name="arg0">A string representing the new width value (not used in current implementation).</param>
        /// </summary>
        private void OnChangeWidth(string arg0)
        {
            BroadcastChange(true);
        }

        /// <summary>
        /// Retrieves the data associated with this UIOpeningItem.
        /// Calls the base class's GetData method to obtain the data.
        /// 
        /// <returns>A FloorMapItem object containing the data.</returns>
        /// </summary>
        override public DataModel.FloorMapItem GetData()
        {
            DataModel.FloorMapItem data = base.GetData();
            return data;
        }

        /// <summary>
        /// Sets the data for this UIOpeningItem.
        /// Calls the base class's SetData method to update the data.
        /// 
        /// <param name="data">A FloorMapItem object containing the data to be set.</param>
        /// </summary>
        override public void SetData(DataModel.FloorMapItem data)
        {
            base.SetData(data);
        }
    }
}
