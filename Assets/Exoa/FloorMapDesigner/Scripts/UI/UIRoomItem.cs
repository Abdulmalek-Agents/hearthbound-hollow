
namespace Exoa.Designer
{
    /// <summary>
    /// Represents a UI item for a room in the designer.
    /// Inherits basic functionality from UIBaseItem and includes methods for data management
    /// and UI event handling.
    /// </summary>
    public class UIRoomItem : UIBaseItem
    {
        /// <summary>
        /// Called when the object is being destroyed.
        /// Overrides the base class's OnDestroy method to include custom cleanup if needed.
        /// </summary>
        override public void OnDestroy()
        {
            base.OnDestroy();
        }

        /// <summary>
        /// Called when the object is initialized.
        /// Overrides the base class's Start method for additional setup if necessary.
        /// </summary>
        override public void Start()
        {
            base.Start();
        }

        /// <summary>
        /// Handles changes in width. 
        /// This method is invoked when the width changes, triggering a broadcast of the change.
        /// </summary>
        /// <param name="arg0">A string parameter that might represent the new width or related information.</param>
        private void OnChangeWidth(string arg0)
        {
            BroadcastChange(true);
        }

        /// <summary>
        /// Retrieves the data model associated with this UI room item.
        /// Overrides the base class's GetData method to return the data.
        /// </summary>
        /// <returns>A DataModel.FloorMapItem instance representing the current data.</returns>
        override public DataModel.FloorMapItem GetData()
        {
            DataModel.FloorMapItem data = base.GetData();
            return data;
        }

        /// <summary>
        /// Sets the data model for this UI room item.
        /// Overrides the base class's SetData method to apply the provided data.
        /// </summary>
        /// <param name="data">A DataModel.FloorMapItem instance containing the data to set.</param>
        override public void SetData(DataModel.FloorMapItem data)
        {
            base.SetData(data);
        }
    }
}
