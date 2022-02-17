using System;
using System.Collections.Generic;
using System.Linq;
using Conway;
using NaughtyAttributes;
using UnityEngine;
using Random = UnityEngine.Random;

public partial class GeometryModifier
{
    
    [ShowIf(nameof(UsesFaces))]public bool EnableFilter;
    [Label("Property"), ShowIf(nameof(UsesFilterAndEnabled)), BoxGroup("Filter")] public FilterTypes FilterType;
    [Label("Axis"), ShowIf(nameof(UsesFilterAxis)), BoxGroup("Filter")] public FilterAxes FilterAxis;
    [Label("Comparison"), ShowIf(nameof(UsesFilterFloatComparison)), BoxGroup("Filter")] public FilterFloatComparisonTypes FilterFloatComparisonType;
    [Label("Comparison"), ShowIf(nameof(UsesFilterInt)), BoxGroup("Filter")] public FilterIntComparisonTypes FilterIntComparisonType;
    [Label("Value"), ShowIf(nameof(UsesFilterBool)), BoxGroup("Filter")] public bool FilterValueBool;
    [Label("Value"), ShowIf(nameof(UsesFilterFloat)), BoxGroup("Filter")] public float FilterValueFloat;
    [Label("Value"), ShowIf(nameof(UsesFilterInt)), BoxGroup("Filter")] public int FilterValueInt;
    [Label("Role"), ShowIf(nameof(UsesFilterRoles)), BoxGroup("Filter"), EnumFlags] public FilterRoles FilterValueRole;
    [Label("Distance From"), ShowIf(nameof(UsesDistanceFrom)), BoxGroup("Filter")]public FilterDistanceFromChoices filterDistanceFrom;
    [Label("Distance To"), ShowIf(nameof(UsesDistanceTo)), BoxGroup("Filter")]public FilterDistanceToChoices filterDistanceTo;
    [Label("Angle To"), ShowIf(nameof(UsesAngleTo)), BoxGroup("Filter")]public FilterAngleToChoices filterAngleTo;

    public enum FilterTypes
    {
        Role = 1,
        Sides = 3,
        Position = 0,
        Distance = 7,
        Angle = 2,
        Area = 6,
        Index = 4,
        Boundary = 5,
        Random = 9,
        Convex = 10,
    }

    public enum FilterDistanceFromChoices
    {
        FaceCenter,
        AnyVertex,
    }
    
    public enum FilterDistanceToChoices
    {
        Point,
        Axis,
        Plane
    }
    
    public enum FilterAngleToChoices
    {
        FaceCenter,
        ShapeCenter,
        Foo
    }

    // public enum FaceSelections
    // {
    //     // Sides
    //     PSided,
    //     QSided,
    //
    //     // Direction
    //     FacingLevel,
    //     FacingCenter,
    //     FacingIn,
    //     FacingOut,
    //
    // }
    
    [Flags]
    public enum FilterRoles
    {
        Ignored = 1,
        Existing = 2,
        New = 4,
        NewAlt = 8,
        ExistingAlt = 16,
    }
    
    public enum FilterAxes {X,Y,Z}
    
    public enum FilterIntComparisonTypes
    {
        LessThan,
        EqualOrLessThan,
        NotEqualTo,
        EqualTo,
        EqualOrGreaterThan,
        GreaterThan,
        Modulo,
        NotModulo
    }
    
    public enum FilterFloatComparisonTypes
    {
        LessThan,
        GreaterThan
    }
    
    private bool UsesFilterAndEnabled() => UsesFaces() && EnableFilter;
    private bool UsesFilterAxis()
    {
        if (FilterType == FilterTypes.Distance || FilterType == FilterTypes.Position)
        {
            return filterDistanceTo != FilterDistanceToChoices.Point;
        }
        return UsesFaces() && EnableFilter && GetFilterConfig()[FilterType].UsesAxis;
    }

    private bool UsesFilterFloat() => UsesFaces() &&
                                      EnableFilter &&
                                      GetFilterConfig()[FilterType].UsesFloat;
    private bool UsesFilterFloatComparison() => UsesFilterFloat() &&
                                                FilterType != FilterTypes.Random;
    private bool UsesFilterBool() => UsesFaces() &&
                                     EnableFilter &&
                                     GetFilterConfig()[FilterType].UsesBool;
    private bool UsesFilterInt() => UsesFaces() &&
                                    EnableFilter &&
                                    GetFilterConfig()[FilterType].UsesInt;
    private bool UsesFilterRoles() => UsesFaces() &&
                                      EnableFilter &&
                                      FilterType==FilterTypes.Role;
    private bool UsesDistanceTo() => UsesFaces() && 
                                          EnableFilter && 
                                          FilterType is FilterTypes.Position or 
                                                        FilterTypes.Distance;

    private bool UsesAngleTo() => UsesFaces() &&
                                     EnableFilter &&
                                     FilterType is FilterTypes.Angle;
    private bool UsesDistanceFrom() => UsesFaces() && 
                                    EnableFilter && 
                                    FilterType is FilterTypes.Position or 
                                                  FilterTypes.Distance;
    private struct FilterConfigItem
    {
        public bool UsesFloat;
        public bool UsesInt;
        public bool UsesAxis;
        public bool UsesBool;
    };
    
    private static Dictionary<FilterTypes, FilterConfigItem> _FilterConfig;

    private static Dictionary<FilterTypes, FilterConfigItem> GetFilterConfig()
    {

        return _FilterConfig ??= new Dictionary<FilterTypes, FilterConfigItem>
        {
            {
                FilterTypes.Position,
                new FilterConfigItem {UsesAxis = true, UsesFloat = true}
            },
            {
                FilterTypes.Role,
                new FilterConfigItem {}
            },
            {
                FilterTypes.Angle,
                new FilterConfigItem {UsesAxis = true, UsesFloat = true}
            },
            {
                FilterTypes.Sides,
                new FilterConfigItem {UsesInt = true}
            },
            {
                FilterTypes.Index,
                new FilterConfigItem {UsesInt = true}
            },
            {
                FilterTypes.Area,
                new FilterConfigItem {UsesFloat = true}
            },
            {
                FilterTypes.Distance,
                new FilterConfigItem {UsesFloat = true, UsesAxis = true}
            },
            {
                FilterTypes.Boundary,
                new FilterConfigItem {UsesBool = true}
            },
            {
                FilterTypes.Random,
                new FilterConfigItem {UsesFloat = true}
            },
            {
                FilterTypes.Convex,
                new FilterConfigItem {UsesBool = true}
            },
        };
    }

    public Func<FilterParams, bool> GetFilter(ConwayPoly poly)
    {
        List<Vector3> GetPointsForDistanceComparison(FilterParams p)
        {
            var points = new List<Vector3>();
            switch (filterDistanceFrom)
            {
                case FilterDistanceFromChoices.FaceCenter:
                    points.Add(poly.Faces[p.index].Centroid);
                    break;
                case FilterDistanceFromChoices.AnyVertex:
                    points.AddRange(poly.Faces[p.index].GetVertices().Select(x => x.Position));
                    break;
            }
            return points;
        }

        int floatComparisonCode;
        switch (FilterFloatComparisonType)
        {
            case FilterFloatComparisonTypes.GreaterThan:
                floatComparisonCode = 1;
                break;
            case FilterFloatComparisonTypes.LessThan:
                floatComparisonCode = -1;
                break;
            default:
                floatComparisonCode = 0;
                break;
        }

        float GetFloatFromVector(Vector3 pos)
        {
            Vector3 AxisVector(Vector3 vec)
            {
                switch (FilterAxis)
                {
                    case FilterAxes.X:
                        vec.x = 0;
                        break;
                    case FilterAxes.Y:
                        vec.y = 0;
                        break;
                    case FilterAxes.Z:
                        vec.z = 0;
                        break;
                }
                return vec;
            }
            
            Vector3 PlaneVector(Vector3 vec)
            {
                switch (FilterAxis)
                {
                    case FilterAxes.X:
                        vec = new Vector3(vec.x, 0, 0);
                        break;
                    case FilterAxes.Y:
                        vec = new Vector3(0, vec.y, 0);
                        break;
                    case FilterAxes.Z:
                        vec = new Vector3(0, 0, vec.z);
                        break;
                }
                return vec;
            }
            
            switch (filterDistanceTo)
            {
                case FilterDistanceToChoices.Point:
                    return pos.magnitude;
                case FilterDistanceToChoices.Axis:
                    return AxisVector(pos).magnitude;
                case FilterDistanceToChoices.Plane:
                    return PlaneVector(pos).magnitude;
            }

            return 0;
        }

        float GetAngleForComparison(FilterParams p)
        {

            Vector3 MaskVector(Vector3 vec)
            {
                switch (FilterAxis)
                {
                    case FilterAxes.X:
                        vec = new Vector3(vec.x, 0, 0);
                        break;
                    case FilterAxes.Y:
                        vec = new Vector3(0, vec.y, 0);
                        break;
                    case FilterAxes.Z:
                        vec = new Vector3(0, 0, vec.z);
                        break;
                }
                return vec;
            }

            float angle = 0;
            Vector3 vec = Vector3.up;
            
            switch (filterAngleTo)
            {
                case FilterAngleToChoices.Foo:
                    vec = (p.poly.Faces[p.index].Centroid - p.poly.GetCentroid()) - p.poly.Faces[p.index].Normal;
                    break;
                case FilterAngleToChoices.ShapeCenter:
                    vec = p.poly.Faces[p.index].Centroid;
                    break;
                case FilterAngleToChoices.FaceCenter:
                    vec = p.poly.Faces[p.index].Normal;
                    break;
            }

            return PolyUtils.ActualMod(Mathf.Tan(vec[(int)FilterAxis]) * Mathf.Rad2Deg, 180f);
        }

        switch (FilterType)
        {
            case FilterTypes.Position:
                return p => GetPointsForDistanceComparison(p)
                    .Select(p => p[(int)FilterAxis])
                    .Any(x=>x.CompareTo(FilterValueFloat) == floatComparisonCode);
            case FilterTypes.Distance:
                return p => GetPointsForDistanceComparison(p)
                    .Select(p => GetFloatFromVector(p))
                    .Any(x=>x.CompareTo(FilterValueFloat) == floatComparisonCode);
            case FilterTypes.Angle:
                return p => 
                    GetAngleForComparison(p)
                    .CompareTo(FilterValueFloat) == floatComparisonCode;
            case FilterTypes.Role:
                return p =>  (1 << (int)poly.FaceRoles[p.index] & (int)FilterValueRole) != 0;
            case FilterTypes.Sides:
                return p => CompareInt(poly.Faces[p.index].Sides, FilterValueInt, FilterIntComparisonType);
            case FilterTypes.Index:
                return p => CompareInt(p.index, FilterValueInt, FilterIntComparisonType);
            case FilterTypes.Area:
                return p => poly.Faces[p.index].GetArea().CompareTo(FilterValueFloat) == floatComparisonCode;
            case FilterTypes.Boundary:
                return p => poly.Faces[p.index].HasNakedEdge()==FilterValueBool;
            case FilterTypes.Random:
                return p => Random.value > FilterValueFloat;
            case FilterTypes.Convex:
                return p => poly.Faces[p.index].IsConvex==FilterValueBool;
            default:
                return null;
        }
    }

    private bool CompareInt(int val, int filterVal, FilterIntComparisonTypes type)
    {
        switch (type)
        {
            case FilterIntComparisonTypes.LessThan:
                return val < filterVal;
            case FilterIntComparisonTypes.EqualOrLessThan:
                return val <= filterVal;
            case FilterIntComparisonTypes.NotEqualTo:
                return val != filterVal;
            case FilterIntComparisonTypes.EqualTo:
                return val == filterVal;
            case FilterIntComparisonTypes.EqualOrGreaterThan:
                return val >= filterVal;
            case FilterIntComparisonTypes.GreaterThan:
                return val > filterVal;
            case FilterIntComparisonTypes.Modulo:
                return val % filterVal == 0;
            case FilterIntComparisonTypes.NotModulo:
                return val % filterVal != 0;
            default:
                return false;
        }
        
    }
}