
using Exoa.Events;
using UnityEngine;

namespace Exoa.Designer
{
    /// <summary>
    /// The SpaceController class is responsible for managing the drawing and rebuilding of spaces 
    /// within the designer. It interacts with control points, UI, and scripting events related to 
    /// floor map items.
    /// </summary>
    public class SpaceController : MonoBehaviour, IObjectDrawer
    {
        /// <summary>
        /// Gets the GameObject associated with this SpaceController instance.
        /// </summary>
        public GameObject GO
        {
            get
            {
                return gameObject;
            }
        }

#if FLOORMAP_MODULE
        protected ControlPointsController cpc;  // Reference to the control points controller.
        protected UIBaseItem ui;                 // Reference to the UI base item.
        protected DataModel.FloorMapItem seq;    // The current floor map item being edited.
        protected ProceduralSpace proceduralSpace; // Holds information about procedural space.
        protected Color roomCOlor;               // The color used for drawing the room.

        protected float lastRebuild;             // Time of the last rebuild.
        public float delayBetweenRebuilds = .1f; // Delay between rebuild requests.
        protected bool queuedRebuild;            // Flag to indicate if a rebuild has been queued.
        protected bool queuedSendRepositionOpeningsEvent; // Flag to determine if the reposition event should be sent.

        /// <summary>
        /// Gets or sets the control points controller for the space.
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
        /// Gets or sets the UIBaseItem for the space.
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
        /// Gets or sets the drawing color for the room.
        /// </summary>
        public Color DrawingColor
        {
            get
            {
                return roomCOlor;
            }

            set
            {
                roomCOlor = value;
            }
        }

        /// <summary>
        /// Cleanup and unsubscribe from events when the object is destroyed.
        /// </summary>
        virtual protected void OnDestroy()
        {
            if (cpc != null)
            {
                cpc.OnPathChanged -= OnPathChanged;
                cpc.OnControlPointsChanged -= OnControlPointsChanged;
                cpc.OnRequestDrawMode -= OnRequestDrawMode;
                cpc.OnRequestRemoval -= OnRequestRemoval;
            }
            if (ui != null) ui.OnChangeSettings -= OnChangeSequenceSettings;
            GameEditorEvents.OnRequestRebuildAllRooms -= OnRequestRebuildAllRooms;
        }

        /// <summary>
        /// Initializes the SpaceController, setting up event subscriptions and configurations for control points.
        /// </summary>
        public void Init()
        {
            if (cpc != null)
            {
                cpc.drawPath = true;
                cpc.snapToGrid = true;
                cpc.snapToPathLines = false;
                cpc.OnPathChanged += OnPathChanged;
                cpc.OnControlPointsChanged += OnControlPointsChanged;
                cpc.OnRequestDrawMode += OnRequestDrawMode;
                cpc.OnRequestRemoval += OnRequestRemoval;
            }
            if (ui != null) ui.OnChangeSettings += OnChangeSequenceSettings;
            GameEditorEvents.OnRequestRebuildAllRooms += OnRequestRebuildAllRooms;
        }

        /// <summary>
        /// Called when a rebuild of all rooms is requested, triggering the local rebuild.
        /// </summary>
        protected void OnRequestRebuildAllRooms()
        {
            Rebuild();
        }

        /// <summary>
        /// Builds or rebuilds the space for a specified floor map item.
        /// </summary>
        /// <param name="s">The floor map item to be built.</param>
        public void Build(DataModel.FloorMapItem s)
        {
            seq = s;
            Rebuild();
        }

        /// <summary>
        /// Handles removal requests from the UI.
        /// </summary>
        /// <param name="request">Indicates the removal request status.</param>
        private void OnRequestRemoval(bool request)
        {
            if (ui != null) ui.Delete();
        }

        /// <summary>
        /// Handles draw mode requests from the UI.
        /// </summary>
        /// <param name="request">Indicates the draw mode request status.</param>
        private void OnRequestDrawMode(bool request)
        {
            if (ui != null) ui.ToggleDrawMode(request, true, true);
        }

        /// <summary>
        /// Called when the control points change, triggering a rebuild.
        /// </summary>
        protected void OnControlPointsChanged()
        {
            Rebuild();
        }

        /// <summary>
        /// Called when the path changes, triggering a rebuild with an option to send reposition events.
        /// </summary>
        protected void OnPathChanged()
        {
            Rebuild(true);
        }

        /// <summary>
        /// Adjusts the sequence settings based on the provided floor map item and its type, 
        /// followed by a rebuild.
        /// </summary>
        /// <param name="s">The floor map item with updated settings.</param>
        /// <param name="type">The type of floor map item.</param>
        public void OnChangeSequenceSettings(DataModel.FloorMapItem s, DataModel.FloorMapItemType type)
        {
            seq = s;
            Rebuild();
        }

        /// <summary>
        /// Unity's Update method that checks for queued rebuilds and processes them at a defined interval.
        /// </summary>
        void Update()
        {
            if (queuedRebuild && lastRebuild < Time.time - delayBetweenRebuilds)
            {
                Rebuild(queuedSendRepositionOpeningsEvent);
            }
        }

        /// <summary>
        /// Rebuilds the space, optionally sending reposition openings events. Queues 
        /// the rebuild if it's too soon since the last rebuild.
        /// </summary>
        /// <param name="sendRepositionOpeningsEvent">Whether to send reposition openings event.</param>
        virtual protected void Rebuild(bool sendRepositionOpeningsEvent = false)
        {
            if (lastRebuild > Time.time - delayBetweenRebuilds)
            {
                queuedRebuild = true;
                if (sendRepositionOpeningsEvent)
                    queuedSendRepositionOpeningsEvent = true;
                return;
            }

            if (sendRepositionOpeningsEvent)
            {
                GameEditorEvents.OnRequestRepositionOpenings?.Invoke();
            }

            lastRebuild = Time.time;
            queuedRebuild = false;
            queuedSendRepositionOpeningsEvent = false;

            // TO BE EXTENDED
        }
#endif
    }
}
