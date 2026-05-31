
using Exoa.Events;
using System.Collections.Generic;
using UnityEngine;

namespace Exoa.Designer
{
    /// <summary>
    /// Controls the creation and management of openings (doors/windows) within the design environment.
    /// Implements functionality for drawing, rebuilding, and maintaining the state of openings based on user interactions and events.
    /// </summary>
    public class OpeningController : MonoBehaviour, IObjectDrawer
    {
        /// <summary>
        /// Reference to the control points controller that manages the drawing of paths.
        /// </summary>
        private ControlPointsController cpc;

        /// <summary>
        /// Reference to the UI base item associated with the openings.
        /// </summary>
        private UIBaseItem ui;

        /// <summary>
        /// The specific floor map item represented by this opening controller.
        /// </summary>
        private DataModel.FloorMapItem seq;

        /// <summary>
        /// Color used for drawing the openings.
        /// </summary>
        private Color doorColor;

        /// <summary>
        /// Timestamp of the last rebuild operation.
        /// </summary>
        private float lastRebuild;

        /// <summary>
        /// Delay time between rebuilds to prevent excessive updates.
        /// </summary>
        public float delayBetweenRebuilds = .1f;

        /// <summary>
        /// Indicates whether a rebuild operation has been queued.
        /// </summary>
        private bool queuedRebuild;

        /// <summary>
        /// Indicates whether to queue the send of the reposition openings event.
        /// </summary>
        private bool queuedSendRepositionOpeningsEvent;

        /// <summary>
        /// Prefab used for creating opening instances.
        /// </summary>
        public GameObject openingPrefab;

        /// <summary>
        /// Gets or sets the Control Points Controller.
        /// </summary>
        public ControlPointsController Cpc
        {
            get
            {
                return cpc;
            }
            set
            {
                cpc = value;
            }
        }

        /// <summary>
        /// Gets the GameObject associated with this controller.
        /// </summary>
        public GameObject GO
        {
            get
            {
                return gameObject;
            }
        }

        /// <summary>
        /// Gets or sets the UIBaseItem associated with this controller.
        /// </summary>
        public UIBaseItem UI
        {
            get
            {
                return ui;
            }
            set
            {
                ui = value as UIBaseItem;
            }
        }

        /// <summary>
        /// Gets or sets the drawing color for the doors.
        /// </summary>
        public Color DrawingColor
        {
            get
            {
                return doorColor;
            }
            set
            {
                doorColor = value;
            }
        }

        /// <summary>
        /// Cleans up event subscriptions when this object is destroyed.
        /// </summary>
        void OnDestroy()
        {
            if (cpc != null) cpc.OnPathChanged -= OnPathChanged;
            if (cpc != null) cpc.OnControlPointsChanged -= OnControlPointsChanged;
            if (cpc != null) cpc.OnRequestDrawMode -= OnRequestDrawMode;
            if (ui != null) ui.OnChangeSettings -= OnChangeSettings;
            GameEditorEvents.OnRequestRepositionOpenings -= OnRequestRepositionOpenings;
            GameEditorEvents.OnRequestRebuildAllOpenings -= OnRequestRebuildAllOpenings;
        }

        /// <summary>
        /// Initializes the OpeningController, setting up control points and event listeners.
        /// </summary>
        public void Init()
        {
            if (cpc != null)
            {
                cpc.drawPath = false;
                cpc.drawWidth = true;
                cpc.Width = ui.Width;
                cpc.snapToGrid = true;
                cpc.snapToPathLines = true;
                cpc.OnPathChanged += OnPathChanged;
                cpc.OnControlPointsChanged += OnControlPointsChanged;
                cpc.OnRequestDrawMode += OnRequestDrawMode;
            }
            ui.OnChangeSettings += OnChangeSettings;
            GameEditorEvents.OnRequestRepositionOpenings += OnRequestRepositionOpenings;
            GameEditorEvents.OnRequestRebuildAllOpenings += OnRequestRebuildAllOpenings;
        }

        /// <summary>
        /// Responds to the event that requests rebuilding of all openings.
        /// </summary>
        private void OnRequestRebuildAllOpenings()
        {
            Rebuild(false);
        }

        /// <summary>
        /// Responds to the event that requests repositioning of openings.
        /// </summary>
        private void OnRequestRepositionOpenings()
        {
            cpc.ReSnapControlPoints();
            Rebuild(false);
        }

        /// <summary>
        /// Builds the opening based on the specified FloorMapItem.
        /// </summary>
        /// <param name="s">The FloorMapItem to build the opening with.</param>
        public void Build(DataModel.FloorMapItem s)
        {
            seq = s;
            Rebuild();
        }

        /// <summary>
        /// Responds to draw mode requests from the UI.
        /// </summary>
        /// <param name="request">Whether to toggle draw mode.</param>
        private void OnRequestDrawMode(bool request)
        {
            if (ui != null) ui.ToggleDrawMode(request, true, true);
        }

        /// <summary>
        /// Responds to changes in control points, triggering a rebuild.
        /// </summary>
        private void OnControlPointsChanged()
        {
            Rebuild();
        }

        /// <summary>
        /// Responds to changes in path settings, triggering a rebuild.
        /// </summary>
        private void OnPathChanged()
        {
            Rebuild();
        }

        /// <summary>
        /// Updates the opening based on settings changes from the UI.
        /// </summary>
        /// <param name="s">The FloorMapItem containing updated settings.</param>
        /// <param name="type">The type of the FloorMapItem.</param>
        public void OnChangeSettings(DataModel.FloorMapItem s, DataModel.FloorMapItemType type)
        {
            seq = s;
            Rebuild();
        }

        /// <summary>
        /// Updates the controller state each frame, checking for queued rebuilds.
        /// </summary>
        void Update()
        {
            if (queuedRebuild && lastRebuild < Time.time - delayBetweenRebuilds)
            {
                Rebuild(queuedSendRepositionOpeningsEvent);
            }
        }

        /// <summary>
        /// Rebuilds the openings based on the current configuration and state.
        /// Optionally sends an event to rebuild rooms.
        /// </summary>
        /// <param name="sendRebuildRoomsEvent">Indicates whether to send a rebuild event for rooms.</param>
        private void Rebuild(bool sendRebuildRoomsEvent = true)
        {
            HDLogger.Log("Rebuild Opening sendRebuildRoomsEvent:" + sendRebuildRoomsEvent, HDLogger.LogCategory.Floormap);
            cpc.Width = ui.Width;

            if (lastRebuild > Time.time - delayBetweenRebuilds)
            {
                queuedRebuild = true;
                queuedSendRepositionOpeningsEvent = sendRebuildRoomsEvent;
                return;
            }

            lastRebuild = Time.time;
            queuedRebuild = false;
            queuedSendRepositionOpeningsEvent = false;

            List<Vector3> worldPosList = cpc.GetPointsWorldPositionList();
            List<Vector3> directions = cpc.GetPointsDirectionList();
            if (transform.childCount > worldPosList.Count)
            {
                transform.ClearChildren();
            }
            GameObject inst = null;
            ProceduralOpening po = null;
            for (int j = 0; j < worldPosList.Count; j++)
            {
                if (transform.childCount <= j)
                {
                    inst = Instantiate(openingPrefab);
                    inst.transform.SetParent(transform);
                }
                else
                {
                    inst = transform.GetChild(j).gameObject;
                }
                po = inst.GetComponent<ProceduralOpening>();
                inst.transform.position = worldPosList[j] + directions[j] * ui.Width * .5f;
                inst.transform.rotation = Quaternion.Euler(0, 90, 0);
                if (directions[j] != Vector3.zero)
                    inst.transform.rotation *= Quaternion.LookRotation(directions[j], Vector3.up);

                DataModel.FloorMapItemType type = ui.sequencingItemType;
                float thickness = type == DataModel.FloorMapItemType.Door ? AppController.Instance.doorsThickness : AppController.Instance.windowsThickness;
                po.Generate(type, ui.Width, ui.Height, ui.YPos,
                    ui.HasWindow, ui.WindowFrameSize, ui.WindowSizeH, ui.WindowSizeV,
                    ui.WindowSubDivH, ui.WindowSubDivV,
                    AppController.Instance.wallsHeight, thickness);
            }

            if (sendRebuildRoomsEvent)
                GameEditorEvents.OnRequestRebuildAllRooms?.Invoke();
        }
    }
}
