using System;
using Conway;
using UnityEngine;
using Wythoff;
using Random = UnityEngine.Random;


[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class LoftProfileTest4 : MonoBehaviour
{
    public enum DomeAnimationType
    {
        Height,
        ShearDirection,
        
    }
    
    public PolyTypes PolyType;
    [Range(1,40)] public int PrismP = 4;
    [Range(1,40)] public int PrismQ = 4;

    public bool ApplyOp;
    public Ops op1;
    public FaceSelections op1Facesel;
    public float op1Amount1 = 0;
    public float op1Amount2 = 0;
    public Ops op2;
    public FaceSelections op2Facesel;
    public float op2Amount1 = 0;
    public float op2Amount2 = 0;
    public bool Canonicalize;
    public Vector3 Position = Vector3.zero;
    public Vector3 Rotation = Vector3.zero;
    public Vector3 Scale = Vector3.one;
    public PolyHydraEnums.ColorMethods ColorMethod;
    public AnimationCurve Profile;
    public AnimationCurve Shear;
    public FaceSelections domeFaceSel;
    public bool FlipProfile;

    public float DomeHeight = 1f;
    public float DomeDepth = 1f;
    [Range(-1,1)]public float AnimateAmount = 0.5f;
    [Range(0,10)] public float AnimateSpeed = 1f;
    public DomeAnimationType domeAnimationType;
    public Easing.EasingType animationEasingType;
    public bool AnimateOn;
    public int AnimationSkipFrames = 2;
    
    private AnimationCurve shearDirection;
    
    public Color[] Colors =
    {
        new Color(1.0f, 0.5f, 0.5f),
        new Color(0.8f, 0.85f, 0.9f),
        new Color(0.5f, 0.6f, 0.6f),
        new Color(1.0f, 0.94f, 0.9f),
        new Color(0.66f, 0.2f, 0.2f),
        new Color(0.6f, 0.0f, 0.0f),
        new Color(1.0f, 1.0f, 1.0f),
        new Color(0.6f, 0.6f, 0.6f),
        new Color(0.5f, 1.0f, 0.5f),
        new Color(0.5f, 0.5f, 1.0f),
        new Color(0.5f, 1.0f, 1.0f),
        new Color(1.0f, 0.5f, 1.0f),
    };

    private ConwayPoly poly;
    
    void Start()
    {
        shearDirection = AnimationCurve.Linear(0, 0, 1, 0);
        Generate();
    }
    
    private void OnValidate()
    {
        Generate();
    }

    public void Update()
    {
        if (AnimateOn && (Time.frameCount % Math.Min(AnimationSkipFrames,1))==0)
        {
            float animValue = AnimateAmount * Easing.Funcs[animationEasingType](Mathf.Clamp((Time.time * AnimateSpeed) % 1.5f, 0f, 1f));
            switch (domeAnimationType)
            {
            case DomeAnimationType.Height:
                DomeHeight = animValue;
                break;
            case DomeAnimationType.ShearDirection:
                shearDirection = AnimationCurve.Linear(0, 0, 1, animValue);
                break;
            }
            Generate();
        }
    }

    [ContextMenu("Generate")]
    public void Generate()
    {
        
        var wythoff = new WythoffPoly(PolyType, PrismP, PrismQ);
        wythoff.BuildFaces();
        poly = new ConwayPoly(wythoff);
        
        if (ApplyOp)
        {
            var o1 = new OpParams {valueA = op1Amount1, valueB = op1Amount2, facesel = op1Facesel};
            poly = poly.ApplyOp(op1, o1);
            var o2 = new OpParams {valueA = op2Amount1, valueB = op2Amount2, facesel = op2Facesel};
            poly = poly.ApplyOp(op2, o2);
        }

        if (Canonicalize)
        {
            poly = poly.Canonicalize(0.01, 0.01);
        }
        

        poly = poly.LoftAlongProfile(new OpParams(DomeDepth, DomeHeight, domeFaceSel), Profile, Shear, shearDirection, flipProfile: FlipProfile);
        
        // poly = poly.Transform(Position, Rotation, Scale);
        
        var mesh = PolyMeshBuilder.BuildMeshFromConwayPoly(poly, false, Colors, ColorMethod);
        GetComponent<MeshFilter>().mesh = mesh;
    }
    void OnDrawGizmos () {
        // GizmoHelper.DrawGizmos(poly, transform, true, false, false);
    }

}
