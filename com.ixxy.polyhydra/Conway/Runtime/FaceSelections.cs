using System;

namespace Conway
{
    [Serializable]
    public enum FaceSelections
    {
        All,

        // Sides
        ThreeSided,
        FourSided,
        FiveSided,
        SixSided,
        SevenSided,
        EightSided,
        NineSided,
        TenSided,
        ElevenSided,
        TwelveSided,
        PSided,
        QSided,
        EvenSided,
        OddSided,

        // Direction
        FacingUp,
        FacingStraightUp,
        FacingDown,
        FacingStraightDown,
        FacingForward,
        FacingBackward,
        FacingStraightForward,
        FacingStraightBackward,
        FacingLevel,
        FacingCenter,
        FacingIn,
        FacingOut,

        // Role
        Ignored,
        Existing,
        New,
        NewAlt,
        AllNew,

        // Index
        Odd,
        Even,
        OnlyFirst,
        ExceptFirst,
        OnlyLast,
        ExceptLast,
        Random,

        // Edges
        Inner,
        Outer,

        // Distance or position
        TopHalf,

        // Area
        Smaller,
        Larger,

        None,
    }
}