public enum EdgeShape
{
    J, // Edges can be a path of any shape
    U, // Edges must look the same after reflecting across their length
    S, // Edges must look the same after a 180° rotation
    I  // Edges must look the same after both rotation and reflection
};