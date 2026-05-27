
//This class is auto-generated, do not modify (use Tools/TagsLayersEnumBuilder)
namespace Exoa.Designer
{
    /// <summary>
    /// Represents a collection of layer names, their corresponding mask, and number values.
    /// This is an abstract class intended to provide constant values for layers used in the application.
    /// </summary>
    public abstract class Layers
    {
        /// <summary>
        /// The default layer.
        /// </summary>
        public const string Default = "Default";

        /// <summary>
        /// The transparent effect layer.
        /// </summary>
        public const string TransparentFX = "TransparentFX";

        /// <summary>
        /// The layer to ignore raycasts.
        /// </summary>
        public const string IgnoreRaycast = "Ignore Raycast";

        /// <summary>
        /// The water layer.
        /// </summary>
        public const string Water = "Water";

        /// <summary>
        /// The UI layer.
        /// </summary>
        public const string UI = "UI";

        /// <summary>
        /// The effect layer.
        /// </summary>
        public const string FX = "FX";

        /// <summary>
        /// The floor layer.
        /// </summary>
        public const string Floor = "Floor";

        /// <summary>
        /// The wall layer.
        /// </summary>
        public const string Wall = "Wall";

        /// <summary>
        /// The join layer.
        /// </summary>
        public const string Join = "Join";

        /// <summary>
        /// The room box layer.
        /// </summary>
        public const string RoomBox = "RoomBox";

        /// <summary>
        /// The ghost layer.
        /// </summary>
        public const string Ghost = "Ghost";

        /// <summary>
        /// The ceiling layer.
        /// </summary>
        public const string Ceil = "Ceil";

        /// <summary>
        /// The area layer.
        /// </summary>
        public const string Area = "Area";

        /// <summary>
        /// The module layer.
        /// </summary>
        public const string Module = "Module";

        /// <summary>
        /// The exterior wall layer.
        /// </summary>
        public const string ExteriorWall = "ExteriorWall";

        /// <summary>
        /// The roof layer.
        /// </summary>
        public const string Roof = "Roof";

        /// <summary>
        /// The outside layer.
        /// </summary>
        public const string Outside = "Outside";

        /// <summary>
        /// The mask value for the default layer.
        /// </summary>
        public const int DefaultMask = 1;

        /// <summary>
        /// The mask value for the transparent effect layer.
        /// </summary>
        public const int TransparentFXMask = 1 << 1;

        /// <summary>
        /// The mask value for the ignore raycast layer.
        /// </summary>
        public const int IgnoreRaycastMask = 1 << 2;

        /// <summary>
        /// The mask value for the water layer.
        /// </summary>
        public const int WaterMask = 1 << 4;

        /// <summary>
        /// The mask value for the UI layer.
        /// </summary>
        public const int UIMask = 1 << 5;

        /// <summary>
        /// The mask value for the effects layer.
        /// </summary>
        public const int FXMask = 1 << 8;

        /// <summary>
        /// The mask value for the floor layer.
        /// </summary>
        public const int FloorMask = 1 << 9;

        /// <summary>
        /// The mask value for the wall layer.
        /// </summary>
        public const int WallMask = 1 << 10;

        /// <summary>
        /// The mask value for the join layer.
        /// </summary>
        public const int JoinMask = 1 << 11;

        /// <summary>
        /// The mask value for the room box layer.
        /// </summary>
        public const int RoomBoxMask = 1 << 12;

        /// <summary>
        /// The mask value for the ghost layer.
        /// </summary>
        public const int GhostMask = 1 << 13;

        /// <summary>
        /// The mask value for the ceiling layer.
        /// </summary>
        public const int CeilMask = 1 << 14;

        /// <summary>
        /// The mask value for the area layer.
        /// </summary>
        public const int AreaMask = 1 << 15;

        /// <summary>
        /// The mask value for the module layer.
        /// </summary>
        public const int ModuleMask = 1 << 16;

        /// <summary>
        /// The mask value for the exterior wall layer.
        /// </summary>
        public const int ExteriorWallMask = 1 << 17;

        /// <summary>
        /// The mask value for the roof layer.
        /// </summary>
        public const int RoofMask = 1 << 18;

        /// <summary>
        /// The mask value for the outside layer.
        /// </summary>
        public const int OutsideMask = 1 << 19;

        /// <summary>
        /// The number corresponding to the default layer.
        /// </summary>
        public const int DefaultNumber = 0;

        /// <summary>
        /// The number corresponding to the transparent effect layer.
        /// </summary>
        public const int TransparentFXNumber = 1;

        /// <summary>
        /// The number corresponding to the ignore raycast layer.
        /// </summary>
        public const int IgnoreRaycastNumber = 2;

        /// <summary>
        /// The number corresponding to the water layer.
        /// </summary>
        public const int WaterNumber = 4;

        /// <summary>
        /// The number corresponding to the UI layer.
        /// </summary>
        public const int UINumber = 5;

        /// <summary>
        /// The number corresponding to the effects layer.
        /// </summary>
        public const int FXNumber = 8;

        /// <summary>
        /// The number corresponding to the floor layer.
        /// </summary>
        public const int FloorNumber = 9;

        /// <summary>
        /// The number corresponding to the wall layer.
        /// </summary>
        public const int WallNumber = 10;

        /// <summary>
        /// The number corresponding to the join layer.
        /// </summary>
        public const int JoinNumber = 11;

        /// <summary>
        /// The number corresponding to the room box layer.
        /// </summary>
        public const int RoomBoxNumber = 12;

        /// <summary>
        /// The number corresponding to the ghost layer.
        /// </summary>
        public const int GhostNumber = 13;

        /// <summary>
        /// The number corresponding to the ceiling layer.
        /// </summary>
        public const int CeilNumber = 14;

        /// <summary>
        /// The number corresponding to the area layer.
        /// </summary>
        public const int AreaNumber = 15;

        /// <summary>
        /// The number corresponding to the module layer.
        /// </summary>
        public const int ModuleNumber = 16;

        /// <summary>
        /// The number corresponding to the exterior wall layer.
        /// </summary>
        public const int ExteriorWallNumber = 17;

        /// <summary>
        /// The number corresponding to the roof layer.
        /// </summary>
        public const int RoofNumber = 18;

        /// <summary>
        /// The number corresponding to the outside layer.
        /// </summary>
        public const int OutsideNumber = 19;

        /// <summary>
        /// Enumerates the types of layers that can exist.
        /// </summary>
        public enum Type
        {
            /// <summary>
            /// The default layer.
            /// </summary>
            Default = 0,

            /// <summary>
            /// The transparent effect layer.
            /// </summary>
            TransparentFX = 1,

            /// <summary>
            /// The layer that ignores raycasts.
            /// </summary>
            IgnoreRaycast = -1,

            /// <summary>
            /// The water layer.
            /// </summary>
            Water = 4,

            /// <summary>
            /// The UI layer.
            /// </summary>
            UI = 5,

            /// <summary>
            /// The effects layer.
            /// </summary>
            FX = 8,

            /// <summary>
            /// The floor layer.
            /// </summary>
            Floor = 9,

            /// <summary>
            /// The wall layer.
            /// </summary>
            Wall = 10,

            /// <summary>
            /// The join layer.
            /// </summary>
            Join = 11,

            /// <summary>
            /// The room box layer.
            /// </summary>
            RoomBox = 12,

            /// <summary>
            /// The ghost layer.
            /// </summary>
            Ghost = 13,

            /// <summary>
            /// The ceiling layer.
            /// </summary>
            Ceil = 14,

            /// <summary>
            /// The area layer.
            /// </summary>
            Area = 15,

            /// <summary>
            /// The module layer.
            /// </summary>
            Module = 16,

            /// <summary>
            /// The exterior wall layer.
            /// </summary>
            ExteriorWall = 17,

            /// <summary>
            /// The roof layer.
            /// </summary>
            Roof = 18,

            /// <summary>
            /// The outside layer.
            /// </summary>
            Outside = 19
        }
    }
}
