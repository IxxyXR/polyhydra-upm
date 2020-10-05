using System;
using System.Collections.Generic;
using UnityEditor;


namespace Conway
{
    public partial class ConwayPoly
    {

        public void InitTags()
        {
            FaceTags = new List<HashSet<Tuple<string, TagType>>>();
            for (var i = 0; i < Faces.Count; i++)
            {
                FaceTags.Add(new HashSet<Tuple<string, TagType>>());
            }
        }

        public void ClearTags()
        {
            InitTags();
        }

        public void TagFaces(string tags, FaceSelections facesel=FaceSelections.All, Func<FilterParams, bool> filter = null, bool introvert=false)
        {
            
            if (FaceTags == null || FaceTags.Count == 0)
            {
                InitTags();
            }
            
            var newTagList = OpParams.TagListFromString(tags, introvert);

            int counter = 0;
            for (var i = 0; i < Faces.Count; i++)
            {
                if (IncludeFace(i, facesel, null, filter))
                {
                    var existingTagSet = FaceTags[i];
                    existingTagSet.UnionWith(newTagList);
                    FaceTags[i] = existingTagSet;
                    counter++;
                }
            }
        }

        public List<int> GetFaceSelection(FaceSelections facesel)
        {
            var selectedFaces = new List<int>();
            for (var faceIndex = 0; faceIndex < Faces.Count; faceIndex++)
            {
                var face = Faces[faceIndex];
                if (IncludeFace(faceIndex, facesel))
                {
                    selectedFaces.Add(faceIndex);
                }
            }

            return selectedFaces;
        }
    }
}