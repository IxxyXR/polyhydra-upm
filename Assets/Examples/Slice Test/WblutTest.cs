using Conway;
using Johnson;
using UnityEngine;
using Wythoff;


[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class WblutTest : MonoBehaviour
{
    public PolyHydraEnums.JohnsonPolyTypes JohnsonPolyType;
    [Range(3,40)] public int Sides = 4;
 
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
    public PolyHydraEnums.ColorMethods ColorMethod;

    [Space]
    public Vector3 PositionBefore = Vector3.zero;
    public Vector3 RotationBefore = Vector3.zero;
    public Vector3 ScaleBefore = Vector3.one;
    public Vector3 SliceAngle = Vector3.zero;
    public float SliceMin = 0.01f;
    public float SliceMax = 0.2f;
    public float SliceStart = -.5f;
    public float SliceEnd = 0.5f;
    public float ShiftMin = -.2f;
    public float ShiftMax = .2f;
    public float Gap = 0.1f;


    [Space]
    public int RandomSeed = 12345;
    public bool SliceX = false;
    public bool SliceY = true;
    public bool SliceZ = false;
    public bool Weld;
    public bool Cap;

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
        SliceAngle = new Vector3(Mathf.Sin(Time.time/7f + 5f) * 30, Mathf.Sin(Time.time/4f) * 50, 0);
        Generate();
    }

    [ContextMenu("Generate")]
    public void Generate()
    {
        if (Random.seed != 0) Random.seed = RandomSeed;
        
        poly = JohnsonPoly.Build(JohnsonPolyType, Sides);
        poly.Transform(PositionBefore, RotationBefore, ScaleBefore);
        
        if (ApplyOp)
        {
            var o1 = new OpParams {valueA = op1Amount1, valueB = op1Amount2, facesel = op1Facesel};
            poly = poly.ApplyOp(op1, o1);
        }

        if (Canonicalize)
        {
            poly = poly.Canonicalize(0.01, 0.01);
        }
        
        // poly.Recenter();

        ConwayPoly DoSlices(ConwayPoly input, Vector3 axis)
        {
            var result = new ConwayPoly();
            float offset = SliceStart;
            
            do
            {
                offset += Random.Range(SliceMin, SliceMax);
                var planePos = new Vector3(offset*axis.x, offset*axis.y, offset*axis.z);
                var sliced = input.SliceByPlane(new Plane(Quaternion.Euler(SliceAngle) * axis, planePos), Cap);
                var gapVector = new Vector3(-Gap*axis.x, -Gap*axis.y, -Gap*axis.z);
                result.Transform(gapVector);
                float randomShift = Random.Range(ShiftMin, ShiftMax);
                var randomShiftVector = new Vector3(randomShift*axis.y, randomShift*axis.z, randomShift*axis.x);
                result.Append(sliced.bottom.Duplicate(randomShiftVector));
                input = sliced.top;
            } while (offset < SliceEnd);

            result.Append(input);
            if (Weld)
            {
                result = result.Weld(0.0001f);
            }
            return result;
        }

        if (SliceX) poly = DoSlices(poly, Vector3.right);
        if (SliceY) poly = DoSlices(poly, Vector3.up);
        if (SliceZ) poly = DoSlices(poly, Vector3.forward);
        
        // if (Weld)
        // {
        //     poly = poly.Weld(0.0001f);
        // }
        
        if (ApplyOp)
        {
            var o2 = new OpParams {valueA = op2Amount1, valueB = op2Amount2, facesel = op2Facesel};
            poly = poly.ApplyOp(op2, o2);
        }

        // poly.Recenter();
        
        var mesh = PolyMeshBuilder.BuildMeshFromConwayPoly(poly, false, null, ColorMethod);
        GetComponent<MeshFilter>().mesh = mesh;
    }
    
    void OnDrawGizmos () {
        // GizmoHelper.DrawGizmos(poly, transform, true, false, false, false);
    }


}
