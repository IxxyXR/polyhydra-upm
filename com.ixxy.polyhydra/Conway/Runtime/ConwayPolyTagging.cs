using System;
using System.Collections.Generic;

namespace Conway
{
    public partial class ConwayPoly
    {
        #region tag methods

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
            var tagList = StringToTagList(tags, true);
            if (FaceTags == null || FaceTags.Count == 0)
            {
                InitTags();
            }

            for (var i = 0; i < Faces.Count; i++)
            {
                var tagset = FaceTags[i];
                if (IncludeFace(i, facesel, tagList, filter))
                {
                    tagset.UnionWith(tagList);
                }
            }
        }

        #endregion
    }
}