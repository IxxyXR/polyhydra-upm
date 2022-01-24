namespace Conway
{
    public enum Ops
    {

        Identity = 0,
        Kis = 1,
        Ambo = 2,
        Zip = 3,
        Expand = 4,

        Bevel = 5,
        Join = 6,
        Needle = 7,
        Ortho = 8,
        Meta = 9,
        Truncate = 10,

        Dual = 11,

        Gyro = 12,
        Snub = 13,
        Subdivide = 14,
        Loft = 15,
        Chamfer = 16,
        Quinto = 17,

        Lace = 18,
        JoinedLace = 19,
        OppositeLace = 20,
        JoinKisKis = 21,
        Stake = 22,
        JoinStake = 23,
        Medial = 24,
        EdgeMedial = 25,
//		JoinedMedial,

        Propeller = 26,
        Whirl = 27,
        Volute = 28,
        Exalt = 29,
        Yank = 30,

        Squall = 31,
        JoinSquall = 32,

        Cross = 33,
        
        SplitFaces = 34,
        Gable = 35,

        Extrude = 36,
        Shell = 37,
        Segment = 79,
        Skeleton = 38,

        FaceOffset = 39,
        FaceScale = 40,
        FaceRotate = 41,
		FaceRotateX = 42,
		FaceRotateY = 43,
        FaceSlide = 44,

        VertexScale = 45,
        VertexRotate = 46,
        VertexFlex = 47,
        VertexStellate = 81,

        // PolarOffset,   TODO
        Hinge = 48,

//		Ribbon,
//		FaceTranslate,

        AddDual = 49,
        AddCopyX = 50,
        AddCopyY = 51,
        AddCopyZ = 52,
        AddMirrorX = 53,
        AddMirrorY = 54,
        AddMirrorZ = 55,

        FaceRemove = 56,
        FaceKeep = 57,
        VertexRemove = 58,
        VertexKeep = 59,
        FillHoles = 60,
        ExtendBoundaries = 61,
        ConnectFaces = 80,
        FaceMerge = 62,
        Weld = 63,

        Recenter = 64,
        SitLevel = 65,
        Stretch = 66,
        Slice = 67,

        ConvexHull = 68,
        Spherize = 69,
        Cylinderize = 70,
        Canonicalize = 71,

        Stack = 72,
        Layer = 73,

        Stash = 74,
        Unstash = 75,
        UnstashToVerts = 76,
        UnstashToFaces = 77,
        TagFaces = 78,

    }
}