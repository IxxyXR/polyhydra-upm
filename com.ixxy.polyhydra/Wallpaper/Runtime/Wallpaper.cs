// Based on https://github.com/hwatheod/wallpaper


using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Polygon
{
    // Polygon coordinates.
    public Vector2[] points;

    // Number of sides in the polygon.
    public int polySides;

    /**
     * @param points {{0,0}, {10,0}, {10,10}, {0,10}} Coordinates of points in order (clockwise or counterclockwise).
     * @param offsetX X offset to be applied to each point.
     * @param offsetY Y offset to be applied to each point.
     */

    public Polygon(Vector2[] _points, float offsetX, float offsetY)
    {
        if (_points.Length < 1)
        {
            Debug.LogError("Empty polygon");
            return;
        }
        polySides = _points.Length;
        points = _points.Select(v=>new Vector2(v.x+offsetX, v.y+offsetY)).ToArray();
    }
}


public class SymmetryGroup {
    
    private static Dictionary<R, string> conwayGroupSymbolMap = new Dictionary<R, string>()
    {
        {R.p1, "o"},
        {R.p2, "2222"},
        {R.pm, "**"},
        {R.pg, "xx"},
        {R.cm, "*x"},
        {R.pmm, "*2222"},
        {R.pmg, "22*"},
        {R.pgg, "22x"},
        {R.cmm, "2*22"},
        {R.p4, "442"},
        {R.p4m, "*442"},
        {R.p4g, "4*2"},
        {R.p3, "333"},
        {R.p3m1, "*333"},
        {R.p31m, "3*3"},
        {R.p6, "632"},
        {R.p6m, "*632"},
    };
    private static Dictionary<R, string> crystallographicGroupSymbolMap = new Dictionary<R, string>()
    {
        {R.p1, "p1"},
        {R.pg, "pg"},
        {R.cm, "cm"},
        {R.pm, "pm"},
        {R.p6, "p6"},
        {R.p6m, "p6mm"},
        {R.p3, "p3"},
        {R.p3m1, "p3m1"},
        {R.p31m, "p31m"},
        {R.p4, "p4"},
        {R.p4m, "p4mm"},
        {R.p4g, "p4mg"},
        {R.p2, "p2"},
        {R.pgg, "p2gg"},
        {R.pmg, "p2mg"},
        {R.pmm, "p2mm"},
        {R.cmm, "c2mm"},
    };

    public Polygon fundamentalRegion; // fundamental region for the symmetry group
    public Vector2 center;  // center of fundamental tile for the translation subgroup
    private Vector2 translationX;
    private Vector2 translationY;  // the 2 translation vectors for the translation subgroup
    private Matrix4x4[] cosetReps; // coset representatives of the translation subgroup in the symmetry group,
                        // which can be applied to the fundamental region to get the fundamental tile
                        // Identity matrix is NOT included.
    private R id;
    
    private Matrix4x4 setReflectionMatrix(Vector2 p1, Vector2 p2)
    {
        
        var plane = new Plane();
        plane.Set3Points(
            new Vector3(p1.x, p1.y, 0),
            new Vector3(p2.x, p2.y, 0),
            new Vector3(p1.x, p1.y, 1.1f)
        );
        var normals = plane.normal;
        
        var reflectionMat = new Matrix4x4();
        reflectionMat.m00 = (1F - 2F * normals.x * normals.x);
        reflectionMat.m01 = (-2F * normals.x * normals.y);
        reflectionMat.m02 = (-2F * normals.x * normals.z);
        reflectionMat.m03 = (-2F * plane.distance * normals.x);

        reflectionMat.m10 = (-2F * normals.y * normals.x);
        reflectionMat.m11 = (1F - 2F * normals.y * normals.y);
        reflectionMat.m12 = (-2F * normals.y * normals.z);
        reflectionMat.m13 = (-2F * plane.distance * normals.y);

        reflectionMat.m20 = (-2F * normals.z * normals.x);
        reflectionMat.m21 = (-2F * normals.z * normals.y);
        reflectionMat.m22 = (1F - 2F * normals.z * normals.z);
        reflectionMat.m23 = (-2F * plane.distance * normals.z);

        reflectionMat.m30 = 0F;
        reflectionMat.m31 = 0F;
        reflectionMat.m32 = 0F;
        reflectionMat.m33 = 1F;
        
        return reflectionMat;
    }

    public enum R
    {
        p1,
        pg,
        cm,
        pm,
        p6,
        p6m,
        p3,
        p3m1,
        p31m,
        p4,
        p4m,
        p4g,
        p2,
        pgg,
        pmg,
        pmm,
        cmm,
    }


    public static Dictionary<R, string> getConwayGroupSymbolMap() {
        return conwayGroupSymbolMap;
    }

    public string getConwaySymbol() {
        return conwayGroupSymbolMap[id];
    }
    
    public Matrix4x4 setRotate(float rotation, Vector2 center)
    {
        Ray axis = new Ray(Vector3.zero, new Vector3(center.x, center.y, 0));
        var mat = Matrix4x4.TRS(
            -axis.origin,
            Quaternion.AngleAxis(rotation, Vector3.forward),
            Vector3.one
        );
        return mat * Matrix4x4.TRS(axis.origin, Quaternion.identity, Vector3.one);
    }

    public string getCrystallographicSymbol() {
        return crystallographicGroupSymbolMap[id];
    }

    public SymmetryGroup(R symmetryGroupId, Vector2 center) {
        
        id = symmetryGroupId;
        
        float d1x, d1y, d2x, d2y, offsetX, offsetY;
        
        switch(symmetryGroupId) {
            
            case R.p1:                
                d1x = 2;
                d1y = 0;
                d2x = .8f;
                d2y = 2;
                offsetX = center.x/2 - (d1x + d2x) / 2;
                offsetY = center.y/2 - (d1y + d2y) / 2;
                fundamentalRegion = new Polygon(new []
                {
                    new Vector2(0f, 0f),
                    new Vector2(d1x, d1y),
                    new Vector2(d1x+d2x, d1y+d2y),
                   new Vector2(d2x, d2y)
                }, offsetX, offsetY);
                center = new Vector2(
                    (d1x + d2x) / 2 + offsetX,
                    (d1y + d2y) / 2 + offsetY
                );
                translationX = new Vector2(d1x, d2x);
                translationY = new Vector2(d1y, d2y);
                cosetReps = new Matrix4x4[] {};
                break;
                
            case R.p2:
                d1x = 2;
                d1y = 0;
                d2x = .8f;
                d2y = 2;
                offsetX = center.x/2 - (d1x + d2x)/2;
                offsetY = center.y/2 - (d1y + d2y)/2;
                fundamentalRegion = new Polygon(new [] {
                    new Vector2(0,0),
                    new Vector2(d1x, d1y),
                    new Vector2(d1x+d2x, d1y+d2y),
                    new Vector2(d2x, d2y)
                }, offsetX, offsetY);
                center = new Vector2(
                    d1x / 2 + offsetX,
                    d1y / 2 + offsetY
                );
                translationX = new Vector2(d1x, d2x*2);
                translationY = new Vector2(d1y, d2y*2);
                cosetReps = new Matrix4x4[1];
                cosetReps[0] = setRotate(180, center);
                break;
                
            case R.p3:
                float hexSize = 3;
                d1x = 3*hexSize/4;
                d1y = hexSize * ((float)Math.Sqrt(3)/4);
                d2x = d1x;
                d2y = -d1y;
                offsetX = center.x/2;
                offsetY = center.y/2;
                fundamentalRegion = new Polygon(new [] {
                    new Vector2(0,0),
                    new Vector2(hexSize/2 * 1/2, hexSize/2 * (float)(Math.Sqrt(3)/2)),
                    new Vector2(hexSize/2,0),
                    new Vector2(hexSize/2 * 1/2, -hexSize/2 * (float)Math.Sqrt(3)/2)
                }, offsetX, offsetY);
                center = new Vector2(center.x/2, center.y/2);
                translationX = new Vector2(d1x, d2x);
                translationY = new Vector2(d1y, d2y);
                cosetReps = new Matrix4x4[2];
                cosetReps[0] = setRotate(120, center);
                cosetReps[1] = setRotate(240, center);
                break;
                
            case R.p4:                
                float squareSize = 3;
                d1x = squareSize;
                d1y = 0;
                d2x = 0;
                d2y = squareSize;
                offsetX = center.x/2;
                offsetY = center.y/2;
                fundamentalRegion = new Polygon(new [] {
                    new Vector2(0,0),
                    new Vector2(d2x/2, d2y/2),
                    new Vector2((d1x+d2x)/2,(d1y+d2y)/2),
                    new Vector2(d1x/2, d1y/2)
                }, offsetX, offsetY);
                center = new Vector2(
                    center.x/2,
                    center.y/2
                );
                translationX = new Vector2(d1x, d2x);
                translationY = new Vector2(d1y, d2y);
                cosetReps = new Matrix4x4[3];
                cosetReps[0] = setRotate(90, center);
                cosetReps[1] = setRotate(180, center);
                cosetReps[2] = setRotate(270, center);
                break;
                
            case R.p6:
                hexSize = 4;
                d1x = 3*hexSize/4;
                d1y = hexSize * ((float)Math.Sqrt(3)/4);
                d2x = d1x;
                d2y = -d1y;
                offsetX = center.x/2;
                offsetY = center.y/2;
                fundamentalRegion = new Polygon(new [] {
                    new Vector2(0,0),
                    new Vector2(0, hexSize/2 * (float)Math.Sqrt(3)/2),
                    new Vector2(hexSize/4,hexSize/2 * (float)Math.Sqrt(3)/2),
                    new Vector2(3*hexSize/8, hexSize*(float)Math.Sqrt(3)/8)
                }, offsetX, offsetY);
                center = new Vector2(
                    center.x/2,
                    center.y/2
                );
                translationX = new Vector2(d1x, d2x);
                translationY = new Vector2(d1y, d2y);
                cosetReps = new Matrix4x4[5];
                for (int i=0; i<5; i++) {
                    cosetReps[i] = setRotate(60 * (i+1), center);
                }
                break;
                
            case R.pmm:
                float dx = 4;
                float dy = 2;
                offsetX = center.x/2 - dx/4;
                offsetY = center.y/2 - dy/4;
                fundamentalRegion = new Polygon(new [] {
                    new Vector2(0,0),
                    new Vector2(dx/2, 0),
                    new Vector2(dx/2, dy/2),
                    new Vector2(0, dy/2)
                }, offsetX, offsetY);
                center = new Vector2(
                    center.x/2,
                    center.y/2
                );
                translationX = new Vector2(dx, 0);
                translationY = new Vector2(0, dy);
                cosetReps = new Matrix4x4[3];
                cosetReps[0] = setReflectionMatrix(fundamentalRegion.points[0], fundamentalRegion.points[1]);
                cosetReps[1] = setReflectionMatrix(fundamentalRegion.points[0], fundamentalRegion.points[3]);
                cosetReps[2] = setRotate(180, new Vector2(offsetX, offsetY));
                break;
                
            case R.p3m1:
                hexSize = 5;
                d1x = 3*hexSize/4;
                d1y = hexSize * ((float)Math.Sqrt(3)/4);
                d2x = d1x;
                d2y = -d1y;
                offsetX = center.x/2;
                offsetY = center.y/2;
                fundamentalRegion = new Polygon(new [] {
                    new Vector2(0,0),
                    new Vector2(hexSize/2 * 1/2, hexSize/2 * (float)(Math.Sqrt(3)/2) ),
                    new Vector2(hexSize/2,0)
                }, offsetX, offsetY);
                center = new Vector2(
                    center.x/2,
                    center.y/2
                );
                translationX = new Vector2(d1x, d2x);
                translationY = new Vector2(d1y, d2y);
                cosetReps = new Matrix4x4[5];
                cosetReps[0] = setRotate(120, center);
                cosetReps[1] = setRotate(240, center);
                
                cosetReps[2] = setReflectionMatrix(fundamentalRegion.points[2], fundamentalRegion.points[0]);
                cosetReps[3] = cosetReps[2];
                cosetReps[3] = cosetReps[0] * cosetReps[3];
                cosetReps[4] = cosetReps[2];
                cosetReps[4] = cosetReps[1] * cosetReps[4];
                break;
                
            case R.p4m:
                squareSize = 4;
                d1x = squareSize;
                d1y = 0;
                d2x = 0;
                d2y = squareSize;
                offsetX = center.x/2;
                offsetY = center.y/2;
                fundamentalRegion = new Polygon(new [] {
                    new Vector2(0,0),
                    new Vector2(d2x/2, d2y/2),
                    new Vector2((d1x+d2x)/2,(d1y+d2y)/2)
                }, offsetX, offsetY);
                center = new Vector2(
                    center.x/2,
                    center.y/2
                );
                translationX = new Vector2(d1x, d2x);
                translationY = new Vector2(d1y, d2y);
                cosetReps = new Matrix4x4[7];
                cosetReps[0] = setRotate(90, center);
                cosetReps[1] = setRotate(180, center);
                cosetReps[2] = setRotate(270, center);
                
                cosetReps[3] = setReflectionMatrix(fundamentalRegion.points[1], fundamentalRegion.points[2]);
                for (int i=4; i<7; i++) {
                    cosetReps[i] = cosetReps[3];
                    cosetReps[i] = cosetReps[i-4] * cosetReps[i];
                }
                break;
                
            case R.p6m:
                hexSize = 5;
                d1x = 3*hexSize/4;
                d1y = hexSize * ((float)Math.Sqrt(3)/4);
                d2x = d1x;
                d2y = -d1y;
                offsetX = center.x/2;
                offsetY = center.y/2;
                fundamentalRegion = new Polygon(new [] {
                    new Vector2(0,0),
                    new Vector2(0, hexSize/2 * (float)Math.Sqrt(3)/2),
                    new Vector2(hexSize/4,hexSize/2 * (float)Math.Sqrt(3)/2)
                }, offsetX, offsetY);
                center = new Vector2(
                    center.x/2,
                    center.y/2
                );
                translationX = new Vector2(d1x, d2x);
                translationY = new Vector2(d1y, d2y);
                cosetReps = new Matrix4x4[11];
                for (int i=0; i<5; i++ ) {
                    cosetReps[i] = setRotate(60 * (i+1), center);
                }
                
                cosetReps[5] = setReflectionMatrix(fundamentalRegion.points[0], fundamentalRegion.points[2]);
                for (int i=6; i<11; i++) {
                    cosetReps[i] = cosetReps[5];
                    cosetReps[i] = cosetReps[i-6] * cosetReps[i];
                }
                break;
                
            case R.pm:
                dx = 3;
                dy = 1.2f;
                offsetX = center.x/2 - dx / 4;
                offsetY = center.y/2 - dy/2;
                fundamentalRegion = new Polygon(new []{
                    new Vector2(0,0),
                    new Vector2(dx/2, 0),
                    new Vector2(dx/2, dy),
                    new Vector2(0, dy)
                }, offsetX, offsetY);
                center = new Vector2(
                    center.x/2,
                    center.y/2
                );
                translationX = new Vector2(dx, 0);
                translationY = new Vector2(0, dy);
                cosetReps = new Matrix4x4[1];
                
                cosetReps[0] = setReflectionMatrix(fundamentalRegion.points[0], fundamentalRegion.points[3]);
                break;
                
            case R.cm:
                dx = 1.5f;
                dy = 1.2f;
                offsetX = center.x/2 - dx/2;
                offsetY = center.y/2 - dy/2;
                fundamentalRegion = new Polygon(new []{
                    new Vector2(0,0),
                    new Vector2(dx, 0),
                    new Vector2(dx, dy),
                    new Vector2(0, dy)
                }, offsetX, offsetY);
                center = new Vector2(
                    center.x/2,
                    center.y/2
                );
                translationX = new Vector2(dx, dx);
                translationY = new Vector2(dy, -dy);
                cosetReps = new Matrix4x4[1];
                
                cosetReps[0] = setReflectionMatrix(fundamentalRegion.points[0], fundamentalRegion.points[3]);
                break;
                
            case R.pg:
                dx = 1.5f;
                dy = 1.2f;
                offsetX = center.x/2 - dx/2;
                offsetY = center.y/2 - dy/2;
                fundamentalRegion = new Polygon(new []{
                    new Vector2(0,0),
                    new Vector2(dx, 0),
                    new Vector2(dx, dy),
                    new Vector2(0, dy)
                }, offsetX, offsetY);
                center = new Vector2(
                    center.x/2,
                    center.y/2
                );
                translationX = new Vector2(dx, 0);
                translationY = new Vector2(0, 2*dy);
                cosetReps = new Matrix4x4[1];
                
                cosetReps[0] = setReflectionMatrix(new Vector2(dx/2 + offsetX, 0 + offsetY), new Vector2(dx/2 + offsetX, dy + offsetY));
                cosetReps[0] = Matrix4x4.Translate(new Vector3(0, dy, 0)) * cosetReps[0];
                break;
                
            case R.pmg:
                dx = 1.5f;
                dy = 1.2f;
                offsetX = center.x/2 - dx/2;
                offsetY = center.y/2 - dy/2;
                fundamentalRegion = new Polygon(new [] {
                    new Vector2(0,0),
                    new Vector2(dx, 0),
                    new Vector2(dx, dy),
                    new Vector2(0, dy)
                }, offsetX, offsetY);
                center = new Vector2(
                    center.x/2,
                    center.y/2
                );
                translationX = new Vector2(2*dx, 0);
                translationY = new Vector2(0, 2*dy);
                cosetReps = new Matrix4x4[3];
                
                cosetReps[0] = setReflectionMatrix(fundamentalRegion.points[1], fundamentalRegion.points[2]);
                cosetReps[1] = setRotate(
                    180,
                    new Vector2(dx/2 + offsetX, 0 + offsetY)
                );
                cosetReps[2] = cosetReps[1];
                cosetReps[2] = cosetReps[0] * cosetReps[2];
                break;
                
            case R.pgg:
                dx = 1.5f;
                dy = 1.2f;
                offsetX = center.x/2 - dx/2;
                offsetY = center.y/2 - dy/2;
                fundamentalRegion = new Polygon(new []{
                    new Vector2(0,0),
                    new Vector2(dx, 0),
                    new Vector2(dx, dy),
                    new Vector2(0, dy)
                }, offsetX, offsetY);
                center = new Vector2(
                    center.x/2,
                    center.y/2
                );
                translationX = new Vector2(2*dx, 0);
                translationY = new Vector2(0, 2*dy);
                cosetReps = new Matrix4x4[3];
                
                cosetReps[0] = setReflectionMatrix(fundamentalRegion.points[1], fundamentalRegion.points[2]);
                cosetReps[0] = Matrix4x4.Translate(new Vector3(0, dy, 0)) * cosetReps[0];
                cosetReps[1] = setRotate(180, new Vector2(dx/2 + offsetX, 0 + offsetY));
                cosetReps[2] = cosetReps[1];
                cosetReps[2] = cosetReps[0] * cosetReps[2];
                break;
                
            case R.cmm:
                dx = 1.5f;
                dy = 1.2f;
                offsetX = center.x/2 - dx/2;
                offsetY = center.y/2 - dy/2;
                fundamentalRegion = new Polygon(new []{
                    new Vector2(0,0),
                    new Vector2(dx, 0),
                    new Vector2(dx, dy),
                    new Vector2(0, dy)
                }, offsetX, offsetY);
                center = new Vector2(
                    center.x/2,
                    center.y/2
                );
                translationX = new Vector2(dx, dx);
                translationY = new Vector2(2*dy, -2*dy);
                cosetReps = new Matrix4x4[3];
                
                cosetReps[0] = setReflectionMatrix(fundamentalRegion.points[1], fundamentalRegion.points[2]);
                cosetReps[1] = setRotate(180, new Vector2(dx/2 + offsetX, 0 + offsetY));
                cosetReps[2] = cosetReps[0];
                cosetReps[2] = cosetReps[1] * cosetReps[2];
                break;
                
            case R.p31m:
                float baseSize = 3;
                offsetX = center.x/2 - baseSize / 2;
                offsetY = center.y/2;
                fundamentalRegion = new Polygon(new []{
                    new Vector2(0,0),
                    new Vector2(baseSize, 0),
                    new Vector2(baseSize/2, (baseSize / 2) * (float)Math.Sqrt(3)/3)
                }, offsetX, offsetY);
                center = new Vector2(
                    3*baseSize / 4 + offsetX,
                    baseSize * (float)Math.Sqrt(3)/4 + offsetY
                );
                translationX = new Vector2(baseSize, baseSize/2);
                translationY = new Vector2(0, baseSize * (float)Math.Sqrt(3)/2);
                cosetReps = new Matrix4x4[5];
                cosetReps[0] = setRotate(120, new Vector2(fundamentalRegion.points[2][0], fundamentalRegion.points[2][1]));
                cosetReps[1] = setRotate(240, new Vector2(fundamentalRegion.points[2][0], fundamentalRegion.points[2][1]));
                
                cosetReps[2] = setReflectionMatrix(fundamentalRegion.points[1], center);
                cosetReps[3] = cosetReps[0];
                cosetReps[3] = cosetReps[2] * cosetReps[3];
                cosetReps[4] = cosetReps[1];
                cosetReps[4] = cosetReps[2] * cosetReps[4];
                break;
                
            case R.p4g:
                squareSize = 1.5f;
                offsetX = center.x/2 - squareSize / 2;
                offsetY = center.y/2 - squareSize / 2;
                fundamentalRegion = new Polygon(new []{
                    new Vector2(0,0),
                    new Vector2(0,squareSize),
                    new Vector2(squareSize,squareSize),
                    new Vector2(squareSize,0)
                }, offsetX, offsetY);
                center = new Vector2(
                    0 + offsetX,
                    0 + offsetY
                );
                translationX = new Vector2(2 * squareSize, 2 * squareSize);
                translationY = new Vector2(2 * squareSize, -2 * squareSize);
                cosetReps = new Matrix4x4[7];
                for (int i=0; i<3; i++) {
                    cosetReps[i] = setRotate(90 * (i + 1), center);
                }
                
                cosetReps[3] = setReflectionMatrix(fundamentalRegion.points[2], fundamentalRegion.points[3]);
                for (int i=4; i<7; i++) {
                    cosetReps[i] = cosetReps[i-4];
                    cosetReps[i] = cosetReps[3] * cosetReps[i];
                }
                break;
                
        }
    }

    public Polygon getFundamentalRegion() {
        return fundamentalRegion;
    }

    public Vector2 getTranslationX() {
        return translationX;
    }

    public Vector2 getTranslationY() {
        return translationY;
    }

    public Matrix4x4[] getCosetReps() {
        return cosetReps;
    }

    public R getId() {
        return id;
    }
}

