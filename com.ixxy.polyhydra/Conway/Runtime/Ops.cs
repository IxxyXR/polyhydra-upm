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

        Extrude,
        Shell,
        Skeleton,

        VertexScale,
        VertexRotate,
        VertexFlex,

        FaceOffset,
        FaceScale,
        FaceRotate,
        FaceSlide,

        // PolarOffset,   TODO
        Hinge,

//		Ribbon,
//		FaceTranslate,
//		FaceRotateX,
//		FaceRotateY,

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

        Spherize,
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