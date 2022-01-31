using System.Collections.Generic;
using Conway;
using NaughtyAttributes;
using UnityEngine;

public class BaseGenerator : BaseComponent
{
    [Foldout("Transform")] public Vector3 Position = Vector3.zero;
    [Foldout("Transform")] public Vector3 Rotation = Vector3.zero;
    [Foldout("Transform")] public Vector3 Scale = Vector3.one;

    protected ConwayPoly poly;

    public virtual ConwayPoly Generate()
    {
        if (Position != Vector3.zero || Rotation != Vector3.zero || Scale != Vector3.one)
        {
            poly = poly.Transform(Position, Rotation, Scale);
        }
        return poly;
    }
    
    
    [ContextMenu("Add Grid")]
    private void AddGrid()
    {
#if UNITY_EDITOR
        gameObject.AddComponent<GenerateGrid>();
        MoveLastComponentToMe();
#endif
    }
    
    [ContextMenu("Add Wythoff")]
    private void AddWythoff()
    {
#if UNITY_EDITOR
        gameObject.AddComponent<GenerateWythoff>();
        MoveLastComponentToMe();
#endif
    }

    [ContextMenu("Add Other")]
    private void AddOther()
    {
#if UNITY_EDITOR
        gameObject.AddComponent<GenerateOther>();
        MoveLastComponentToMe();
#endif
    }

    [ContextMenu("Add Kepler Grid")]
    private void AddKeplerGrid()
    {
#if UNITY_EDITOR
        gameObject.AddComponent<GenerateKeplerGrid>();
        MoveLastComponentToMe();
#endif
    }

    [ContextMenu("Add Johnson")]
    private void AddJohnson()
    {
#if UNITY_EDITOR
        gameObject.AddComponent<GenerateJohnson>();
        MoveLastComponentToMe();
#endif
    }
}