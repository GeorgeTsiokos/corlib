
namespace CorLib.Linq {

    /// <summary>
    /// Identify in which set(s) the object lives
    /// </summary>
    /// <remarks>
    /// Descriptions from http://en.wikipedia.org/wiki/Set_(mathematics)
    /// </remarks>
    public enum ObjectSetLocation : byte {
        /// <summary>
        /// The relative complement of B in A, denoted by A − B (or A \ B),
        /// is the set of all elements which are members of A but not 
        /// members of B.
        /// </summary>
        AButNotB = 10,
        /// <summary>
        /// The intersection of A and B, denoted by A ∩ B, is the set 
        /// of all things which are members of both A and B. If A ∩ B = ∅, 
        /// then A and B are said to be disjoint.
        /// </summary>
        Intersection = 11,
        /// <summary>
        /// The relative complement of A in B, denoted by B − A (or B \ A),
        /// is the set of all elements which are members of B but not 
        /// members of A.
        /// </summary>
        BButNotA = 12
    }
}