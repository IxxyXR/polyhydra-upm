using System.Collections;
using System.Collections.Generic;
using Conway;
using Johnson;
using UnityEngine;

public class MinimalExample : MonoBehaviour
{
    
    void Start()
    {
        // 1. Create a starting shape
        ConwayPoly poly1 = JohnsonPoly.Build(PolyHydraEnums.JohnsonPolyTypes.Prism, 4);
        
        // 2. Apply an operator to the starting shape
        poly1 = poly1.ApplyOp(Ops.Chamfer, new OpParams(.3f));

        // 3. Create a second shape
        ConwayPoly poly2 = JohnsonPoly.Build(PolyHydraEnums.JohnsonPolyTypes.Pyramid, 4);

        // 4. Move our second shape down by 0.5
        poly2 = poly1.Duplicate(new Vector3(0, .5f, 0));

        // 5. Join the two shapes
        poly1.Append(poly2);

        // 6. Build and apply the mesh
        var mesh = PolyMeshBuilder.BuildMeshFromConwayPoly(poly1);
        GetComponent<MeshFilter>().mesh = mesh;
    }
    
   }