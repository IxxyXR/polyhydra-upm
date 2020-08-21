using Conway;
using Johnson;
using UnityEngine;
using Random = UnityEngine.Random;


[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class RandomSpaceshipGenerator : MonoBehaviour
{

    public Material material;

    [Range(3, 10)]
    public int numSides = 4;
    [Range(2, 5)]
    public int numSections = 4;
    [Range(0, 3)]
    public float NoseLength = 1;
    [Range(0, 1)]
    public float NoseSharpness = 1;
    public bool NoseVariant;
    public bool EngineVariant;

    public int seed = 12345;
    
    [Range(0f, 1f)]
    public float ChanceOfSimpleSegment = 0.85f;
    [Range(0f, 1f)]
    public float ChanceOfLaceSegment = 0.75f;
    [Range(0f, 1f)]
    public float ChanceOfTruncateSegment = 0.75f;
    [Range(0f, 1f)]
    public float ChanceOfFins = 0.5f;
    [Range(0f, 1f)]
    public float ChanceOfWings = 0.25f;
    
    public float foo = 0f;
    public float bar = 0f;
    
    private float loftLow = -.25f;
    private float loftHigh = 0.75f;


    void Start()
    {
        Generate();
    }

    private void OnValidate()
    {
        Generate();
    }
    
    [ContextMenu("Generate")]
    public void Generate()
    {
        Random.seed = seed;
        
        var spaceship = JohnsonPoly.Prism(numSides);
        var wings = new ConwayPoly();
        float angleCorrection = 180f / numSides;
        if (numSides % 2 != 0) angleCorrection /= 2f;
        spaceship = spaceship.Rotate(Vector3.up, angleCorrection);
        spaceship = spaceship.Rotate(Vector3.left, -90);

        for (int i = 0; i < 2; i++)
        {
            for (int j = 0; j <= numSections; j++)
            {
                spaceship = MakeSection(spaceship);
            }
            
            
            spaceship = spaceship.Rotate(Vector3.up, 180);
            // Change range for back sections
            loftLow = -0.35f;
            loftHigh = 0.15f;

        }

        // Nose
        if (NoseLength > 0)
        {
            spaceship = spaceship.Loft(new OpParams {valueA = .2f, valueB = 0, facesel = FaceSelections.FacingStraightForward});
            spaceship = spaceship.FaceSlide(new OpParams{valueA = .12f, facesel = FaceSelections.Existing});
            if (NoseVariant)
            {
                spaceship = spaceship.Lace(new OpParams{valueA = NoseSharpness, valueB = NoseLength, facesel = FaceSelections.Existing});
            }
            else
            {
                spaceship = spaceship.Loft(new OpParams{valueA = NoseSharpness, valueB = NoseLength, facesel = FaceSelections.Existing});
            }

        }
        
        // Add Panel Ridges to everything
        spaceship = spaceship.Loft(new OpParams{valueA = 0.1f, valueB = 0.025f});

        // Engines
        spaceship = spaceship.Loft(new OpParams{valueA = Random.Range(.3f, .4f), valueB = Random.Range(-.2f, .2f), facesel = FaceSelections.FacingStraightBackward});
        if (EngineVariant)
        {
            var engines = spaceship.Duplicate();
            spaceship = spaceship.FaceRemove(new OpParams{facesel = FaceSelections.Existing});
            engines = engines.FaceKeep(new OpParams{facesel = FaceSelections.Existing});
            engines = engines.Ortho(new OpParams{valueA = 0});
            engines = engines.Loft(new OpParams{valueA = Random.Range(0, .25f), valueB = -.5f, facesel = FaceSelections.All});
            spaceship.Append(engines);
        }
        else
        {
            spaceship = spaceship.Loft(new OpParams{valueA = Random.Range(.5f, .5f), valueB = Random.Range(.2f, .5f), facesel = FaceSelections.Existing});
            spaceship = spaceship.Loft(new OpParams{valueA = Random.Range(.1f, .3f), valueB = Random.Range(-.3f, -.7f), facesel = FaceSelections.AllNew});
        }

        //spaceship = spaceship.Kis(1f, FaceSelections.FacingForward);
        wings = wings.Loft(new OpParams{valueA = 0.1f, valueB = 0.025f});
        spaceship.Append(wings);

        var mesh = PolyMeshBuilder.BuildMeshFromConwayPoly(spaceship, false);
        GetComponent<MeshFilter>().mesh = mesh;
        GetComponent<MeshRenderer>().material = material;
    }

    private ConwayPoly MakeSection(ConwayPoly spaceship)
    {
        if (Random.value < ChanceOfSimpleSegment)
        {
            spaceship = spaceship.Loft(new OpParams{valueA = Random.Range(loftLow, loftHigh), valueB = Random.Range(.2f, .5f), facesel = FaceSelections.FacingStraightForward});
            spaceship = MakeWings(spaceship);
        }
        else
        {
            if (Random.value < ChanceOfLaceSegment)
            {
                spaceship = spaceship.Lace(new OpParams{valueA = Random.Range(loftLow, loftHigh), facesel = FaceSelections.FacingStraightForward, valueB = Random.Range(.2f, .5f)});
                spaceship = MakeWings(spaceship);
            }
            else if (Random.value < ChanceOfTruncateSegment)
            {
                spaceship = spaceship.Truncate(new OpParams{valueA = Random.Range(loftLow, loftHigh), facesel = FaceSelections.FacingForward});
                spaceship = MakeWings(spaceship);
            }
            else
            {
                spaceship = RibbedExtrude(spaceship, Random.Range(2, 7));
            }
        }

        if (Random.value < ChanceOfFins)
        {
            spaceship = spaceship.Loft(new OpParams{valueA = Random.Range(.5f, 0), valueB = Random.Range(0.05f, 1.3f), facesel = FaceSelections.AllNew});
        }

        spaceship = spaceship.FaceSlide(new OpParams{valueA = Random.Range(-.3f, .3f), valueB = 0, facesel = FaceSelections.FacingStraightForward});
        return spaceship;
    }

    private ConwayPoly RibbedExtrude(ConwayPoly poly, int numRibs)
    {
        float translateForwardsPerRib = Random.Range(0.02f, 0.2f);
        float ribDepth = Random.Range(0.02f, 0.2f);
        for (int i=0; i<numRibs; i++)
        {
            poly = poly.Loft(new OpParams{valueA = ribDepth, valueB = translateForwardsPerRib * 0.25f, facesel = FaceSelections.FacingStraightForward});
            poly = poly.Loft(new OpParams{valueA = 0, valueB = translateForwardsPerRib * 0.5f, facesel = FaceSelections.FacingStraightForward});
            poly = poly.Loft(new OpParams{valueA = -ribDepth, valueB = translateForwardsPerRib * 0.25f, facesel = FaceSelections.FacingStraightForward});
            poly = poly.Loft(new OpParams{valueA = 0, valueB = translateForwardsPerRib * 0.25f, facesel = FaceSelections.FacingStraightForward});
        }

        return poly;
    }
    
    private ConwayPoly MakeWings(ConwayPoly spaceship)
    {
        if (Random.value < ChanceOfWings)
        {
            var wingFaces = spaceship.Duplicate();
            wingFaces = wingFaces.FaceKeep(new OpParams{facesel = FaceSelections.AllNew});
            wingFaces = wingFaces.FaceKeep(new OpParams{facesel = FaceSelections.FacingLevel});
            wingFaces = wingFaces.FaceScale(new OpParams{facesel = FaceSelections.All, valueA = Random.Range(0, 0.5f)});
            wingFaces = wingFaces.Loft(new OpParams{valueA = Random.Range(0, 1f), valueB = Random.Range(.5f, 2f)});
            for (int i=0; i<Random.Range(0, 3); i++)
            {
                wingFaces = wingFaces.Loft(new OpParams{valueA = Random.Range(0, 1f), valueB = Random.Range(.15f, 1.5f), facesel = FaceSelections.Existing});
                if (Random.value < 0.5f)
                {
                    wingFaces = wingFaces.FaceSlide(new OpParams{valueA = Random.Range(-.5f, .5f), valueB = Random.Range(-1, .25f), facesel = FaceSelections.Existing});

                }
            }
            spaceship.Append(wingFaces);
        }
        return spaceship;
    }



}
