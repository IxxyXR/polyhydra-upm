using System.Collections.Generic;
using System.Linq;

namespace Conway {
    /// <summary>
    /// 
    /// </summary>
    public class MeshVertexList : List<Vertex> {
        
        private ConwayPoly _mConwayPoly;

        /// <summary>
        /// Creates a vertex list that is aware of its parent mesh
        /// </summary>
        /// <param name="conwayPoly"></param>
        public MeshVertexList(ConwayPoly conwayPoly) : base() {
            _mConwayPoly = conwayPoly;
        }

        /// <summary>
        /// Convenience constructor, for use outside of the mesh class
        /// </summary>
        public MeshVertexList() : base() {
            _mConwayPoly = null;
        }

    }
}