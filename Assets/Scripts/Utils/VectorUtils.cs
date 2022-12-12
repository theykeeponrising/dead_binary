using UnityEngine;
using UnityEngine.UI;

public static class VectorUtils
{
    public static bool RayPlaneIntersection(out Vector3 intersectionPoint, Vector3 rayPoint, Vector3 rayDir, 
        Vector3 planePoint, Vector3 planeNormal)
    {
        intersectionPoint = new Vector3(0.0f, 0.0f, 0.0f);
        float a = Vector3.Dot((planePoint - rayPoint), planeNormal);
        float b = Vector3.Dot(rayDir, planeNormal);

        if (b != 0)
        {
            float x = a / b;
            intersectionPoint = rayDir * x + rayPoint;
            // If x is less than 0, intersection is in the opposite direction of the ray
            return x >= 0;
        }

        // Line parallel to plane, so no intersection
        return false;
    }
}