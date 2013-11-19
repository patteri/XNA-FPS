using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace FirstPersonCameraWindows.Models
{
    /// <summary>
    /// Helper struct containing data related to "edge case" calculations.
    /// </summary>
    public struct EdgeCase
    {
        /// <summary>
        /// The distance to the edge.
        /// </summary>
        public float distance;

        /// <summary>
        /// The correction vector.
        /// </summary>
        public Vector3 correction;

        /// <summary>
        /// Initializes a new instance of the <see cref="EdgeCase"/> struct.
        /// </summary>
        /// <param name="distance">The distance to the edge.</param>
        /// <param name="correction">The correction vector.</param>
        public EdgeCase(float distance, Vector3 correction)
        {
            this.distance = distance;
            this.correction = correction;
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return String.Format("X: {0}, Y: {1}, Z: {2}, D: {3}", correction.X, correction.Y, correction.Z, distance);
        }
    }
}
