using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PolyPieceGenerator
{
    public uint numberOfSides = 4;
    public float mainRadius = 1.0f;
    public bool generateCollider = false;
    public bool colliderIsTrigger = false;

    public bool debugRender = true;
    public Material mainMaterial;
    public Material outlineMaterial;
    public float outlineWidthPercentage = 0.05f;

    // Use this for initialization
    public void Generate(GameObject gameObject)
    {
        Vector2[] collider_verts = new Vector2[numberOfSides];

        float rotationOffsetDegrees = 360.0f / numberOfSides / 2;
        float actualRadius = mainRadius / Mathf.Cos(Mathf.Deg2Rad * rotationOffsetDegrees);

        for (uint i = 0; i < numberOfSides; ++i)
        {
            float rad_ang = Mathf.Deg2Rad * (360.0f / numberOfSides * i + rotationOffsetDegrees);
            Vector3 dir = new Vector3(Mathf.Cos(rad_ang), Mathf.Sin(rad_ang));
            collider_verts[i] = actualRadius * dir;
        }

        if (generateCollider)
        {
            PolygonCollider2D col = gameObject.AddComponent<PolygonCollider2D>();
            col.points = collider_verts;
            col.isTrigger = this.colliderIsTrigger;
        }

        if (debugRender)
        {
            gameObject.AddComponent<MeshRenderer>();
            gameObject.AddComponent<MeshFilter>();

            // fill
            GameObject fill = new GameObject
            {
                name = "Fill"
            };
            fill.transform.localScale = gameObject.transform.localScale;
            fill.transform.SetPositionAndRotation(gameObject.transform.position + new Vector3(0, 0, -0.1f),
                                                  gameObject.transform.rotation);
            fill.transform.SetParent(gameObject.transform, true);
            fill.AddComponent<MeshRenderer>();
            fill.AddComponent<MeshFilter>();

            gameObject.GetComponent<MeshRenderer>().material = outlineMaterial;
            fill.GetComponent<MeshRenderer>().material = mainMaterial;

            // gen mesh
            Vector3[] vertices = new Vector3[numberOfSides + 1];
            Vector3[] outline_fill_vertices = new Vector3[numberOfSides + 1];
            vertices[numberOfSides] = new Vector3();
            outline_fill_vertices[numberOfSides] = new Vector3();

            for (uint i = 0; i < numberOfSides; ++i)
            {
                vertices[i] = new Vector3(collider_verts[i].x, collider_verts[i].y);
                outline_fill_vertices[i] = (1 - outlineWidthPercentage) * vertices[i];
            }

            int[] triVerts = new int[3 * numberOfSides];
            for (int i = 0; i < numberOfSides; ++i)
            {
                triVerts[3 * i] = (int)numberOfSides;
                triVerts[3 * i + 1] = (i + 1) % (int)numberOfSides;
                triVerts[3 * i + 2] = i;
            }

            Mesh mesh = new Mesh
            {
                vertices = vertices,
                triangles = triVerts
            };
            gameObject.GetComponent<MeshFilter>().mesh = mesh;


            Mesh fill_mesh = new Mesh
            {
                vertices = outline_fill_vertices,
                triangles = triVerts
            };
            fill.GetComponent<MeshFilter>().mesh = fill_mesh;
        }
    }

    public static float CalculateActualRadius(int numberOfSides, float mainRadius)
    {
        float rotationOffsetRadians = Mathf.Deg2Rad * 360.0f / numberOfSides / 2;
        return mainRadius / Mathf.Cos(rotationOffsetRadians);
    }

    public static float CalculateVStackDistance(int numberOfSides, float mainRadius)
    {
        float rotationOffsetRadians = Mathf.Deg2Rad * 360.0f / numberOfSides / 2;
        float actualRadius = mainRadius / Mathf.Cos(rotationOffsetRadians);
        return actualRadius * (2.0f - Mathf.Sin(rotationOffsetRadians));
    }
}
