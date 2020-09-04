using System;
using UnityEngine.UIElements.Experimental;

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


        public OpParams(
            FaceSelections selection, 
            string selectByTags = "",
            bool randomValues = false 
        )
        {
            facesel = selection;
            randomize = randomValues;
            tags = selectByTags;
        }
        
        public OpParams(
            float a,
            FaceSelections selection,
            string selectByTags = "",
            bool randomValues = false 
        )
        {
            valueA = a;
            facesel = selection;
            randomize = randomValues;
            tags = selectByTags;
        }
        
        public OpParams(
            float a, float b,
            FaceSelections selection, 
            string selectByTags = "",
            bool randomValues = false 
        )
        {
            valueA = a;
            valueB = b;
            facesel = selection;
            randomize = randomValues;
            tags = selectByTags;
        }
        
        public OpParams(
            float a, float b,
            string selectByTags = "",
            bool randomValues = false 
        )
        {
            valueA = a;
            valueB = b;
            randomize = randomValues;
            tags = selectByTags;
        }
        
        public OpParams(
            float a, float b,
            Func<FilterParams, bool> selection,
            string selectByTags = "",
            bool randomValues = false 
        )
        {
            valueA = a;
            valueB = b;
            filterFunc = selection;
            randomize = randomValues;
            tags = selectByTags;
        }
        
        public OpParams(
            Func<FilterParams, float> a,
            Func<FilterParams, bool> selection,
            string selectByTags = "",
            bool randomValues = false 
        )
        {
            funcA = a;
            filterFunc = selection;
            randomize = randomValues;
            tags = selectByTags;
        }
        
        public OpParams(
            Func<FilterParams, float> a,
            string selectByTags = "",
            bool randomValues = false 
        )
        {
            funcA = a;
            randomize = randomValues;
            tags = selectByTags;
        }
        
        public OpParams(
            Func<FilterParams, float> a,
            Func<FilterParams, float> b,
            Func<FilterParams, bool> selection,
            string selectByTags = "",
            bool randomValues = false 
        )
        {
            funcA = a;
            funcB = b;
            filterFunc = selection;
            randomize = randomValues;
            tags = selectByTags;
        }
        
        public OpParams(
            float a,
            Func<FilterParams, float> b,
            Func<FilterParams, bool> selection,
            string selectByTags = "",
            bool randomValues = false 
        )
        {
            valueA = a;
            funcB = b;
            filterFunc = selection;
            randomize = randomValues;
            tags = selectByTags;
        }
        
        public OpParams(
            float a,
            Func<FilterParams, float> b,
            string selectByTags = "",
            bool randomValues = false 
        )
        {
            valueA = a;
            funcB = b;
            randomize = randomValues;
            tags = selectByTags;
        }
        
        public OpParams(
            float a,
            Func<FilterParams, bool> selection,
            string selectByTags = "",
            bool randomValues = false 
        )
        {
            valueA = a;
            filterFunc = selection;
            randomize = randomValues;
            tags = selectByTags;
        }
        
        public OpParams(
            Func<FilterParams, bool> selection,
            string selectByTags = "",
            bool randomValues = false 
        )
        {
            filterFunc = selection;
            randomize = randomValues;
            tags = selectByTags;
        }
        
        public OpParams(
            string selectByTags = "",
            bool randomValues = false 
        )
        {
            randomize = randomValues;
            tags = selectByTags;
        }
        
        public OpParams(
            float a,
            string selectByTags = "",
            bool randomValues = false 
        )
        {
            valueA = a;
            randomize = randomValues;
            tags = selectByTags;
        }

    }
}