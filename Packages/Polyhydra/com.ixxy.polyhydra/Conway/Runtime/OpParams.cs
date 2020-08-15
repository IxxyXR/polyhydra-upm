using System;

namespace Conway
{
    public class OpParams
    {
        public float valueA = 0;
        public float valueB = 0;
        public FaceSelections facesel = FaceSelections.All;
        public string tags = "";
        public bool randomize = false;
        public Func<FilterParams, float> funcA = null;
        public Func<FilterParams, float> funcB = null;
        public Func<FilterParams, bool> filterFunc;

        public float GetValueA(ConwayPoly poly, int index) => funcA?.Invoke(new FilterParams(poly, index)) ?? valueA;
        public float GetValueB(ConwayPoly poly, int index) => funcB?.Invoke(new FilterParams(poly, index)) ?? valueB;
    }
}