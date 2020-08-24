using System;
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
    [Range(1, 6)]
    public int numSections = 3;
    [Range(0, 3)]
    public float NoseLength = 1;
    [Range(0, 1)]
    public float NoseSharpness = 1;
    public bool NoseVariant;
    public bool EngineVariant;

    public int seed = 12345;
    
    [Range(0f, 1f)]
    public float ChanceOfLaceSegment = 0.25f;
    [Range(0f, 1f)]
    public float ChanceOfStakeSegment = 0.25f;
    [Range(0f, 1f)]
    public float ChanceOfFins = 0.25f;
    [Range(0f, 1f)]
    public float ChanceOfWings = 0.25f;
    [Range(0f, 1f)]
    public float ChanceOfRibbedSegment = 0.25f;
    [Range(0f, 1f)]
    public float ChanceOfAntenna = 0.25f;


    private float loftLow = -.25f;
    private float loftHigh = 0.75f;

    private ConwayPoly spaceship;
    private ConwayPoly wings;
    private ConwayPoly antennae;

    private bool alreadyStake;

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
        
        spaceship = JohnsonPoly.Prism(numSides);
        wings = new ConwayPoly();
        antennae = new ConwayPoly();
        
        float angleCorrection = 180f / numSides;
        if (numSides % 2 != 0) angleCorrection /= 2f;
        spaceship = spaceship.Rotate(Vector3.up, angleCorrection);
        spaceship = spaceship.Rotate(Vector3.left, -90);

        alreadyStake = false;

        for (int i = 0; i < 2; i++)  // Loop twice - once for the back and once for the front.
        {
            for (int j = 0; j <= numSections; j++)
            {
                spaceship = MakeSection(spaceship);
            }
            
            // Second time through loop:
            // Flip everything around ready to generate the back sections 
            
            spaceship = spaceship.Rotate(Vector3.up, 180);
            wings = wings.Rotate(Vector3.up, 180);
            antennae = antennae.Rotate(Vector3.up, 180);
            
            // Change random range for front sections
            loftLow = -0.35f;
            loftHigh = 0.15f;

        }


        
        // Make the engines
        var engines = spaceship.FaceKeep(new OpParams {facesel = FaceSelections.FacingStraightBackward});
        spaceship = spaceship.FaceRemove(new OpParams {facesel = FaceSelections.FacingStraightBackward});
        engines = engines.Loft(new OpParams{valueA = Random.Range(.3f, .4f), valueB = Random.Range(-.2f, .2f)});
        // spaceship = engines;
        if (EngineVariant)
        {
            var engineRim = engines.FaceRemove(new OpParams{facesel = FaceSelections.Existing});
            engines = engines.FaceKeep(new OpParams{facesel = FaceSelections.Existing});
            engines = engines.Ortho(new OpParams{valueA = 0});
            engines = engines.Loft(new OpParams{valueA = Random.Range(0, .25f), valueB = -.5f});
            engines.Append(engineRim);
        }
        else
        {
            engines = engines.Loft(new OpParams{valueA = Random.Range(.25f, .75f), valueB = Random.Range(0, .2f), facesel = FaceSelections.Existing});
            engines = engines.Loft(new OpParams{valueA = Random.Range(.1f, .3f), valueB = Random.Range(-.3f, -.7f), facesel = FaceSelections.AllNew});
        }
        
        // Make the nose section
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
        
        
        // Add panel insets to the hull
        spaceship = spaceship.Loft(new OpParams{valueA = 0.1f, valueB = 0.025f});

        // Add panel insets to the wings
        wings = wings.Loft(new OpParams{valueA = 0.1f, valueB = 0.025f});
        
        spaceship.Append(engines);
        spaceship.Append(wings);
        spaceship.Append(antennae);

        // Build the final mesh
        var mesh = PolyMeshBuilder.BuildMeshFromConwayPoly(spaceship, false);
        GetComponent<MeshFilter>().mesh = mesh;
        GetComponent<MeshRenderer>().material = material;
    }

    private ConwayPoly MakeSection(ConwayPoly spaceship)
    {

        if (Random.value < ChanceOfLaceSegment)
        {
            spaceship = spaceship.Lace(new OpParams{valueA = Random.Range(loftLow, loftHigh), facesel = FaceSelections.FacingStraightForward, valueB = Random.Range(.2f, .5f)});
        }
        else if (Random.value < ChanceOfStakeSegment && !alreadyStake)  // Only do this once
        {
            spaceship = spaceship.Stake(new OpParams{valueA = Random.Range(loftLow, loftHigh), facesel = FaceSelections.FacingForward});
            alreadyStake = true;
        }
        else if (Random.value < ChanceOfRibbedSegment)
        {
            spaceship = RibbedExtrude(spaceship, Random.Range(2, 7));
        }
        else  // Just a normal section
        {
            spaceship = spaceship.Loft(new OpParams{valueA = Random.Range(loftLow, loftHigh), valueB = Random.Range(.2f, .5f), facesel = FaceSelections.FacingStraightForward});
        
            // Each simple section can have wings or fins
            if (Random.value < ChanceOfWings)
            {
                var newWings = MakeWings(spaceship, MakeSideFaceFilter(numSides));
                wings.Append(newWings);
            }
            else if (Random.value < ChanceOfFins)
            {
                spaceship = spaceship.Loft(new OpParams{valueA = Random.Range(.5f, 0), valueB = Random.Range(0.05f, 1.0f), facesel = FaceSelections.AllNew});
            }
            else if (Random.value < ChanceOfAntenna)
            {
                antennae.Append(MakeAntenna(spaceship));
            }
            

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

    public float AngleFromNormalY(float y)
    {
        // Assumes y comes from a normalized vector
        return Mathf.Tan(y / 1f) * Mathf.Rad2Deg;
    }

    public Func<FilterParams, bool> MakeSideFaceFilter(int sides)
    {
        float sideAngle = 180f / sides;
        return p => AngleFromNormalY(Math.Abs(p.poly.Faces[p.index].Normal.y)) < sideAngle;
    }

    private ConwayPoly MakeAntenna(ConwayPoly spaceship)
    {
        var allNew = spaceship.FaceselToFaceFilterFunc(FaceSelections.AllNew);
        var facingUp = spaceship.FaceselToFaceFilterFunc(FaceSelections.FacingUp);
        Func<FilterParams, bool> newAndFacingUp = x => allNew(x) && facingUp(x);
        var topSurfaces = spaceship.FaceKeep(new OpParams{filterFunc = newAndFacingUp});
        
        var antennaGroup = new ConwayPoly();
        var antenna = JohnsonPoly.Pyramid(4);
        foreach (var face in topSurfaces.Faces)
        {
            float radius = Random.Range(0.01f, 0.05f);
            float height = Random.Range(0.25f, 2f);
            float offset = Random.value < .5 ? 0 : Random.value < 0.5 ? .5f : -.5f;
            antennaGroup.Append(antenna.Transform(
                face.GetPolarPoint(90, offset),
                Vector3.zero,
                new Vector3(radius, height, radius)
            ));
        }
        return antennaGroup;
    }

    private ConwayPoly MakeWings(ConwayPoly spaceship, Func<FilterParams, bool> sideFaceFilter)
    {
        // Creates wings positioned on the last section created
        
        var wing = spaceship.Duplicate();
        // Only keep the new section
        wing = wing.FaceKeep(new OpParams{facesel = FaceSelections.AllNew});
        // And only the faces pointing out mostly level
        wing = wing.FaceKeep(new OpParams{filterFunc = sideFaceFilter});
        // Scale them down
        wing = wing.FaceScale(new OpParams{facesel = FaceSelections.All, valueA = Random.Range(0, 0.5f)});
        // Extrude out the wings
        wing = wing.Loft(new OpParams{valueA = Random.Range(0, 1f), valueB = Random.Range(.5f, 2f)});
        for (int i=0; i<Random.Range(0, 3); i++)
        {
            wing = wing.Loft(new OpParams{valueA = Random.Range(0, 1f), valueB = Random.Range(.15f, 1.5f), facesel = FaceSelections.Existing});
            if (Random.value < 0.5f)
            {
                wing = wing.FaceSlide(new OpParams{valueA = Random.Range(-.5f, .5f), valueB = Random.Range(-1, .25f), facesel = FaceSelections.Existing});

            }
        }
    
        return wing;
    }



}
