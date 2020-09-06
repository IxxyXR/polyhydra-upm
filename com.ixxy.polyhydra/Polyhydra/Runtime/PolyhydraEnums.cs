using System.Collections.Generic;
using Conway;


public static class PolyHydraEnums
{
	public enum ColorMethods
    {
        BySides,
        ByRole,
        ByFaceDirection,
        ByTags,
    }

	public enum PolyTypeCategories
	{
		All,
		Platonic,
		Prismatic,
		Archimedean,
		KeplerPoinsot,
		Convex,
		Star,
	}


	public enum ShapeTypes
	{
		Uniform,
		Grid,
		Johnson,
		Waterman,
		Other
	}

	public enum GridShapes
	{
		Plane,
		Torus,
		Cylinder,
		Cone,
		Sphere,
//		Conic_Frustum,
//		Mobius,
//		Torus_Trefoil,
//		Klein,
//		Klein2,
//		Roman,
//		Roman_Boy,
//		Cross_Cap,
//		Cross_Cap2,
	}

	public enum JohnsonPolyTypes
    {
        Prism,
        Antiprism,

        Pyramid,
        ElongatedPyramid,
        GyroelongatedPyramid,

        Dipyramid,
        ElongatedDipyramid,
        GyroelongatedDipyramid,

        Cupola,
        ElongatedCupola,
        GyroelongatedCupola,

        OrthoBicupola,
        GyroBicupola,
        ElongatedOrthoBicupola,
        ElongatedGyroBicupola,
        GyroelongatedBicupola,

        Rotunda,
        ElongatedRotunda,
        GyroelongatedRotunda,
        GyroelongatedBirotunda,
    }

	public enum GridTypes
	{
		Square,
		Isometric,
		Hex,
		Polar,

		U_3_6_3_6,
		U_3_3_3_4_4,
		U_3_3_4_3_4,
//		U_3_3_3_3_6, TODO Fix
		U_3_12_12,
		U_4_8_8,
		U_3_4_6_4,
		U_4_6_12,
	}

	public enum OtherPolyTypes
	{
		Polygon,
		UvSphere,
		UvHemisphere,
		GriddedCube,

		C_Shape,
		L_Shape,
		L_Alt_Shape,
		H_Shape,
	}

    public class OpConfig
    {
	    public bool usesAmount = true;
	    public float amountDefault = 0;
	    public float amountMin = -20;
	    public float amountMax = 20;
	    public float amountSafeMin = -10;
	    public float amountSafeMax = 0.999f;
	    public bool usesAmount2 = false;
	    public float amount2Default = 0;
	    public float amount2Min = -20;
	    public float amount2Max = 20;
	    public float amount2SafeMin = -10;
	    public float amount2SafeMax = 0.999f;
	    public bool usesFaces = false;
	    public bool usesRandomize = false;
	    public FaceSelections faceSelection = FaceSelections.All;
    }

	public static Dictionary<Ops, OpConfig> OpConfigs = new Dictionary<Ops, OpConfig>
	{
		{Ops.Identity, new OpConfig {usesAmount = false}},
		{
			Ops.Kis,
			new OpConfig
			{
				usesFaces = true,
				amountDefault = 0.1f,
				amountMin = -6, amountMax = 6, amountSafeMin = -0.5f, amountSafeMax = 0.999f,
				usesRandomize = true
			}
		},
		{Ops.Dual, new OpConfig {usesAmount = false}},
		{Ops.Ambo, new OpConfig {usesAmount = false}},
		{
			Ops.Zip,
			new OpConfig
			{
				amountDefault = 0.5f,
				amountMin = -2f, amountMax = 2f, amountSafeMin = 0.0001f, amountSafeMax = .999f,
				usesRandomize = true
			}
		},
		{
			Ops.Expand,
			new OpConfig
			{
				amountDefault = 0.5f,
				amountMin = -4, amountMax = 4, amountSafeMin = 0.001f, amountSafeMax = 0.999f
			}
		},
		{
			Ops.Bevel,
			new OpConfig
			{
				amountDefault = 0.25f,
				amountMin = -6, amountMax = 6, amountSafeMin = 0.001f, amountSafeMax = 0.4999f,
				usesAmount2 = true,
				amount2Default = 0.25f,
				amount2Min = -6, amount2Max = 6, amount2SafeMin = 0.001f, amount2SafeMax = 0.4999f,
				usesRandomize = true
			}
		},
		{
			Ops.Join,
			new OpConfig
			{
				amountDefault = 0.5f,
				amountMin = -1f, amountMax = 2f, amountSafeMin = -0.5f, amountSafeMax = 0.999f
			}
		},
		{ // TODO Support random
			Ops.Needle,
			new OpConfig
			{
				amountDefault = 0f,
				amountMin = -6, amountMax = 6, amountSafeMin = -0.5f, amountSafeMax = 0.5f,
				usesRandomize = true
			}
		},
		{
			Ops.Ortho,
			new OpConfig
			{
				amountDefault = 0.1f,
				amountMin = -6, amountMax = 6, amountSafeMin = -0.5f, amountSafeMax = 0.999f,
				usesRandomize = true
			}
		},
		{
			Ops.Meta,
			new OpConfig
			{
				amountDefault = 0f,
				amountMin = -6, amountMax = 6, amountSafeMin = -0.333f, amountSafeMax = 0.666f,
				usesAmount2 = true,
				amount2Default = 0f,
				amount2Min = -3, amount2Max = 3, amount2SafeMin = -0.5f, amount2SafeMax = 0.99f,
				usesRandomize = true
			}
		},
		{
			Ops.Truncate,
			new OpConfig
			{
				usesFaces = true,
				amountDefault = 0.3f,
				amountMin = -6, amountMax = 6, amountSafeMin = 0.001f, amountSafeMax = 0.499f,
				usesRandomize = true
			}
		},
		{
			Ops.Gyro,
			new OpConfig
			{
				amountDefault = 0.33f,
				amountMin = -.5f, amountMax = 0.5f, amountSafeMin = 0.001f, amountSafeMax = 0.499f,
				usesAmount2 = true,
				amount2Min = -3, amount2Max = 3, amount2SafeMin = -0.5f, amount2SafeMax = 1
			}
		},
		{
			Ops.Snub,
			new OpConfig
			{
				amountDefault = 0.5f,
				amountMin = -1f, amountMax = 1f, amountSafeMin = 0.001f, amountSafeMax = 0.999f
			}
		},
		{Ops.Subdivide, new OpConfig
		{
			amountDefault = 0,
			amountMin = -3, amountMax = 3, amountSafeMin = -0.5f, amountSafeMax = 1,
		}},
		{
			Ops.Loft,
			new OpConfig
			{
				usesFaces = true,
				amountDefault = 0.5f,
				amountMin = -4, amountMax = 4, amountSafeMin = 0.001f, amountSafeMax = 0.999f,
				usesAmount2 = true,
				amount2Min = -3, amount2Max = 3, amount2SafeMin = -1, amount2SafeMax = 1,
				usesRandomize = true
			}
		},
		{
			Ops.Chamfer,
			new OpConfig
			{
				amountDefault = 0.5f,
				amountMin = -4, amountMax = 4, amountSafeMin = 0.001f, amountSafeMax = 0.999f
			}
		},
		{
			Ops.Quinto,
			new OpConfig
			{
				amountDefault = 0.5f,
				amountMin = -4, amountMax = 4, amountSafeMin = 0.001f, amountSafeMax = 0.999f,
				usesAmount2 = true,
				amount2Min = -3, amount2Max = 3, amount2SafeMin = -0.5f, amount2SafeMax = 1,
				usesRandomize = true
			}
		},
		{
			Ops.Lace,
			new OpConfig
			{
				usesFaces = true,
				amountDefault = 0.5f,
				amountMin = -4, amountMax = 4, amountSafeMin = 0.001f, amountSafeMax = 0.999f,
				usesAmount2 = true,
				amount2Min = -3, amount2Max = 3, amount2SafeMin = -0.5f, amount2SafeMax = 1,
				usesRandomize = true
			}
		},
		{
			Ops.JoinedLace,
			new OpConfig
			{
				amountDefault = 0.5f,
				amountMin = -4, amountMax = 4, amountSafeMin = 0.001f, amountSafeMax = 0.999f,
				usesAmount2 = true,
				amount2Min = -3, amount2Max = 3, amount2SafeMin = -0.5f, amount2SafeMax = 1,
				usesRandomize = true
			}
		},
		{
			Ops.OppositeLace,
			new OpConfig
			{
				amountDefault = 0.5f,
				amountMin = -4, amountMax = 4, amountSafeMin = 0.001f, amountSafeMax = 0.999f,
				usesAmount2 = true,
				amount2Min = -3, amount2Max = 3, amount2SafeMin = -0.5f, amount2SafeMax = 1,
				usesRandomize = true
			}
		},
		{
			Ops.JoinKisKis,
			new OpConfig
			{
				amountDefault = 0.5f,
				amountMin = -4, amountMax = 4, amountSafeMin = 0.001f, amountSafeMax = 0.999f,
				usesAmount2 = true,
				amount2Min = -3, amount2Max = 3, amount2SafeMin = -0.5f, amount2SafeMax = 1,
			}
		},
		{
			Ops.Stake,
			new OpConfig
			{
				usesFaces = true,
				amountDefault = 0.5f, amountMin = -4, amountMax = 4, amountSafeMin = 0.001f, amountSafeMax = 0.999f
			}
		},
		{
			Ops.JoinStake,
			new OpConfig
			{
				amountDefault = 0.5f,
				amountMin = -4, amountMax = 4, amountSafeMin = 0.001f, amountSafeMax = 0.999f
			}
		},
		{
			Ops.Medial,
			new OpConfig
			{
				amountDefault = 2f,
				amountMin = 2, amountMax = 8, amountSafeMin = 1, amountSafeMax = 6,
				usesAmount2 = true,
				amount2Min = -3, amount2Max = 3, amount2SafeMin = -0.5f, amount2SafeMax = 1,
			}
		},
		{
			Ops.EdgeMedial,
			new OpConfig
			{
				amountDefault = 2f,
				amountMin = 2, amountMax = 8, amountSafeMin = 1, amountSafeMax = 6,
				usesAmount2 = true,
				amount2Min = -3, amount2Max = 3, amount2SafeMin = -0.5f, amount2SafeMax = 1,
			}
		},
		// {
		// 	Ops.JoinedMedial,
		// 	new OpConfig
		// 	{
		// 		amountDefault=2f,
		// 		amountMin=2, amountMax=8, amountSafeMin=1, amountSafeMax=4,
		// 		usesAmount2 = true,
		// 		amount2Min = -3, amount2Max = 3, amount2SafeMin = -0.5f, amount2SafeMax = 1,
		// 	}
		// },
		{
			Ops.Propeller,
			new OpConfig
			{
				amountDefault = 0.25f,
				amountMin = -4, amountMax = 4, amountSafeMin = 0f, amountSafeMax = 0.5f
			}
		},
		{
			Ops.Whirl,
			new OpConfig
			{
				amountDefault = 0.25f,
				amountMin = -4, amountMax = 4, amountSafeMin = 0.001f, amountSafeMax = 0.5f
			}
		},
		{
			Ops.Volute,
			new OpConfig
			{
				amountDefault = 0.33f,
				amountMin = -4, amountMax = 4, amountSafeMin = 0.001f, amountSafeMax = 0.999f
			}
		},
		{
			Ops.Exalt,
			new OpConfig
			{
				usesFaces = true,
				amountDefault = 0.1f,
				amountMin = -6, amountMax = 6, amountSafeMin = 0.001f, amountSafeMax = 0.999f,
				usesRandomize = true
			}
		},
		{
			Ops.Yank,
			new OpConfig
			{
				usesFaces = true,
				amountDefault = 0.33f,
				amountMin = -6, amountMax = 6, amountSafeMin = 0.001f, amountSafeMax = 0.999f,
				usesRandomize = true
			}
		},
		{
			Ops.Cross,
			new OpConfig
			{
				amountDefault = 0.5f,
				amountMin = -1, amountMax = 1, amountSafeMin = -1, amountSafeMax = 0.999f,
				usesRandomize = true
			}
		},

		{
			Ops.Squall,
			new OpConfig
			{
				amountDefault = 0.5f,
				amountMin = -4, amountMax = 4, amountSafeMin = 0.001f, amountSafeMax = 0.999f
			}
		},
		{
			Ops.JoinSquall,
			new OpConfig
			{
				amountDefault = 0.5f,
				amountMin = -4, amountMax = 4, amountSafeMin = 0.001f, amountSafeMax = 0.999f
			}
		},
		{
			Ops.SplitFaces,
			new OpConfig
			{
				usesFaces = true,
				usesAmount = false,
			}
		},
		{
			Ops.Gable,
			new OpConfig
			{
				usesFaces = true,
				amountDefault = 0.5f,
				amountMin = -4, amountMax = 4, amountSafeMin = 0.001f, amountSafeMax = 0.999f,
				usesAmount2 = true,
				amount2Min = -3, amount2Max = 3, amount2SafeMin = -0.5f, amount2SafeMax = 1,
				usesRandomize = true
			}
		},
		{
			Ops.FaceOffset,
			new OpConfig
			{
				usesFaces = true,
				amountDefault = 0.1f,
				amountMin = -6, amountMax = 6, amountSafeMin = -1, amountSafeMax = 0.999f,
				usesRandomize = true
			}
		},
		//{Ops.Ribbon, new OpConfig{}},
		{
			Ops.Extrude,
			new OpConfig
			{
				usesFaces = true,
				amountDefault = 0.1f,
				amountMin = -12, amountMax = 12, amountSafeMin = -6f, amountSafeMax = 6f,
				usesRandomize = true
			}
		},
		{
			Ops.Shell,
			new OpConfig
			{
				amountDefault = 0.1f,
				amountMin = -6, amountMax = 6, amountSafeMin = 0.001f, amountSafeMax = 0.999f
			}
		},
		{
			Ops.Skeleton,
			new OpConfig
			{
				usesFaces = true,
				amountDefault = 0.1f,
				amountMin = -6, amountSafeMin = 0.001f, amountSafeMax = 0.999f, amountMax = 6
			}
		},
		{
			Ops.VertexScale,
			new OpConfig
			{
				usesFaces = true,
				amountDefault = 0.5f,
				amountMin = -6, amountMax = 6, amountSafeMin = -1, amountSafeMax = 0.999f,
				usesRandomize = true
			}
		},
		{
			Ops.VertexRotate,
			new OpConfig
			{
				usesFaces = true,
				amountDefault = 0.5f,
				amountMin = -4, amountMax = 4, amountSafeMin = -1, amountSafeMax = 1,
				usesRandomize = true
			}
		},
		{
			Ops.VertexFlex,
			new OpConfig
			{
				usesFaces = true,
				amountDefault = 0.1f, amountMin = -6, amountMax = 6, amountSafeMin = -1, amountSafeMax = 0.999f,
				usesRandomize = true
			}
		},
		//{Ops.FaceTranslate, new OpConfig{usesFaces=true, amountDefault=0.1f, amountMin=-6, amountMax=6}},
		{
			Ops.FaceScale,
			new OpConfig
			{
				usesFaces = true,
				amountDefault = -0.5f,
				amountMin = -6, amountMax = 6, amountSafeMin = -1, amountSafeMax = 0,
				usesRandomize = true
			}
		},
		{
			Ops.FaceRotate,
			new OpConfig
			{
				usesFaces = true,
				amountDefault = 45f,
				amountMin = -3, amountMax = 3, amountSafeMin = -1, amountSafeMax = 1,
				usesRandomize = true
			}
		},
		// {
		// 	Ops.PolarOffset,
		// 	new OpConfig
		// 	{
		// 		usesFaces = true,
		// 		amountDefault = 0.5f,
		// 		amountMin = -4, amountMax = 4, amountSafeMin = -1, amountSafeMax = 1,
		// 	}
		// },
		{
			Ops.FaceSlide,
			new OpConfig
			{
				usesFaces = true,
				amountDefault = 0.5f,
				amountMin = -4, amountMax = 4, amountSafeMin = -1f, amountSafeMax = 1f,
				usesAmount2 = true,
				amount2Min = -4f, amount2Max = 4f, amount2SafeMin = -2f, amount2SafeMax = 2f,
				usesRandomize = true
			}
		},
//			{Ops.FaceRotateX, new OpConfig{usesFaces=true, amountDefault=0.1f, amountMin=-180, amountMax=180}},
//			{Ops.FaceRotateY, new OpConfig{usesFaces=true, amountDefault=0.1f, amountMin=-180, amountMax=180}},
		{Ops.FaceRemove, new OpConfig {usesFaces = true, usesAmount = false}},
		{Ops.FillHoles, new OpConfig {usesAmount = false}},
		{
			Ops.ExtendBoundaries,
			new OpConfig
			{
				amountDefault = 0.5f,
				amountMin = -4, amountMax = 4, amountSafeMin = -1f, amountSafeMax = 1f,
				usesAmount2 = true,
				amount2Min = -180, amount2Max = 180, amount2SafeMin = -100f, amount2SafeMax = 100,
			}
		},
		{Ops.FaceMerge, new OpConfig {usesFaces = true, usesAmount = false}},
		{Ops.FaceKeep, new OpConfig {usesFaces = true, usesAmount = false}},
		{Ops.VertexRemove, new OpConfig {usesFaces = true, usesAmount = false}},
		{Ops.VertexKeep, new OpConfig {usesFaces = true, usesAmount = false}},
		{
			Ops.Layer,
			new OpConfig
			{
				usesFaces = true,
				amountDefault = 0.1f,
				amountMin = -2f, amountMax = 2f, amountSafeMin = -2f, amountSafeMax = 2f,
				usesAmount2 = true,
				amount2Min = -3, amount2Max = 3, amount2SafeMin = -1, amount2SafeMax = 1,
				usesRandomize = true
			}
		},
		{
			Ops.Hinge,
			new OpConfig
			{
				amountDefault = 15f,
				amountMin = -180, amountMax = 180, amountSafeMin = 0, amountSafeMax = 180
			}
		},
		{
			Ops.AddDual,
			new OpConfig
			{
				amountDefault = 1f,
				amountMin = -6, amountMax = 6, amountSafeMin = -2, amountSafeMax = 2
			}
		},
		{
			Ops.AddCopyX,
			new OpConfig
			{
				usesFaces = true,
				amountDefault = 0,
				amountMin = -6, amountMax = 6, amountSafeMin = -2, amountSafeMax = 2
			}
		},
		{
			Ops.AddCopyY,
			new OpConfig
			{
				usesFaces = true,
				amountDefault = 0,
				amountMin = -6, amountMax = 6, amountSafeMin = -2, amountSafeMax = 2
			}
		},
		{
			Ops.AddCopyZ,
			new OpConfig
			{
				usesFaces = true,
				amountDefault = 0,
				amountMin = -6, amountMax = 6, amountSafeMin = -2, amountSafeMax = 2
			}
		},
		{
			Ops.AddMirrorX,
			new OpConfig
			{
				usesFaces = true,
				amountDefault = 0,
				amountMin = -6, amountMax = 6, amountSafeMin = -2, amountSafeMax = 2
			}
		},
		{
			Ops.AddMirrorY,
			new OpConfig
			{
				usesFaces = true,
				amountDefault = 0,
				amountMin = -6, amountMax = 6, amountSafeMin = -2, amountSafeMax = 2
			}
		},
		{
			Ops.AddMirrorZ,
			new OpConfig
			{
				usesFaces = true,
				amountDefault = 0,
				amountMin = -6, amountMax = 6, amountSafeMin = -2, amountSafeMax = 2
			}
		},
		{Ops.Stash, new OpConfig
			{
				usesFaces = true,
				usesAmount = false
			}
		},
		{
			Ops.Unstash,
			new OpConfig
			{
				usesFaces = true,
				amountDefault = 0,
				amountMin = -6, amountMax = 6, amountSafeMin = -2, amountSafeMax = 2,
				usesAmount2 = true,
				amount2Min = -6, amount2Max = 6, amount2SafeMin = -2, amount2SafeMax = 2
			}
		},
		{
			Ops.UnstashToFaces,
			new OpConfig
			{
				usesFaces = true,
				amountDefault = 0,
				amountMin = -6, amountMax = 6, amountSafeMin = -2, amountSafeMax = 2,
				usesAmount2 = true,
				amount2Min = -3, amount2Max = 3, amount2SafeMin = -1, amount2SafeMax = 1
			}
		},
		{
			Ops.UnstashToVerts,
			new OpConfig
			{
				usesFaces = true,
				amountDefault = 0,
				amountMin = -6, amountMax = 6, amountSafeMin = -2, amountSafeMax = 2,
				usesAmount2 = true,
				amount2Min = -3, amount2Max = 3, amount2SafeMin = -1, amount2SafeMax = 1
			}
		},
		{Ops.TagFaces, new OpConfig
			{
				usesFaces = true,
			}
		},
		{
			Ops.Stack,
			new OpConfig
			{
				usesFaces = true,
				amountDefault = 0.5f,
				amountMin = -2f, amountMax = 2f, amountSafeMin = -2f, amountSafeMax = 2f,
				usesAmount2 = true,
				amount2Default =  0.8f,
				amount2Min = 0.1f, amount2Max = .9f, amount2SafeMin = .01f, amount2SafeMax = .99f
			}
		},
		{
			Ops.Canonicalize,
			new OpConfig
			{
				usesAmount = false,
			}
		},
		{
			Ops.ConvexHull,
			new OpConfig
			{
				usesFaces = false,
				usesAmount = false,
			}
		},
		{
			Ops.Spherize,
			new OpConfig
			{
				usesFaces = true,
				amountDefault = 1.0f, amountMin = -2, amountMax = 2, amountSafeMin = -2,
				amountSafeMax = 2f
			}
		},
		{
			Ops.Stretch,
			new OpConfig
			{
				amountDefault = 1.0f,
				amountMin = -6f, amountMax = 6f, amountSafeMin = -3f, amountSafeMax = 3f
			}
		},
		{
			Ops.Slice,
			new OpConfig
			{
				amountDefault = 0.5f,
				amountMin = 0f, amountMax = 1f, amountSafeMin = 0f, amountSafeMax = 1f,
				usesAmount2 = true,
				amount2Min = -3, amount2Max = 3, amount2SafeMin = -1, amount2SafeMax = 1
			}
		},
		{Ops.Recenter, new OpConfig {usesAmount = false}},
		{Ops.SitLevel, new OpConfig
		{
			amountDefault = 0,
			amountMin = 0f, amountMax = 1f, amountSafeMin = 0f, amountSafeMax = 1f,
		}},
		{
			Ops.Weld,
			new OpConfig
			{
				amountDefault = 0.001f,
				amountMin = 0, amountMax = .25f, amountSafeMin = 0.001f, amountSafeMax = 0.1f
			}
		}
	};

}