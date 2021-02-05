namespace Conway
{
    public enum Ops
    {

        Identity,
        Kis,
        Ambo,
        Zip,
        Expand,

        Bevel,
        Join,
        Needle,
        Ortho,
        Meta,
        Truncate,

        Dual,

        Gyro,
        Snub,
        Subdivide,
        Loft,
        Chamfer,
        Quinto,

        Lace,
        JoinedLace,
        OppositeLace,
        JoinKisKis,
        Stake,
        JoinStake,
        Medial,
        EdgeMedial,
//		JoinedMedial,

        Propeller,
        Whirl,
        Volute,
        Exalt,
        Yank,

        Squall,
        JoinSquall,

        Cross,
        
        SplitFaces,
        Gable,

        Extrude,
        Shell,
        Skeleton,

        FaceOffset,
        FaceScale,
        FaceRotate,
		FaceRotateX,
		FaceRotateY,
        FaceSlide,

        VertexScale,
        VertexRotate,
        VertexFlex,

        // PolarOffset,   TODO
        Hinge,

//		Ribbon,
//		FaceTranslate,

        AddDual,
        AddCopyX,
        AddCopyY,
        AddCopyZ,
        AddMirrorX,
        AddMirrorY,
        AddMirrorZ,

        FaceRemove,
        FaceKeep,
        VertexRemove,
        VertexKeep,
        FillHoles,
        ExtendBoundaries,
        FaceMerge,
        Weld,

        Recenter,
        SitLevel,
        Stretch,
        Slice,

        ConvexHull,
        Spherize,
        Cylinderize,
        Canonicalize,

        Stack,
        Layer,

        Stash,
        Unstash,
        UnstashToVerts,
        UnstashToFaces,
        TagFaces,

    }
}