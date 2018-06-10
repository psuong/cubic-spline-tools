﻿using UnityEngine;
using System.Collections.Generic;

namespace Curves {

    [CreateAssetMenu(menuName = "Curves/Bezier", fileName = "Bezier Curve")]
    public class Bezier : ScriptableObject {

#pragma warning disable 414
        [HideInInspector]
        public Vector3[] points = { Vector3.zero, Vector3.forward * 10f };
        [HideInInspector]
        public Vector3[] controlPoints = { new Vector3(-2.5f, 0f, 2.5f), new Vector3(2.5f, 0f, 7.5f) };
#pragma warning restore 414
        
        /// <summary>
        /// Calculates a cubic bezier curve given a factor. The factor will determine the parametric value,
        /// t.
        /// </summary>
        /// <param name="segments">How many line segments should be generated in the spline?</param>
        /// <returns>An array of points.<returns>
        public Vector3[] GetCubicBezierPoints(int segments) {
            var points = new List<Vector3>();
            var size = this.points.Length;

            for (int i = 1; i < size; i++) {
                var start = this.points[i - 1];
                var end = this.points[i];

                var controlStart = controlPoints[i == 1 ? 0 : i];
                var controlEnd = controlPoints[i == 1 ? i : i + (i - 1)];

                for (int t = 0; t <= segments; t++) {
                    var progress = ((float)t) / ((float)segments);
                    var point = Bezier.GetCubicBezierCurve(start, controlStart, controlEnd, end, progress);
                    points.Add(point);
                }
            }

            return points.ToArray();
        }

        /// <summary>
        /// Gets a point along the tangent of the cubic bezier curve.
        /// </summary>
        /// <param name="p0">Start point</param>
        /// <param name="p1">First control point</param>
        /// <param name="p2">Second control point</param>
        /// <param name="p3">End point</param>
        /// <param name="t">The parametric value, t.</param>
        /// <returns>A point along the cubic bezier curve</returns>
        public static Vector3 GetCubicBezierCurve(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t) {
            t = Mathf.Clamp01(t);
            var inverseT = 1f - t;

            return (Mathf.Pow(inverseT, 3) * p0) + (3 * Mathf.Pow(inverseT, 2) * t * p1) + (3 * inverseT * Mathf.Pow(t, 2) * p2) + (Mathf.Pow(t, 3) * p3); 
        }

        /// <summary>
        /// Gets a point along the tangent of the quadratic bezier curve.
        /// </summary>
        /// <param name="p0">Start point</param>
        /// <param name="p1">Control point</param>
        /// <param name="p2">End point</param>
        /// <returns>A point along the quadratic bezier curve</returns>
        public static Vector3 GetQuadraticBezierCurve(Vector3 p0, Vector3 p1, Vector3 p2, float t) {
            t = Mathf.Clamp01(t);
            var inverseT = 1f - t;

            return inverseT * inverseT * p0 + 2f * inverseT * t * p1 + t * t * p2;
        }

        public static Vector3[] GetVelocities(int lineStep, Vector3[] points, Vector3[] cPoints, Vector3 origin) {
            var velocities = new Vector3[(lineStep * points.Length) - 1];
            
            var t = 1f / lineStep;
            var index = 0;

            for (int i = 1; i < points.Length; i++) {
                var start = points[i - 1];
                var end = points[i];

                var cStart = cPoints[i == 1 ? 0 : i];
                var cEnd = cPoints[i == 1 ? i : i + (i - 1)];
                
                for (float j = 0f; j < 1f; j += t) {
                    var velocity = GetVelocity(start, cStart, cEnd, end, j);
                    // Subtract the velociy from the origin to get the direction of the velocity
                    velocities[index] = velocity - origin;
                    index++;
                }
            }
            return velocities;
        }

        /// <summary>
        /// Gets the tangent (velocity) at a point.
        /// </summary>
        /// <param name="p0">The first point on the bezier curve</param>
        /// <param name="p1">The first control point on the bezier curve</param>
        /// <param name="p2">The second control point on the bezier curve</param>
        /// <param name="p3">The second point on the bezier curve</param>
        /// <param name="t">Parametric value, t, which defines the percentage on the curve</param>
        /// <returns>The first derivative at a said point.</returns>
        public static Vector3 GetVelocity(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t) {
            t = Mathf.Clamp01(t);
            var inverseT = 1f - t;
            
            return (3f * Mathf.Pow(inverseT, 2)  * p1 - p0) + (6f * inverseT * t * (p2 - p1)) + (3 * Mathf.Pow(t, 20) * (p3 - p2));
        }
    }
}
