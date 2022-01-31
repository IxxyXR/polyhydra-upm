using Conway;
using Johnson;
using Wythoff;
using Grids;


public class PolyBuilder
{
    public ConwayPoly poly;
    
    private PolyHydraEnums.ShapeTypes ShapeType;
    private PolyTypes PolyType;
    private PolyHydraEnums.JohnsonPolyTypes JohnsonPolyType;
    private GridEnums.GridTypes GridType;
    private GridEnums.GridShapes GridShape;
    private PolyHydraEnums.OtherPolyTypes OtherPolyType;
    private int P, Q;

    public PolyBuilder (
        PolyHydraEnums.ShapeTypes shapeType,
        PolyTypes polyType,
        PolyHydraEnums.JohnsonPolyTypes johnsonPolyType,
        GridEnums.GridTypes gridType,
        GridEnums.GridShapes gridShape,
        PolyHydraEnums.OtherPolyTypes otherPolyType
    )
    {
        ShapeType = shapeType;
        PolyType = polyType;
        JohnsonPolyType = johnsonPolyType;
        GridType = gridType;
        GridShape = gridShape;
        OtherPolyType = otherPolyType;
    }

    public void Build(int p, int q)
    {

        P = p;
        Q = q;
        
        switch (ShapeType)
        {
            case PolyHydraEnums.ShapeTypes.Uniform:
                var wythoff = new WythoffPoly(PolyType, P, Q);
                wythoff.BuildFaces();
                poly = new ConwayPoly(wythoff);
                break;
            case PolyHydraEnums.ShapeTypes.Johnson:
                poly = JohnsonPoly.Build(JohnsonPolyType, P);
                break;
            case PolyHydraEnums.ShapeTypes.Grid:
                poly = Grids.Grids.MakeGrid(GridType, GridShape, P, Q);
                break;
            case PolyHydraEnums.ShapeTypes.Other:
                poly = JohnsonPoly.BuildOther(OtherPolyType, P, Q);
                break;
            case PolyHydraEnums.ShapeTypes.Waterman:
                poly = WatermanPoly.Build(1f, P, Q, true);
                break;
        }
    }

    public int NumberofParams()
    {
        int numParams = 0;
        
        switch (ShapeType)
        {
            case PolyHydraEnums.ShapeTypes.Uniform:
                bool isPrism = ((int)PolyType) <= 4;
                numParams = isPrism ? 2 : 0 ;
                break;
            case PolyHydraEnums.ShapeTypes.Johnson:
                numParams = 1;
                break;
            case PolyHydraEnums.ShapeTypes.Grid:
                numParams = 2;
                break;
            case PolyHydraEnums.ShapeTypes.Other:
                switch (OtherPolyType)
                {
                    case PolyHydraEnums.OtherPolyTypes.GriddedCube:
                    case PolyHydraEnums.OtherPolyTypes.UvHemisphere:
                    case PolyHydraEnums.OtherPolyTypes.UvSphere:
                        numParams = 2;
                        break;
                    case PolyHydraEnums.OtherPolyTypes.Polygon:
                        numParams = 1;
                        break;
                    case PolyHydraEnums.OtherPolyTypes.C_Shape:
                    case PolyHydraEnums.OtherPolyTypes.H_Shape:
                    case PolyHydraEnums.OtherPolyTypes.L_Shape:
                    case PolyHydraEnums.OtherPolyTypes.L_Alt_Shape:
                        numParams = 0;
                        break;
                }
                break;
            case PolyHydraEnums.ShapeTypes.Waterman:
                numParams = 3;
                break;
        }
        
        return numParams;
    }
}
