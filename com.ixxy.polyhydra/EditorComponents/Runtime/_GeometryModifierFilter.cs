using System;
using System.Collections.Generic;
using Conway;
using NaughtyAttributes;
using UnityEngine;

public partial class GeometryModifier
{
    
    [ShowIf(nameof(UsesFaces))]public bool EnableFilter;
    [ShowIf(nameof(UsesFilterAndEnabled)), Label("Filter")] public FaceSelections Facesel;
    [ShowIf(nameof(UsesFilterAndEnabled)), BoxGroup("Filter")] public FilterTypes FilterType;
    [ShowIf(nameof(UsesFilterFloat)), BoxGroup("Filter")] public FilterFloatComparisonTypes FilterFloatComparisonType;
    [ShowIf(nameof(UsesFilterInt)), BoxGroup("Filter")] public FilterIntComparisonTypes FilterIntComparisonType;
    [ShowIf(nameof(UsesFilterAxis)), BoxGroup("Filter")] public FilterAxes FilterAxis;
    [ShowIf(nameof(UsesFilterFloat)), BoxGroup("Filter")] public float FilterValueFloat;
    [ShowIf(nameof(UsesFilterInt)), BoxGroup("Filter")] public int FilterValueInt;
    [ShowIf(nameof(UsesFilterRoles)), BoxGroup("Filter")] public ConwayPoly.Roles FilterValueRole;

    public enum FilterTypes
    {
        Position,
        Role,
        Direction,
        Sides,
        Index,
        // Boundary,
        Area,
    }
    
    public enum FilterAxes {X,Y,Z}

    public enum FilterIntComparisonTypes
    {
        LessThan,
        EqualOrLessThan,
        NotEqualTo,
        EqualTo,
        EqualOrGreaterThan,
        GreaterThan
    }
    
    public enum FilterFloatComparisonTypes
    {
        LessThan,
        GreaterThan
    }
    
    private bool UsesFilterAndEnabled() => UsesFaces() && EnableFilter;
    private bool UsesFilterAxis() => UsesFaces() && EnableFilter && GetFilterConfig()[FilterType].UsesAxis;
    private bool UsesFilterFloat() => UsesFaces() && EnableFilter && GetFilterConfig()[FilterType].UsesFloat;
    private bool UsesFilterInt() => UsesFaces() && EnableFilter && GetFilterConfig()[FilterType].UsesInt;
    private bool UsesFilterRoles() => UsesFaces() && EnableFilter && FilterType==FilterTypes.Role;
    
    private struct FilterConfigItem
    {
        public bool UsesFloat;
        public bool UsesInt;
        public bool UsesAxis;
    };
    
    private static Dictionary<FilterTypes, FilterConfigItem> _FilterConfig;

    private static Dictionary<FilterTypes, FilterConfigItem> GetFilterConfig()
    {
        return _FilterConfig ??= new Dictionary<FilterTypes, FilterConfigItem>
        {
            {
                FilterTypes.Position,
                new FilterConfigItem {UsesAxis = true, UsesFloat = true, UsesInt = false}
            },
            {
                FilterTypes.Role,
                new FilterConfigItem {UsesAxis = false, UsesFloat = false, UsesInt = false}
            },
            {
                FilterTypes.Direction,
                new FilterConfigItem {UsesAxis = true, UsesFloat = true, UsesInt = false}
            },
            {
                FilterTypes.Sides,
                new FilterConfigItem {UsesAxis = false, UsesFloat = false, UsesInt = true}
            },
            {
                FilterTypes.Index,
                new FilterConfigItem {UsesAxis = false, UsesFloat = false, UsesInt = true}
            },
            {
                FilterTypes.Area,
                new FilterConfigItem {UsesAxis = false, UsesFloat = true, UsesInt = false}
            },
        };
    }

    public Func<FilterParams, bool> GetFilter(ConwayPoly poly)
    {
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

        switch (FilterType)
        {
            case FilterTypes.Position:
                return p => poly.Faces[p.index].Centroid[(int)FilterAxis].CompareTo(FilterValueFloat) == floatComparisonCode;
            case FilterTypes.Role:
                return p => poly.FaceRoles[p.index] == FilterValueRole;
            case FilterTypes.Direction:
                return p => poly.Faces[p.index].GetArea().CompareTo(FilterValueFloat) == floatComparisonCode;
            case FilterTypes.Sides:
                return p => CompareInt(poly.Faces[p.index].Sides, FilterValueInt, FilterIntComparisonType);
            case FilterTypes.Index:
                return p => CompareInt(p.index, FilterValueInt, FilterIntComparisonType);
            case FilterTypes.Area:
                return p => poly.Faces[p.index].GetArea().CompareTo(FilterValueFloat) == floatComparisonCode;
            default:
                return null;
        }
    }


    private bool CompareInt(int val, int filterVal, FilterIntComparisonTypes type)
    {
        switch (type)
        {
            case FilterIntComparisonTypes.LessThan:
                return val > filterVal;
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
            default:
                return false;
        }
        
    }
}