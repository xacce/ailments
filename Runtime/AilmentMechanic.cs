using System;

namespace GameReady.Ailments.Runtime
{
    public enum StackMode
    {
        Override,
        Discard,
    }


    public enum AilmentType
    {

    }

    [Flags]
    public enum AilmentTag
    {
        Blind = 1,
        Freeze = 2,
        Bleed = 4,
        Fire = 8
    }
}