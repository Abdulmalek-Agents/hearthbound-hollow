using System;

namespace SimpleTalentTreeUi
{
    /// <summary>
    /// Types of point pools that can be consumed by talents.
    /// You can extend this enum to match your game (e.g. Strength, Agility, etc.).
    /// </summary>
    public enum TalentPointType
    {
        Global = 0,
        Dark = 1,
        Fire = 2
    }
}
