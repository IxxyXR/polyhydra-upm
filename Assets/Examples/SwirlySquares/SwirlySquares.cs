using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Conway;
using Grids;
using UnityEngine;


// [ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class SwirlySquares : MonoBehaviour
{
    public GridEnums.GridTypes GridType;
    public GridEnums.GridShapes GridShape;
    
    [Range(1,40)] public int width = 4;
    [Range(1,40)] public int depth = 3;

    public bool ApplyOp;
    public Ops op1;
    public FaceSelections op1Facesel;
    public float op1Amount1 = 0;
    public float op1Amount2 = 0;
    public Ops op2;
    public FaceSelections op2Facesel;
    public float op2Amount1 = 0;
    public float op2Amount2 = 0;
    public Ops op3;
    public FaceSelections op3Facesel;
    public float op3Amount1 = 0;
    public float op3Amount2 = 0;
    public PolyHydraEnums.ColorMethods ColorMethod;
    public float TweenTime = 2f;


    private ConwayPoly poly;
    private Func<FilterParams, bool> blackFilter;
    private Func<FilterParams, bool> whiteFilter;
    
    private Color[] C1 =
    {
        Color.white,
        Color.black,
        Color.white,
        Color.black,
        Color.white,
        Color.black,
        Color.green,
        Color.green,
        Color.green
    };

    private Color[] C2 =
    {
        Color.black,
        Color.white,
        Color.black,
        Color.white,
        Color.black,
        Color.white,
        Color.green,
        Color.green,
        Color.green
    };
    
    private Color[] meshColors;


    private void DebugTags()
    {
        Debug.Log("-----------------");
        var tags = poly.FaceTags.SelectMany(d => d.Select(i => i.Item1));
        var uniqueTags = new HashSet<string>(tags).ToList();
        Debug.Log(String.Join(",", uniqueTags));

    }

    private void Init()
    {
        meshColors = C1;
        iTween.Init(gameObject);
    }
    
    void Start()
    {
        Init();
        Generate();
        TweenB1();
    }

    private void TweenB1()
    {
        meshColors = C1;
        transform.localScale = Vector3.one * 20;
        width = 4;
        depth = 4;
        transform.rotation = Quaternion.identity;
        op1 = Ops.Gyro;
        op1Amount1 = 0.5f;
        op2 = Ops.Identity;
        whiteFilter = x => x.poly.FaceRoles[x.index]==ConwayPoly.Roles.New;
        blackFilter = x => x.poly.FaceRoles[x.index]==ConwayPoly.Roles.NewAlt;
        Generate();
        
        iTween.ValueTo(gameObject, new Hashtable()
        {
            {"from", 0.5f},
            {"to", 0f},
            {"time", TweenTime},
            {"onupdate", nameof(SetOp1Amount1)},
            {"easetype", iTween.EaseType.easeInOutCubic},
            {"oncomplete", nameof(TweenB2)}
        });
    }
    
    private void TweenB2()
    {
        op1 = Ops.Stake;
        op1Amount1 = 1f;
        whiteFilter = x => x.poly.FaceRoles[x.index]==ConwayPoly.Roles.New;
        blackFilter = x => x.poly.FaceRoles[x.index]==ConwayPoly.Roles.NewAlt;
        
        Generate();
        iTween.ValueTo(gameObject, new Hashtable()
        {
            {"from", 1f},
            {"to", 0f},
            {"time", TweenTime},
            {"onupdate", nameof(SetOp1Amount1)},
            {"easetype", iTween.EaseType.easeInOutCubic},
            {"oncomplete", nameof(TweenB3)}
        });
    }
    
    private void TweenB3()
    {
        transform.localScale = Vector3.one * 10;
        width = 8;
        depth = 8;
        op1 = Ops.FaceScale;
        op1Amount1 = 0.0001f;
        op2 = Ops.FaceRotateX;
        op2Amount1 = 0f;
        Generate();
        iTween.ValueTo(gameObject, new Hashtable()
        {
            {"from", 0f},
            {"to", 1f},
            {"time", TweenTime/2f},
            {"onupdate", nameof(SetOp2Amount1)},
            {"easetype", iTween.EaseType.linear},
            {"oncomplete", nameof(TweenB4)}
        });
    }
    
    private void TweenB4()
    {
        meshColors = C2;
        transform.localScale = Vector3.one * 10;
        width = 8;
        depth = 8;
        op1 = Ops.FaceScale;
        op1Amount1 = 0.0001f;
        op2 = Ops.FaceRotateX;
        op2Amount1 = 1f;
        Generate();
        iTween.ValueTo(gameObject, new Hashtable()
        {
            {"from", 1f},
            {"to", 0f},
            {"time", TweenTime/2f},
            {"onupdate", nameof(SetOp2Amount1)},
            {"easetype", iTween.EaseType.easeInCubic},
            {"oncomplete", nameof(TweenB1)}
        });
    }    

    private void TweenA1()
    {
        op1 = Ops.Ortho;
        whiteFilter = x => x.poly.FaceRoles[x.index]==ConwayPoly.Roles.New;
        blackFilter = x => x.poly.FaceRoles[x.index]==ConwayPoly.Roles.NewAlt;
        
        iTween.ValueTo(gameObject, new Hashtable()
        {
            {"from", 0.0f},
            {"to", 242.0f},
            {"time", TweenTime},
            {"onupdate", nameof(SetOp1Amount1)},
            {"easetype", iTween.EaseType.easeInOutQuad},
            // {"looptype", iTween.LoopType.pingPong},
            {"oncomplete", nameof(TweenA2)}
        });
        
        iTween.ScaleTo(gameObject, new Hashtable()
        {
            {"scale", new Vector3(80, 80, 80)},
            {"time", TweenTime},
            {"easetype", iTween.EaseType.easeInOutQuad},
            // {"looptype", iTween.LoopType.pingPong}
        });
    }
    
    private void TweenA2()
    {
        // poly.ClearTags();
        // blackFilter = x => 
        // {
        //     var centroid = x.poly.Faces[x.index].Centroid;
        //     return !((centroid.x > 0f && centroid.z > 0f) || (centroid.x < 0f && centroid.z < 0f));
        // };
        // whiteFilter = x => false;

        transform.localScale = Vector3.one * 100;
        op1Amount1 = 0;

        // op1 = Ops.FaceRotate;
        // iTween.ValueTo(gameObject, new Hashtable()
        // {
        //     {"from", 0.0f},
        //     {"to", 1.0f},
        //     {"time", TweenTime},
        //     {"onupdate", nameof(SetOp1Amount1)},
        //     {"easetype", iTween.EaseType.easeInOutQuad},
        //     // {"looptype", iTween.LoopType.pingPong},
        //     {"oncomplete", nameof(TweenA3)}
        // });
        
        iTween.ScaleTo(gameObject, new Hashtable()
        {
            {"scale", new Vector3(10, 10, 10)},
            {"time", TweenTime},
            {"easetype", iTween.EaseType.easeInOutCubic},
            {"oncomplete", nameof(TweenA1)}
            // {"looptype", iTween.LoopType.pingPong}
        });
    }
    
    private void TweenA3()
    {
        iTween.ScaleTo(gameObject, new Hashtable()
        {
            {"scale", new Vector3(10, 10, 10)},
            {"time", TweenTime},
            {"easetype", iTween.EaseType.easeInQuart},
            {"oncomplete", nameof(TweenA1)}
            // {"looptype", iTween.LoopType.pingPong}
        });
    }

    private void SetOp1Amount1(float val){
        op1Amount1 = val;
        Generate();
    }
    
    private void SetOp2Amount1(float val){
        op2Amount1 = val;
        Generate();
    }
    
    void Update()
    {
        // if (op1Animate || op2Animate)
        // {
        // Generate();
        // }
    }

    private void OnValidate()
    {
        if (Application.isPlaying) return;
        Init();
        Generate();
    }

    [ContextMenu("Generate")]
    public void Generate()
    {
        poly = Grids.Grids.MakeGrid(GridType, GridShape, width, depth);
        poly.ClearTags();
        poly.TagFaces("#FFFFFF", filter: whiteFilter);
        poly.TagFaces("#000000", filter: blackFilter);
        
        if (ApplyOp)
        {
            var o1 = new OpParams {valueA = op1Amount1, valueB = op1Amount2, facesel = op1Facesel};
            poly = poly.ApplyOp(op1, o1);
            var o2 = new OpParams {valueA = op2Amount1, valueB = op2Amount2, facesel = op2Facesel};
            poly = poly.ApplyOp(op2, o2);
            var o3 = new OpParams {valueA = op3Amount1, valueB = op3Amount2, facesel = op3Facesel};
            poly = poly.ApplyOp(op3, o3);
        }
        var mesh = PolyMeshBuilder.BuildMeshFromConwayPoly(poly, false, meshColors, ColorMethod);
        GetComponent<MeshFilter>().mesh = mesh;
    }

}
