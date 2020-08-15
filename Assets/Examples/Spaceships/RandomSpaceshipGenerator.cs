using Conway;
using Johnson;
using UnityEngine;
using Random = UnityEngine.Random;


[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class RandomSpaceshipGenerator : MonoBehaviour
{

    public Material material;
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
    [Range(0f, 1f)]
    public float ChanceOfSharpNose = 0.25f;
    [Range(0f, 1f)]
    public float ChanceOfEngineVariant = 0.5f;




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
        int numSides = Random.Range(4, 8);
        var spaceship = JohnsonPoly.Prism(numSides);
        var wings = new ConwayPoly();
        float angleCorrection = 180f / numSides;
        if (numSides % 2 != 0) angleCorrection /= 2f;
        spaceship = spaceship.Rotate(Vector3.up, angleCorrection);
        spaceship = spaceship.Rotate(Vector3.left, -90);

        float loftLow = -.25f;
        float loftHigh = 0.75f;

        void MakeWings()
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
                wings.Append(wingFaces);
            }
        }

        for (int i = 0; i < 2; i++)
        {
            int numSections = Random.Range(2, 5);
            for (int j = 0; j <= numSections; j++)
            {
                if (Random.value < ChanceOfSimpleSegment)
                {
                    spaceship = spaceship.Loft(new OpParams{valueA = Random.Range(loftLow, loftHigh), valueB = Random.Range(.2f, .5f), facesel = FaceSelections.FacingStraightForward});
                    MakeWings();
                }
                else
                {
                    if (Random.value < ChanceOfLaceSegment)
                    {
                        spaceship = spaceship.Lace(new OpParams{valueA = Random.Range(loftLow, loftHigh), facesel = FaceSelections.FacingStraightForward, valueB = Random.Range(.2f, .5f)});
                        MakeWings();
                    }
                    else if (Random.value < ChanceOfTruncateSegment)
                    {
                        spaceship = spaceship.Truncate(new OpParams{valueA = Random.Range(loftLow, loftHigh), facesel = FaceSelections.FacingForward});
                        MakeWings();
                    }
                    else
                    {
                        spaceship = RibbedExtrude(spaceship, Random.Range(2, 7));
                    }
                }

                if (Random.value < ChanceOfFins)
                {
                    spaceship = spaceship.Loft(new OpParams{valueA = Random.Range(.5f, 0), valueB = Random.Range(0.05f, .3f), facesel = FaceSelections.AllNew});
                }

                spaceship = spaceship.FaceSlide(new OpParams{valueA = Random.Range(-.3f, .3f), valueB = 0, facesel = FaceSelections.FacingStraightForward});

            }
            spaceship = spaceship.Rotate(Vector3.up, 180);

            loftLow = -0.35f;
            loftHigh = 0.15f;

        }

        // Nose
        if (Random.value < ChanceOfSharpNose)
        {
            spaceship = spaceship.Kis(new OpParams{valueA = Random.Range(-.2f, 2f), facesel = FaceSelections.FacingStraightForward});
        }

        spaceship = spaceship.Loft(new OpParams{valueA = 0.1f, valueB = 0.025f});

        // Engines
        spaceship = spaceship.Rotate(Vector3.up, 180);
        spaceship = spaceship.Loft(new OpParams{valueA = Random.Range(.3f, .4f), valueB = Random.Range(-.2f, .2f), facesel = FaceSelections.FacingStraightForward});
        if (Random.value < ChanceOfEngineVariant)
        {
            spaceship = spaceship.Stake(new OpParams{valueA = Random.Range(.2f, .75f), facesel = FaceSelections.Existing});
            spaceship = spaceship.Loft(new OpParams{valueA = Random.Range(.1f, .3f), valueB = Random.Range(-.3f, -.7f), facesel = FaceSelections.AllNew});
        }
        else
        {
            spaceship = spaceship.Loft(new OpParams{valueA = Random.Range(.5f, .5f), valueB = Random.Range(.2f, .5f), facesel = FaceSelections.Existing});
            spaceship = spaceship.Loft(new OpParams{valueA = Random.Range(.1f, .3f), valueB = Random.Range(-.3f, -.7f), facesel = FaceSelections.AllNew});
        }
        spaceship = spaceship.Rotate(Vector3.up, 180);


        //spaceship = spaceship.Kis(1f, FaceSelections.FacingForward);
        wings = wings.Loft(new OpParams{valueA = 0.1f, valueB = 0.025f});
        spaceship.Append(wings);

        var mesh = PolyMeshBuilder.BuildMeshFromConwayPoly(spaceship, false);
        GetComponent<MeshFilter>().mesh = mesh;
        GetComponent<MeshRenderer>().material = material;
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

}
