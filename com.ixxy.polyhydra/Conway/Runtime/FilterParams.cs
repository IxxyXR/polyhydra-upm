namespace Conway
{
    /// <summary>
    /// A class for manifold meshes which uses the Halfedge data structure.
    /// </summary>

    public struct FilterParams
    {
        public FilterParams(ConwayPoly p, int i)
        {
            poly = p;
            index = i;
        }

        public ConwayPoly poly;
        public int index;

    }
}