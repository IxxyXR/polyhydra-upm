using System;
using System.Collections.Generic;


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

        public void TagFaces(string tags, FaceSelections facesel, Func<FilterParams, bool> filter = null)
        {
            if (FaceTags == null || FaceTags.Count == 0)
            {
                InitTags();
            }

            var tagList = StringToTagList(tags, true);
            for (var i = 0; i < Faces.Count; i++)
            {
                if (IncludeFace(i, facesel, null, filter))
                {
                    var tagset = FaceTags[i];
                    tagset.UnionWith(tagList);
                    FaceTags[i] = tagset;

                }
            }
        }

    }
}