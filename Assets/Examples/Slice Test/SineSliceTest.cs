using Conway;
using Johnson;
using UnityEngine;
using Wythoff;


[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class SineSliceTest : MonoBehaviour
{
    public PolyHydraEnums.JohnsonPolyTypes JohnsonPolyType;
    [Range(3,40)] public int Sides = 4;
 
    public bool ApplyOp;
    public Ops op0;
    public FaceSelections op0Facesel;
    public float op0Amount1 = 0;
    public float op0Amount2 = 0;
    public Ops op1;
    public FaceSelections op1Facesel;
    public float op1Amount1 = 0;
    public float op1Amount2 = 0;
    public Ops op2;
    public FaceSelections op2Facesel;
    public float op2Amount1 = 0;
    public float op2Amount2 = 0;
    public bool Canonicalize;
    public PolyHydraEnums.ColorMethods ColorMethod;

    [Space]
    public Vector3 PositionBefore = Vector3.zero;
    public Vector3 RotationBefore = Vector3.zero;
    public Vector3 ScaleBefore = Vector3.one;
    public Vector3 SliceAngle = Vector3.zero;
    public float SliceDistance = 0.2f;
    public float SliceGap = 0.1f;
    public float SliceStart = -.5f;
    public float SliceEnd = 0.5f;
    public bool Cap;
    public bool Weld;
    public float AnimationSpeed;
    
    public float frequency = 1f;
    public float amplitude = 1f;

    private ConwayPoly poly;

    void Start()
    {
        Generate();
    }
    
    private void OnValidate()
    {
        Generate();
    }
    
    private void Update()
    {
        if (AnimationSpeed > 0)
        SliceAngle = new Vector3(Mathf.Sin(Time.time*AnimationSpeed + 5f) * 30, Mathf.Sin(Time.time*AnimationSpeed*.7f) * 60, 0);
        Generate();
    }

    [ContextMenu("Generate")]
    public void Generate()
    {
        poly = JohnsonPoly.Build(JohnsonPolyType, Sides);
        poly = poly.Transform(PositionBefore, RotationBefore, ScaleBefore);
        
        if (ApplyOp)
        {
            var o0 = new OpParams {valueA = op0Amount1, valueB = op0Amount2, facesel = op0Facesel};
            poly = poly.ApplyOp(op0, o0);
        }

        if (Canonicalize)
        {
            poly = poly.Canonicalize(0.01, 0.01);
        }
        
        poly.Recenter();

        ConwayPoly DoSlices(ConwayPoly input, Vector3 axis)
        {
            var result = new ConwayPoly();
            float offset = SliceStart;

            int failsafe = 100;
            
            do
            {
                offset += SliceDistance;
                var planePos = new Vector3(offset*axis.x, offset*axis.y, offset*axis.z);
                var sliced = input.SliceByPlane(new Plane(Quaternion.Euler(SliceAngle) * axis, planePos), Cap);
                var gapVector = new Vector3(-SliceGap*axis.x, -SliceGap*axis.y, -SliceGap*axis.z);
                result = result.Transform(gapVector);
                var lerp = Mathf.InverseLerp(SliceStart, SliceEnd, offset);
                var shift = new Vector3(Mathf.Sin(lerp*frequency)*amplitude, 0, 0);
                result.Append(sliced.bottom.Transform(shift));
                input = sliced.top;
                failsafe--;
            } while (offset < SliceEnd && failsafe > 0);

            result.Append(input);
            if (Weld)
            {
                result = result.Weld(0.0001f);
            }
            return result;
        }

        var output = new ConwayPoly();
        output.Append(DoSlices(poly, Vector3.up));

        if (ApplyOp)
        {
            var o1 = new OpParams {valueA = op1Amount1, valueB = op1Amount2, facesel = op1Facesel};
            output = output.ApplyOp(op1, o1);
            if (Weld)
            {
                poly = poly.Weld(0.0001f);
            }
            var o2 = new OpParams {valueA = op2Amount1, valueB = op2Amount2, facesel = op2Facesel};
            output = output.ApplyOp(op2, o2);
        }
        
        output.Recenter();

        var mesh = PolyMeshBuilder.BuildMeshFromConwayPoly(output, false, null, ColorMethod);
        GetComponent<MeshFilter>().mesh = mesh;
    }
    
    void OnDrawGizmos () {
        // GizmoHelper.DrawGizmos(poly, transform, true, false, false, false);
    }


}
