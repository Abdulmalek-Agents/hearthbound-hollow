
using Exoa.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Exoa.Designer
{
    /// <summary>
    /// Base class for UI items within the floor map designer.
    /// Manages UI elements such as buttons, sliders, and toggles related to a floor map item.
    /// </summary>
    public class UIBaseItem : MonoBehaviour
    {
        public DataModel.FloorMapItemType sequencingItemType;
        public Button displayBtn;
        public Button delBtn;
        public Button duplicateBtn;
        public Button isolateBtn;
        public Button horizontalMirrorBtn;
        public Button verticalMirrorBtn;
        public Button drawBtn;

        public TMP_InputField nameInput;
        public Slider widthInput;
        public Slider heightInput;
        public Slider yposInput;
        public Toggle hasWindowToggle;
        public Slider windowFrameSizeInput;
        public Slider windowSizeHInput;
        public Slider windowSizeVInput;
        public Slider windowSubDivHInput;
        public Slider windowSubDivVInput;

        protected Image img;
        public bool displayed = true;
        public bool folded = true;
        public bool drawModeEnabled = true;
        public Color colorDisplayed;
        public Color colorHidden;
        public ControlPointsController cpc;
        public IObjectDrawer drawer;

        public delegate void OnSequenceItemHandler(DataModel.FloorMapItem data, DataModel.FloorMapItemType type);
        public delegate void OnSequenceItemUIHandler(UIBaseItem item, bool active);

        public OnSequenceItemHandler OnChangeSettings;
        public OnSequenceItemHandler OnDuplicate;
        public OnSequenceItemUIHandler OnIsolate;
        public OnSequenceItemUIHandler OnDrawModeToggled;

        /// <summary>
        /// Gets or sets the name of the item.
        /// </summary>
        public string Name
        {
            get
            {
                if (nameInput != null)
                    return nameInput.text;
                return null;
            }
            set
            {
                if (nameInput != null) nameInput.text = value;
            }
        }

        /// <summary>
        /// Gets or sets the Y position of the item.
        /// </summary>
        public float YPos
        {
            get
            {
                return yposInput != null ? yposInput.value : 1;
            }
            set
            {
                if (yposInput != null) yposInput.value = value;
            }
        }

        /// <summary>
        /// Gets or sets the height of the item.
        /// </summary>
        public float Height
        {
            get
            {
                return heightInput != null ? heightInput.value : 1;
            }
            set
            {
                if (heightInput != null) heightInput.value = value;
            }
        }

        /// <summary>
        /// Gets or sets the width of the item.
        /// </summary>
        public float Width
        {
            get { return widthInput != null ? widthInput.value : 1; }
            set { if (widthInput != null) widthInput.value = value; }
        }

        /// <summary>
        /// Gets or sets whether the item has a window.
        /// </summary>
        public bool HasWindow
        {
            get { return hasWindowToggle != null ? hasWindowToggle.isOn : false; }
            set { if (hasWindowToggle != null) hasWindowToggle.isOn = value; }
        }

        /// <summary>
        /// Gets or sets the window frame size.
        /// </summary>
        public float WindowFrameSize
        {
            get { return windowFrameSizeInput != null ? windowFrameSizeInput.value : 0; }
            set { if (windowFrameSizeInput != null) windowFrameSizeInput.value = value; }
        }

        /// <summary>
        /// Gets or sets the horizontal window size.
        /// </summary>
        public float WindowSizeH
        {
            get { return windowSizeHInput != null ? windowSizeHInput.value : 0; }
            set { if (windowSizeHInput != null) windowSizeHInput.value = value; }
        }

        /// <summary>
        /// Gets or sets the vertical window size.
        /// </summary>
        public float WindowSizeV
        {
            get { return windowSizeVInput != null ? windowSizeVInput.value : 0; }
            set { if (windowSizeVInput != null) windowSizeVInput.value = value; }
        }

        /// <summary>
        /// Gets or sets the number of horizontal subdivisions of the window.
        /// </summary>
        public int WindowSubDivH
        {
            get { return windowSubDivHInput != null ? Mathf.RoundToInt(windowSubDivHInput.value) : 0; }
            set { if (windowSubDivHInput != null) windowSubDivHInput.value = value; }
        }

        /// <summary>
        /// Gets or sets the number of vertical subdivisions of the window.
        /// </summary>
        public int WindowSubDivV
        {
            get { return windowSubDivVInput != null ? Mathf.RoundToInt(windowSubDivVInput.value) : 0; }
            set { if (windowSubDivVInput != null) windowSubDivVInput.value = value; }
        }

        /// <summary>
        /// Cleans up button listeners and events before the object is destroyed.
        /// </summary>
        virtual public void OnDestroy()
        {
            delBtn?.onClick.RemoveAllListeners();
            displayBtn?.onClick.RemoveAllListeners();
            duplicateBtn?.onClick.RemoveAllListeners();
            isolateBtn?.onClick.RemoveAllListeners();
            horizontalMirrorBtn?.onClick.RemoveAllListeners();
            verticalMirrorBtn?.onClick.RemoveAllListeners();
            drawBtn?.onClick.RemoveAllListeners();

            nameInput?.onEndEdit.RemoveAllListeners();
            widthInput?.onValueChanged.RemoveAllListeners();
            heightInput?.onValueChanged.RemoveAllListeners();
            yposInput?.onValueChanged.RemoveAllListeners();

            OnChangeSettings = null;
            OnDuplicate = null;
            OnIsolate = null;
        }

        /// <summary>
        /// Initializes button listeners and configures the UI on startup.
        /// </summary>
        virtual public void Start()
        {
            img = GetComponent<Image>();

            delBtn?.onClick.AddListener(OnClickDelete);
            displayBtn?.onClick.AddListener(OnClickDisplay);
            duplicateBtn?.onClick.AddListener(OnClickDuplicate);
            isolateBtn?.onClick.AddListener(OnClickIsolate);
            horizontalMirrorBtn?.onClick.AddListener(() => cpc.Mirror(true));
            verticalMirrorBtn?.onClick.AddListener(() => cpc.Mirror(false));
            drawBtn?.onClick.AddListener(OnClickDraw);

            nameInput?.onEndEdit.AddListener(OnChangeName);
            widthInput?.onValueChanged.AddListener(OnChangeDimensions);
            heightInput?.onValueChanged.AddListener(OnChangeDimensions);
            yposInput?.onValueChanged.AddListener(OnChangeDimensions);
            hasWindowToggle?.onValueChanged.AddListener(OnChangeHasWindowToggle);
            windowFrameSizeInput?.onValueChanged.AddListener(OnChangeDimensions);
            windowSizeHInput?.onValueChanged.AddListener(OnChangeDimensions);
            windowSizeVInput?.onValueChanged.AddListener(OnChangeDimensions);
            windowSubDivHInput?.onValueChanged.AddListener(OnChangeDimensions);
            windowSubDivVInput?.onValueChanged.AddListener(OnChangeDimensions);

            // Initialize colors if ControlPointsController is set
            if (cpc != null)
            {
                colorDisplayed = cpc.pathColor;
#if FLOORMAP_MODULE
                drawer.DrawingColor = cpc.pathColor;
#endif
            }
            SetDisplayed(true);
            ToggleDrawMode(drawModeEnabled, true, false);
        }

        /// <summary>
        /// Handles the toggle for the "Has Window" checkbox.
        /// </summary>
        private void OnChangeHasWindowToggle(bool arg0)
        {
            BroadcastChange(true);
        }

        /// <summary>
        /// Handles changes in dimensions of the item.
        /// </summary>
        private void OnChangeDimensions(float arg0)
        {
            BroadcastChange(true);
        }

        /// <summary>
        /// Handles changes in subdivisions.
        /// </summary>
        private void OnChangeSubDiv(int arg0)
        {
            BroadcastChange(true);
        }

        /// <summary>
        /// Toggles the drawing mode for the UI item.
        /// </summary>
        /// <param name="active">Whether drawing mode is active.</param>
        /// <param name="sendEvent">Whether to send an event after toggling.</param>
        /// <param name="hideGhost">Whether to hide the ghost object.</param>
        virtual public void ToggleDrawMode(bool active, bool sendEvent, bool hideGhost)
        {
            HDLogger.Log("[UIBaseItem] SetDrawModeEnabled drawMode:" + active, HDLogger.LogCategory.Floormap);

            drawModeEnabled = active;
            cpc.IsDrawing = active;
            drawBtn.GetComponentInChildren<TMP_Text>().text = drawModeEnabled ? "DRAW ENABLED" : "DRAW DISABLED";

            if (sendEvent)
                OnDrawModeToggled?.Invoke(this, active);
            if (!active && hideGhost)
                cpc.HideGhost();
        }

        /// <summary>
        /// Handles the click event for the draw button.
        /// </summary>
        virtual public void OnClickDraw()
        {
            drawModeEnabled = !drawModeEnabled;
            ToggleDrawMode(drawModeEnabled, true, true);
        }

        /// <summary>
        /// Handles the isolate button click and invokes the isolate event.
        /// </summary>
        private void OnClickIsolate()
        {
            OnIsolate?.Invoke(this, true);
            SetDisplayed(true);
        }

        /// <summary>
        /// Handles the duplicate button click and invokes the duplicate event.
        /// </summary>
        private void OnClickDuplicate()
        {
            OnDuplicate?.Invoke(GetData(), sequencingItemType);
        }

        /// <summary>
        /// Handles changes to the name input.
        /// </summary>
        /// <param name="arg0">The new name input.</param>
        private void OnChangeName(string arg0)
        {
            //BroadcastChange(true);
        }

        /// <summary>
        /// Broadcasts changes related to the item settings.
        /// </summary>
        /// <param name="resetPlayback">Indicates if playback should be reset.</param>
        protected void BroadcastChange(bool resetPlayback)
        {
            OnChangeSettings?.Invoke(GetData(), sequencingItemType);
        }

        /// <summary>
        /// Handles the display toggle button click.
        /// </summary>
        protected void OnClickDisplay()
        {
            displayed = !displayed;

            SetDisplayed(displayed);
        }

        /// <summary>
        /// Sets whether the item is displayed or hidden.
        /// </summary>
        /// <param name="v">True to display, false to hide.</param>
        public void SetDisplayed(bool v)
        {
            displayed = v;
            img.color = displayed ? colorDisplayed : colorHidden;
            cpc?.gameObject.SetActive(displayed);
            drawer.GO.SetActive(displayed);
        }

        /// <summary>
        /// Handles the delete button click and shows a confirmation alert.
        /// </summary>
        protected void OnClickDelete()
        {
            AlertPopup p = AlertPopup.ShowAlert("confirm", "Confirm?", "Do you really want to delete this sequence?", true, "Cancel");
            if (p != null)
                p.OnClickOKEvent.AddListener(Delete);
        }

        /// <summary>
        /// Deletes the item and cleans up resources.
        /// </summary>
        public void Delete()
        {
            cpc.transform.ClearChildren();
            cpc.gameObject.DestroyUniversal();

            drawer.GO.DestroyUniversal();

            GameEditorEvents.OnRequestRebuildAllRooms?.Invoke();

            gameObject.DestroyUniversal();
        }

        /// <summary>
        /// Converts the current UI item data to a FloorMapItem data model.
        /// </summary>
        /// <returns>A FloorMapItem containing the current item's data.</returns>
        virtual public DataModel.FloorMapItem GetData()
        {
            DataModel.FloorMapItem data = new DataModel.FloorMapItem();
            data.name = Name;
            data.width = Width;
            data.height = Height;
            data.ypos = YPos;
            data.hasWindow = HasWindow;
            data.windowFrameSize = WindowFrameSize;
            data.windowSizeH = WindowSizeH;
            data.windowSizeV = WindowSizeV;
            data.windowSubDivH = WindowSubDivH;
            data.windowSubDivV = WindowSubDivV;

            data.type = sequencingItemType.ToString();
            data.normalizedPositions = new List<Vector3>();

            if (cpc != null)
            {
                ControlPoint[] cps = cpc.transform.GetComponentsInChildren<ControlPoint>();
                List<ControlPoint> cpsList = new List<ControlPoint>(cps);
                cpsList = cps.OrderBy(s => s.transform.GetSiblingIndex()).ToList();
                data.directions = cpc.GetPointsDirectionList();
                data.normalizedPositions = cpc.GetNormalizedPositionList();
            }

            return data;
        }

        /// <summary>
        /// Sets the data for the UI item from a FloorMapItem data model.
        /// </summary>
        /// <param name="data">The data model containing properties to set.</param>
        virtual public void SetData(DataModel.FloorMapItem data)
        {
            Enum.TryParse<DataModel.FloorMapItemType>(data.type, out sequencingItemType);
            Name = data.name;
            Width = data.width;
            Height = data.height;
            YPos = data.ypos;

            HasWindow = data.hasWindow;
            WindowFrameSize = data.windowFrameSize;
            WindowSizeH = data.windowSizeH;
            WindowSizeV = data.windowSizeV;
            WindowSubDivH = data.windowSubDivH;
            WindowSubDivV = data.windowSubDivV;
        }
    }
}
