
using Exoa.Events;
using Exoa.Maths;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Exoa.Designer
{
    /// <summary>
    /// Represents a menu for handling floor map items in the user interface.
    /// This class manages the creation, duplication, and display of floor map items, as well as their interaction states.
    /// </summary>
    public class UIFloorMapMenu : GenericLeftMenu
    {
        public static UIFloorMapMenu instance;

        [System.Serializable]
        /// <summary>
        /// Represents a menu item in the UIFloorMapMenu.
        /// Contains information about the button, prefab, position, type, and animation spring.
        /// </summary>
        public class MenuItem
        {
            public Button createBtn; // The button linked to the menu item.
            public GameObject UIItemPrefab; // Prefab associated with the menu item.
            public float xPos; // The x position of the button in the menu.
            public DataModel.FloorMapItemType type; // Type of the floor map item.
            public FloatSpring posMoveSpring; // Spring for animating position movement.
        }

        public List<MenuItem> menuItems; // List of menu items to display.

        public float itemOffset = 2; // Offset between items in the menu.
        public ScrollRect scrollRect; // Scrollable area for items.
        public RectTransform containerRect; // Container for the instantiated UI items.
        public Transform sceneContainer; // Container for the scene items.
        private FloorMapSerializer serializer; // Serializer to handle floor map data.
        public Button createButton; // Button to open/create new items.
        public Button exitDrawModeBtn; // Button to exit draw mode.
        public CanvasGroup canvasGroup; // Canvas group to manage transparency and interaction.

        private bool isCreateMenuOpen = true; // Whether the create menu is currently open.
        public Springs createMenuMove; // Springs for menu movement animation.
        public Springs scrollMove; // Springs for scroll movement.
        private FloatSpring scrollSpring; // Spring for controlling scroll position.
        private float targetScrollY; // Target Y position for scrolling.
        public bool autoScroll; // Whether automatic scrolling should occur.

        /// <summary>
        /// Cleans up event subscriptions when the object is destroyed.
        /// </summary>
        void OnDestroy()
        {
            GameEditorEvents.OnRequestClearAll -= OnRequestClearAll;
            AppController.OnAppStateChange -= OnAppStateChange;
        }

        /// <summary>
        /// Initializes the UIFloorMapMenu, setting up event listeners and button actions.
        /// </summary>
        void Awake()
        {
            instance = this;
            serializer = GameObject.FindObjectOfType<FloorMapSerializer>();

            for (int i = 0; i < menuItems.Count; i++)
            {
                MenuItem mi = menuItems[i];
                mi.createBtn.onClick.AddListener(() => CreateNewUIItem(new DataModel.FloorMapItem(), mi.UIItemPrefab, mi.type));
                mi.xPos = mi.createBtn.transform.localPosition.x;
                menuItems[i] = mi;
            }
            createButton.onClick.AddListener(() => ToggleMenu(true));
            exitDrawModeBtn.onClick.AddListener(OnClickExitDrawMode);
            exitDrawModeBtn.gameObject.SetActive(false);

            containerRect.ClearChildren();
            ToggleMenu(false);
        }

        /// <summary>
        /// Starts the UIFloorMapMenu by subscribing to relevant events.
        /// </summary>
        override public void Start()
        {
            base.Start();
            GameEditorEvents.OnRequestClearAll += OnRequestClearAll;
            AppController.OnAppStateChange += OnAppStateChange;
        }

        /// <summary>
        /// Handles the action of exiting draw mode on items.
        /// </summary>
        private void OnClickExitDrawMode()
        {
            DisableDrawModeOnItems();
        }

        /// <summary>
        /// Opens the menu, optionally closing it if specified.
        /// </summary>
        /// <param name="close">Indicates whether to close the menu.</param>
        override public void Open(bool close = true)
        {
            base.Open(close);
            if (isCreateMenuOpen) ToggleMenu();
        }

        /// <summary>
        /// Toggles the visibility of the create menu, allowing for animation if specified.
        /// </summary>
        /// <param name="animate">Indicates whether to animate the menu toggle.</param>
        private void ToggleMenu(bool animate = true)
        {
            isCreateMenuOpen = !isCreateMenuOpen;
            for (int i = 0; i < menuItems.Count; i++)
            {
                if (!animate)
                {
                    float targetX = (isCreateMenuOpen ? menuItems[i].xPos : createButton.transform.localPosition.x);
                    menuItems[i].posMoveSpring.Value = targetX;
                    menuItems[i].createBtn.transform.localPosition = menuItems[i].createBtn.transform.localPosition.SetX(targetX);
                }
            }
        }

        /// <summary>
        /// Duplicates a UI item and places it on the menu, disabling draw mode.
        /// </summary>
        /// <param name="s">The floor map item to duplicate.</param>
        /// <param name="type">The type of item to create.</param>
        public void OnDuplicateUIItem(DataModel.FloorMapItem s, DataModel.FloorMapItemType type)
        {
            DisableDrawModeOnItems();
            CreateNewUIItem(s, GetUIItemPrefabByType(type), type);
            scrollSpring.Value = scrollRect.verticalNormalizedPosition;
            targetScrollY = 0;
            autoScroll = true;
        }

        /// <summary>
        /// Gets the UI item prefab based on the specified item type.
        /// </summary>
        /// <param name="type">The type of the floor map item.</param>
        /// <returns>The corresponding prefab for the specified type, or null if not found.</returns>
        private GameObject GetUIItemPrefabByType(DataModel.FloorMapItemType type)
        {
            for (int i = 0; i < menuItems.Count; i++)
            {
                if (menuItems[i].type == type)
                    return menuItems[i].UIItemPrefab;
            }
            return null;
        }

        /// <summary>
        /// Updates the state of the menu and handles automatic scrolling.
        /// </summary>
        override protected void Update()
        {
            base.Update();

            if (HDInputs.EscapePressed())
            {
                DisableDrawModeOnItems();
            }
            if (autoScroll)
                scrollRect.verticalNormalizedPosition = scrollMove.Update(ref scrollSpring, targetScrollY, OnAutoScrollComplete);

            for (int i = 0; i < menuItems.Count; i++)
            {
                float targetX = isCreateMenuOpen ? menuItems[i].xPos : createButton.transform.localPosition.x;
                float finalX = createMenuMove.Update(ref menuItems[i].posMoveSpring, targetX);
                menuItems[i].createBtn.transform.localPosition = menuItems[i].createBtn.transform.localPosition.SetX(finalX);
            }
        }

        /// <summary>
        /// Completes the automatic scrolling process.
        /// </summary>
        private void OnAutoScrollComplete()
        {
            autoScroll = false;
        }

        /// <summary>
        /// Creates a UI item based on the provided prefab and item type.
        /// </summary>
        /// <param name="prefab">The prefab to instantiate.</param>
        /// <param name="type">The type of the floor map item.</param>
        /// <returns>The newly created UIBaseItem.</returns>
        private UIBaseItem CreateUIItem(GameObject prefab, DataModel.FloorMapItemType type)
        {
            GameObject inst = Instantiate(prefab, containerRect);
            RectTransform r = inst.GetComponent<RectTransform>();
            r.localScale = Vector3.one;
            UIBaseItem plmi = r.GetComponent<UIBaseItem>();
            plmi.sequencingItemType = type;
            return plmi;
        }

        /// <summary>
        /// Creates a new UI item based on the provided sequence and type, using the appropriate prefab.
        /// </summary>
        /// <param name="seq">The floor map item data.</param>
        /// <param name="sequenceType">The type of the sequence.</param>
        internal void CreateNewUIItem(DataModel.FloorMapItem seq, string sequenceType)
        {
            DataModel.FloorMapItemType type = seq.GetItemType();
            CreateNewUIItem(seq, GetUIItemPrefabByType(type), type);
            DisableDrawModeOnItems();
        }

        /// <summary>
        /// Creates a new UI item from the given sequence, prefab, and type while ensuring the project is valid.
        /// </summary>
        /// <param name="seq">The floor map item data.</param>
        /// <param name="prefab">The prefab to use for creating the item.</param>
        /// <param name="type">The type of the floor map item.</param>
        public void CreateNewUIItem(DataModel.FloorMapItem seq, GameObject prefab, DataModel.FloorMapItemType type)
        {
            if (!serializer.IsProjectCreatedOrOpened(true))
            {
                return;
            }
            if (isCreateMenuOpen) ToggleMenu();

            UIBaseItem ui = CreateUIItem(prefab, type);
            ControlPointsController cpc = serializer.CreateSequence();
            IObjectDrawer drawer = null;

            switch (type)
            {
                case DataModel.FloorMapItemType.Door:
                case DataModel.FloorMapItemType.Window:
                case DataModel.FloorMapItemType.Opening:
                    drawer = serializer.CreateOpeningController();
                    break;
                case DataModel.FloorMapItemType.Room:
                    drawer = serializer.CreateRoomController();
                    break;
                case DataModel.FloorMapItemType.Outside:
                    drawer = serializer.CreateOutsideController();
                    break;
            }

            ui.OnDuplicate += OnDuplicateUIItem;
            ui.OnIsolate += OnIsolate;
            ui.OnDrawModeToggled += OnDrawModeToggled;
            ui.cpc = cpc;
            ui.drawer = drawer;
#if FLOORMAP_MODULE
            drawer.Cpc = cpc;
            drawer.UI = ui;
            drawer.Build(seq);
            drawer.Init();
#endif
            if (seq.normalizedPositions != null && seq.normalizedPositions.Count > 0)
            {
                ui.SetData(seq);
                uint index = 0;
                foreach (Vector3 normalizedPosition in seq.normalizedPositions)
                {
                    ControlPoint cp = cpc.CreateControlPointBasedOnNormalizedPosition(normalizedPosition,
                        false,
                        index++, false);
                }
                cpc.CreatePathVisualization();
                cpc.OnControlPointsChanged?.Invoke();

            }
            scrollSpring.Value = scrollRect.verticalNormalizedPosition;
            targetScrollY = 0;
            autoScroll = true;
        }

        /// <summary>
        /// Toggles the draw mode for an item, updating the user interface accordingly.
        /// </summary>
        /// <param name="item">The UIBaseItem being toggled.</param>
        /// <param name="active">Indicates whether the draw mode is active.</param>
        private void OnDrawModeToggled(UIBaseItem item, bool active)
        {
            HDLogger.Log("[UIFloorMapMenu] OnDrawModeToggled active:" + active + " item:" + item.sequencingItemType, HDLogger.LogCategory.Floormap);
            DisableDrawModeOnItems(false, active ? item : null);
        }

        /// <summary>
        /// Retrieves a list of current items data represented in the floor map menu.
        /// </summary>
        /// <returns>A list of FloorMapItem containing data from the UI items.</returns>
        internal List<DataModel.FloorMapItem> GetItemsData()
        {
            List<DataModel.FloorMapItem> list = new List<DataModel.FloorMapItem>();
            for (int i = 0; i < containerRect.childCount; i++)
            {
                UIBaseItem item = containerRect.GetChild(i).GetComponent<UIBaseItem>();
                if (item != null)
                {
                    list.Add(item.GetData());
                }
            }
            return list;
        }

        /// <summary>
        /// Updates the menu UI based on the current application state.
        /// </summary>
        /// <param name="state">The new application state received.</param>
        private void OnAppStateChange(AppController.States state)
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = state == AppController.States.PreviewBuilding ? .4f : 1;
                canvasGroup.interactable = state == AppController.States.PreviewBuilding ? false : true;
            }
        }

        /// <summary>
        /// Handles the request to clear all UI elements based on specified conditions.
        /// </summary>
        /// <param name="clearFloorsUI">Indicates if floors UI should be cleared.</param>
        /// <param name="clearFloorMapUI">Indicates if floor map UI should be cleared.</param>
        /// <param name="clearScene">Indicates if the scene should be cleared.</param>
        private void OnRequestClearAll(bool clearFloorsUI, bool clearFloorMapUI, bool clearScene)
        {
            if (!clearFloorMapUI) return;

            containerRect.ClearChildren();
        }

        private bool isolated; // Indicates whether an item is isolated.
        private UIBaseItem itemIsolated; // The item currently isolated in the menu.

        /// <summary>
        /// Toggles the isolation state of a specific UIBaseItem in the menu.
        /// </summary>
        /// <param name="item">The UIBaseItem to isolate.</param>
        /// <param name="active">Indicates whether to activate isolation.</param>
        public void OnIsolate(UIBaseItem item, bool active)
        {
            if (itemIsolated == null || itemIsolated == item)
            {
                isolated = !isolated;
            }
            else
            {
                isolated = true;
            }
            HideSequences(!isolated);
            itemIsolated = item;
        }

        /// <summary>
        /// Disables draw mode on all items, except for the specified one if provided.
        /// </summary>
        /// <param name="drawMode">Indicates whether to enable or disable draw mode.</param>
        /// <param name="exceptItem">The item to exclude from draw mode changes.</param>
        public void DisableDrawModeOnItems(bool drawMode = false, UIBaseItem exceptItem = null)
        {
            bool disableAll = !drawMode && exceptItem == null;
            HDLogger.Log("[UIFloorMapMenu] DisableDrawModeOnItems drawMode:" + drawMode + " disableAll:" + disableAll, HDLogger.LogCategory.Floormap);
            AppController.Instance.State = !disableAll ? AppController.States.Draw : AppController.States.Idle;
            exitDrawModeBtn.gameObject.SetActive(!disableAll);

            UIBaseItem[] items = GameObject.FindObjectsOfType<UIBaseItem>();
            if (disableAll && items.Length > 0)
            {
                items[0].cpc.HideGhost();
            }
            for (int i = 0; i < items.Length; i++)
            {
                if (items[i] != exceptItem)
                {
                    items[i].ToggleDrawMode(drawMode, false, false);
                }
            }
        }

        /// <summary>
        /// Hides or displays the sequences in the menu based on the passed parameter.
        /// </summary>
        /// <param name="displayed">Indicates whether the sequences should be displayed or hidden.</param>
        public void HideSequences(bool displayed = false)
        {
            for (int i = 0; i < containerRect.childCount; i++)
            {
                UIBaseItem item = containerRect.GetChild(i).GetComponent<UIBaseItem>();
                if (item != null)
                {
                    item.SetDisplayed(displayed);
                }
            }
        }
    }
}
