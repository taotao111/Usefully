using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Code.External.Engine.Sqlite
{
    public static class GeometryUtils
    {
        public static GameObject CreateCirclePlaneGameObject()
        {
            GameObject go = new GameObject("CirclePlane");
            Mesh mesh = CreateCirclePlaneMesh(0.5f, 21);
            MeshFilter mf = go.AddComponent<MeshFilter>();
            mf.mesh = mesh;
            MeshRenderer mr = go.AddComponent<MeshRenderer>();
            mr.material = new Material(Shader.Find("Diffuse"));

            return go;
        }
        public static GameObject CreateConeGameObject()
        {
            GameObject go = new GameObject("Cone");
            MeshFilter mf = go.AddComponent<MeshFilter>();
            mf.mesh = CreateConeMesh(0, 0.5f, 1, 60);
            MeshRenderer mr = go.AddComponent<MeshRenderer>();
            mr.material = new Material(Shader.Find("Diffuse"));

            return go;
        }
        //public static GameObject CreateClosedConeGameObject(float openingAngle, float length, int verticesCount)
        //{
        //    float radiusBottom = length * Mathf.Tan(openingAngle * Mathf.Deg2Rad / 2);
        //    return CreateConeClosedGameObject(0, radiusBottom, length, verticesCount);
        //}

        //public static GameObject CreateConeClosedGameObject(int verticesCount = 20)
        //{
        //    return CreateConeClosedGameObject(0, 0.5f, 1, verticesCount);
        //}

        public static Mesh CreateConeMesh(float openingAngle, float length, int verticesCount/*, bool outside, bool inside*/)
        {
            float radiusBottom = length * Mathf.Tan(openingAngle * Mathf.Deg2Rad / 2);

            return CreateConeMesh(0, radiusBottom, length, verticesCount/*, outside, inside*/);
        }
        //public static Mesh CreateConeMesh(float radiusTop, float radiusBottom, float length, int verticesCount, bool outside, bool inside, bool topClosed = false, bool bottomClosed = false)
        //{
        //    Mesh mesh = new Mesh();

        //    if (!outside && !inside)
        //        return mesh;

        //    verticesCount = Mathf.Max(10, verticesCount);

        //    if (topClosed || radiusTop == 0)
        //    {
        //        if (outside && inside)
        //            verticesCount -= 2;
        //        else
        //            verticesCount -= 1;
        //    }

        //    if (bottomClosed || radiusBottom == 0)
        //    {
        //        if (outside && inside)
        //            verticesCount -= 2;
        //        else
        //            verticesCount -= 1;
        //    }


        //    if (outside && inside)
        //    {
        //        verticesCount = verticesCount / 2;
        //    }
        //    else
        //    {
        //        verticesCount = verticesCount / 1;
        //    }



        //    verticesCount /= 2;

        //    // can't access Camera.current
        //    //newCone.transform.position = Camera.current.transform.position + Camera.current.transform.forward * 5.0f;
        //    int multiplier = (outside ? 1 : 0) + (inside ? 1 : 0);
        //    if (topClosed)
        //        multiplier++;

        //    //outsideTop:0,  
        //    //outsideBottom:verticesCount, 
        //    //insideTop: 2 * verticesCount, 
        //    //insideBottom:3 * verticesCount
        //    //closed top: 3 * verticesCount

        //    int offset = (outside && inside ? 2 * verticesCount : 0);
        //    Vector3[] vertices = new Vector3[2 * multiplier * verticesCount]; // 0..n-1: top, n..2n-1: bottom
        //    Vector3[] normals = new Vector3[2 * multiplier * verticesCount];
        //    Vector2[] uvs = new Vector2[2 * multiplier * verticesCount];
        //    int[] tris;
        //    float slope = Mathf.Atan((radiusBottom - radiusTop) / length); // (rad difference)/height
        //    float slopeSin = Mathf.Sin(slope);
        //    float slopeCos = Mathf.Cos(slope);
        //    int i;


        //    for (i = 0; i < verticesCount; i++)
        //    {
        //        float angle = 2 * Mathf.PI * i / verticesCount;
        //        float angleSin = Mathf.Sin(angle);
        //        float angleCos = Mathf.Cos(angle);
        //        float angleHalf = 2 * Mathf.PI * (i + 0.5f) / verticesCount; // for degenerated normals at cone tips
        //        float angleHalfSin = Mathf.Sin(angleHalf);
        //        float angleHalfCos = Mathf.Cos(angleHalf);
        //        vertices[i] = new Vector3(radiusTop * angleCos, length, radiusTop * angleSin);
        //        vertices[i + verticesCount] = new Vector3(radiusBottom * angleCos, 0, radiusBottom * angleSin);


        //        if (radiusTop == 0)
        //            normals[i] = new Vector3(angleHalfCos * slopeCos, slopeSin, angleHalfSin * slopeCos);
        //        else
        //            normals[i] = new Vector3(angleCos * slopeCos, slopeSin, angleSin * slopeCos);
        //        if (radiusBottom == 0)
        //            normals[i + verticesCount] = new Vector3(angleHalfCos * slopeCos, slopeSin, angleHalfSin * slopeCos);
        //        else
        //            normals[i + verticesCount] = new Vector3(angleCos * slopeCos, slopeSin, angleSin * slopeCos);


        //        uvs[i] = new Vector2(1.0f * i / verticesCount, 1);
        //        uvs[i + verticesCount] = new Vector2(1.0f * i / verticesCount, 0);

        //        if (outside && inside)
        //        {
        //            // vertices and uvs are identical on inside and outside, so just copy
        //            vertices[i + 2 * verticesCount] = vertices[i];
        //            vertices[i + 3 * verticesCount] = vertices[i + verticesCount];
        //            uvs[i + 2 * verticesCount] = uvs[i];
        //            uvs[i + 3 * verticesCount] = uvs[i + verticesCount];
        //        }
        //        if (inside)
        //        {
        //            // invert normals
        //            normals[i + offset] = -normals[i];
        //            normals[i + verticesCount + offset] = -normals[i + verticesCount];

        //        }
        //    }



        //    mesh.vertices = vertices;
        //    mesh.normals = normals;
        //    mesh.uv = uvs;
        //    // create triangles
        //    // here we need to take care of point order, depending on inside and outside
        //    int cnt = 0;
        //    if (radiusTop == 0)
        //    {
        //        Debug.Log("top 0");
        //        // top cone
        //        tris = new int[verticesCount * 3 * multiplier];
        //        if (outside)
        //        {
        //            for (i = 0; i < verticesCount; i++)
        //            {
        //                tris[cnt++] = i + verticesCount;
        //                tris[cnt++] = i;
        //                if (i == verticesCount - 1)
        //                    tris[cnt++] = verticesCount;
        //                else
        //                    tris[cnt++] = i + 1 + verticesCount;
        //            }
        //        }
        //        if (inside)
        //        {
        //            //for (i = 0; i < verticesCount; i++)
        //            //{
        //            //    tris[cnt++] = i;
        //            //    tris[cnt++] = i + verticesCount;
        //            //    if (i == verticesCount - 1)
        //            //        tris[cnt++] = verticesCount;
        //            //    else
        //            //        tris[cnt++] = i + 1 + verticesCount;
        //            //}
        //            for (i = offset; i < verticesCount + offset; i++)
        //            {
        //                tris[cnt++] = i;
        //                tris[cnt++] = i + verticesCount;
        //                if (i == verticesCount - 1 + offset)
        //                    tris[cnt++] = verticesCount + offset;
        //                else
        //                    tris[cnt++] = i + 1 + verticesCount;
        //            }
        //        }
        //    }
        //    else if (radiusBottom == 0)
        //    {

        //        // bottom cone
        //        tris = new int[verticesCount * 3 * multiplier];
        //        if (outside)
        //            for (i = 0; i < verticesCount; i++)
        //            {
        //                tris[cnt++] = i;
        //                if (i == verticesCount - 1)
        //                    tris[cnt++] = 0;
        //                else
        //                    tris[cnt++] = i + 1;
        //                tris[cnt++] = i + verticesCount;
        //            }
        //        if (inside)
        //            for (i = offset; i < verticesCount + offset; i++)
        //            {
        //                if (i == verticesCount - 1 + offset)
        //                    tris[cnt++] = offset;
        //                else
        //                    tris[cnt++] = i + 1;
        //                tris[cnt++] = i;
        //                tris[cnt++] = i + verticesCount;
        //            }
        //    }
        //    else
        //    {
        //        // truncated cone
        //        tris = new int[verticesCount * 6 * multiplier];
        //        if (outside)
        //            for (i = 0; i < verticesCount; i++)
        //            {
        //                int ip1 = i + 1;
        //                if (ip1 == verticesCount)
        //                    ip1 = 0;

        //                tris[cnt++] = i;
        //                tris[cnt++] = ip1;
        //                tris[cnt++] = i + verticesCount;

        //                tris[cnt++] = ip1 + verticesCount;
        //                tris[cnt++] = i + verticesCount;
        //                tris[cnt++] = ip1;
        //            }
        //        if (inside)
        //            for (i = offset; i < verticesCount + offset; i++)
        //            {
        //                int ip1 = i + 1;
        //                if (ip1 == verticesCount + offset)
        //                    ip1 = offset;

        //                tris[cnt++] = ip1;
        //                tris[cnt++] = i;
        //                tris[cnt++] = i + verticesCount;

        //                tris[cnt++] = i + verticesCount;
        //                tris[cnt++] = ip1 + verticesCount;
        //                tris[cnt++] = ip1;
        //            }
        //    }
        //    mesh.triangles = tris;

        //    //TangentSolve(mesh);

        //    return mesh;
        //}
        public static void TangentSolve(Mesh mesh)
        {
            int triangleCount = mesh.triangles.Length / 3;
            int vertexCount = mesh.vertices.Length;

            Vector3[] tan1 = new Vector3[vertexCount];
            Vector3[] tan2 = new Vector3[vertexCount];
            Vector4[] tangents = new Vector4[vertexCount];

            for (long a = 0; a < triangleCount; a += 3)
            {
                long i1 = mesh.triangles[a + 0];
                long i2 = mesh.triangles[a + 1];
                long i3 = mesh.triangles[a + 2];

                Vector3 v1 = mesh.vertices[i1];
                Vector3 v2 = mesh.vertices[i2];
                Vector3 v3 = mesh.vertices[i3];

                Vector2 w1 = mesh.uv[i1];
                Vector2 w2 = mesh.uv[i2];
                Vector2 w3 = mesh.uv[i3];

                float x1 = v2.x - v1.x;
                float x2 = v3.x - v1.x;
                float y1 = v2.y - v1.y;
                float y2 = v3.y - v1.y;
                float z1 = v2.z - v1.z;
                float z2 = v3.z - v1.z;

                float s1 = w2.x - w1.x;
                float s2 = w3.x - w1.x;
                float t1 = w2.y - w1.y;
                float t2 = w3.y - w1.y;

                float r = 1.0f / (s1 * t2 - s2 * t1);

                Vector3 sdir = new Vector3((t2 * x1 - t1 * x2) * r, (t2 * y1 - t1 * y2) * r, (t2 * z1 - t1 * z2) * r);
                Vector3 tdir = new Vector3((s1 * x2 - s2 * x1) * r, (s1 * y2 - s2 * y1) * r, (s1 * z2 - s2 * z1) * r);

                tan1[i1] += sdir;
                tan1[i2] += sdir;
                tan1[i3] += sdir;

                tan2[i1] += tdir;
                tan2[i2] += tdir;
                tan2[i3] += tdir;
            }


            for (long a = 0; a < vertexCount; ++a)
            {
                Vector3 n = mesh.normals[a];
                Vector3 t = tan1[a];

                Vector3 tmp = (t - n * Vector3.Dot(n, t)).normalized;
                tangents[a] = new Vector4(tmp.x, tmp.y, tmp.z);

                tangents[a].w = (Vector3.Dot(Vector3.Cross(n, t), tan2[a]) < 0.0f) ? -1.0f : 1.0f;
            }

            mesh.tangents = tangents;
        }
        public static GameObject CreateConeClosedGameObject()
        {

            Transform t = new GameObject("Cone").transform;
            Mesh mesh = CreateConeMesh(0, .5f, 1, 61, true, true);

            MeshFilter mf = t.gameObject.AddComponent<MeshFilter>();
            mf.mesh = mesh;
            MeshRenderer mr = t.gameObject.AddComponent<MeshRenderer>();
            mr.material = new Material(Shader.Find("Diffuse"));

            return t.gameObject;
        }
        public static Mesh CreateConeMesh(float radiusTop, float radiusBottom, float length, int verticesCount,/* bool outside, bool inside,*/ bool topClosed = false, bool bottomClosed = false)
        {
            Mesh mesh = new Mesh();

            //if (!outside && !inside)
            //    return mesh;

            if (radiusTop <= 0 && radiusBottom <= 0)
                return mesh;



            verticesCount = Mathf.Max(10, verticesCount);

            int multiplier = 2;

            int closeCount = 0;

            if (radiusTop > 0 && topClosed)
            {
                multiplier += 1;
                closeCount++;
            }
            if (radiusBottom > 0 && bottomClosed)
            {
                multiplier += 1;
                closeCount++;
            }

            verticesCount = (verticesCount - closeCount) / multiplier;

            Vector3[] vertices = new Vector3[multiplier * verticesCount + closeCount];
            Vector3[] normals = new Vector3[multiplier * verticesCount + closeCount];
            Vector2[] uvs = new Vector2[multiplier * verticesCount + closeCount];
            int[] tris = null;
            float slope = Mathf.Atan((radiusBottom - radiusTop) / length); // (rad difference)/height
            float slopeSin = Mathf.Sin(slope);
            float slopeCos = Mathf.Cos(slope);


            int topOffset = 0;
            int topClosedOffset = -1;
            int topClosedCenterOffset = -1;
            int bottomOffset = verticesCount;
            int bottomClosedOffset = -1;
            int bottomClosedCenterOffset = -1;
            float topUvDiameter = 0;
            float bottomUvDiameter = 0;

            if (radiusTop > 0 && topClosed)
            {
                topClosedOffset = bottomOffset + verticesCount;
                topClosedCenterOffset = multiplier * verticesCount + closeCount - 2;
                topUvDiameter = 1 / (radiusTop * 2);

                vertices[topClosedCenterOffset] = new Vector3(0, length * 0.5f, 0);
                normals[topClosedCenterOffset] = new Vector3(0, 1, 0);
                uvs[topClosedCenterOffset] = new Vector2(0.5f, 0.5f);
            }



            if (radiusBottom > 0 && bottomClosed)
            {
                if (topClosedOffset != -1)
                    bottomClosedOffset = topClosedOffset + verticesCount;
                else
                    bottomClosedOffset = bottomOffset + verticesCount;

                bottomClosedCenterOffset = multiplier * verticesCount + closeCount - 1;

                bottomUvDiameter = 1 / (radiusBottom * 2);

                vertices[bottomClosedCenterOffset] = new Vector3(0, -length * 0.5f, 0);
                normals[bottomClosedCenterOffset] = new Vector3(0, -1, 0);
                uvs[bottomClosedCenterOffset] = new Vector2(0.5f, 0.5f);

            }


            float angleFactor = 2 * Mathf.PI / verticesCount;

            for (int i = 0; i < verticesCount; i++)
            {
                float angle = angleFactor * i;
                float angleSin = Mathf.Sin(angle);
                float angleCos = Mathf.Cos(angle);
                float angleHalf = 2 * Mathf.PI * (i + 0.5f) / verticesCount; // for degenerated normals at cone tips
                float angleHalfSin = Mathf.Sin(angleHalf);
                float angleHalfCos = Mathf.Cos(angleHalf);

                vertices[topOffset + i] = new Vector3(radiusTop * angleCos, length * 0.5f, radiusTop * angleSin);
                vertices[bottomOffset + i] = new Vector3(radiusBottom * angleCos, -length * 0.5f, radiusBottom * angleSin);



                if (radiusTop == 0)
                    normals[topOffset + i] = new Vector3(angleHalfCos * slopeCos, slopeSin, angleHalfSin * slopeCos);
                else
                    normals[topOffset + i] = new Vector3(angleCos * slopeCos, slopeSin, angleSin * slopeCos);

                if (radiusBottom == 0)
                    normals[bottomOffset + i] = new Vector3(angleHalfCos * slopeCos, slopeSin, angleHalfSin * slopeCos);
                else
                    normals[bottomOffset + i] = new Vector3(angleCos * slopeCos, slopeSin, angleSin * slopeCos);


                uvs[topOffset + i] = new Vector2(1.0f * i / verticesCount, 1);
                uvs[bottomOffset + i] = new Vector2(1.0f * i / verticesCount, 0);
                //uvs[i] = new Vector2(0.5f, 0.5f);
                //uvs[bottomOffset + i] = new Vector2(-vertices[bottomOffset + i].x * bottomUvDiameter + 0.5f, -vertices[bottomOffset + i].z * bottomUvDiameter + 0.5f);

                if (topClosedOffset != -1)
                {
                    vertices[topClosedOffset + i] = vertices[topOffset + i];
                    normals[topClosedOffset + i] = new Vector3(0, 1, 0);
                    uvs[topClosedOffset + i] = new Vector2(-vertices[topOffset + i].x * topUvDiameter + 0.5f, -vertices[topOffset + i].z * topUvDiameter + 0.5f);
                }

                if (bottomClosedOffset != -1)
                {
                    vertices[bottomClosedOffset + i] = vertices[bottomOffset + i];
                    normals[bottomClosedOffset + i] = new Vector3(0, -1, 0);
                    uvs[bottomClosedOffset + i] = new Vector2(-vertices[bottomOffset + i].x * bottomUvDiameter + 0.5f, -vertices[bottomOffset + i].z * bottomUvDiameter + 0.5f);

                }



                //if (outside && inside)
                //{
                //    // vertices and uvs are identical on inside and outside, so just copy
                //    vertices[i + 2 * verticesCount] = vertices[i];
                //    vertices[i + 3 * verticesCount] = vertices[i + verticesCount];
                //    uvs[i + 2 * verticesCount] = uvs[i];
                //    uvs[i + 3 * verticesCount] = uvs[i + verticesCount];
                //}
                //if (inside)
                //{
                //    // invert normals
                //    normals[i + offset] = -normals[i];
                //    normals[i + verticesCount + offset] = -normals[i + verticesCount];

                //}
            }

            // create triangles
            // here we need to take care of point order, depending on inside and outside
            int cnt = 0;

            bool outside = true, inside = false;

            if (radiusTop == 0 || radiusBottom == 0)
            {
                tris = new int[(multiplier - 1) * 3 * verticesCount];
            }
            else
            {
                tris = new int[verticesCount * 3 * multiplier];
            }


            if (radiusTop == 0)
            {
                // top cone

                if (outside)
                {
                    for (int i = 0; i < verticesCount; i++)
                    {
                        tris[cnt++] = verticesCount + i;
                        tris[cnt++] = i;
                        if (i == verticesCount - 1)
                            tris[cnt++] = verticesCount;
                        else
                            tris[cnt++] = verticesCount + i + 1;
                    }
                }
                //if (inside)
                //{
                ////for ( int i = 0; i < verticesCount; i++)
                ////{
                ////    tris[cnt++] = i;
                ////    tris[cnt++] = i + verticesCount;
                ////    if (i == verticesCount - 1)
                ////        tris[cnt++] = verticesCount;
                ////    else
                ////        tris[cnt++] = i + 1 + verticesCount;
                ////}
                //for ( int i = offset; i < verticesCount + offset; i++)
                //{
                //    tris[cnt++] = i;
                //    tris[cnt++] = i + verticesCount;
                //    if (i == verticesCount - 1 + offset)
                //        tris[cnt++] = verticesCount + offset;
                //    else
                //        tris[cnt++] = i + 1 + verticesCount;
                //}
                //}




            }
            else if (radiusBottom == 0)
            {

                // bottom cone
                //tris = new int[verticesCount * 3 * multiplier];
                if (outside)
                {
                    for (int i = 0; i < verticesCount; i++)
                    {
                        tris[cnt++] = i;
                        if (i == verticesCount - 1)
                            tris[cnt++] = 0;
                        else
                            tris[cnt++] = i + 1;
                        tris[cnt++] = i + verticesCount;
                    }
                }
                //if (inside)
                //    for ( int i = offset; i < verticesCount + offset; i++)
                //    {
                //        if (i == verticesCount - 1 + offset)
                //            tris[cnt++] = offset;
                //        else
                //            tris[cnt++] = i + 1;
                //        tris[cnt++] = i;
                //        tris[cnt++] = i + verticesCount;
                //    }
            }
            else
            {
                // truncated cone


                if (outside)
                {
                    for (int i = 0; i < verticesCount; i++)
                    {
                        int ip1 = i + 1;
                        if (ip1 == verticesCount)
                            ip1 = 0;

                        tris[cnt++] = i;
                        tris[cnt++] = ip1;
                        tris[cnt++] = i + verticesCount;

                        tris[cnt++] = ip1 + verticesCount;
                        tris[cnt++] = i + verticesCount;
                        tris[cnt++] = ip1;
                    }
                }

                //if (inside)
                //    for ( int i = offset; i < verticesCount + offset; i++)
                //    {
                //        int ip1 = i + 1;
                //        if (ip1 == verticesCount + offset)
                //            ip1 = offset;

                //        tris[cnt++] = ip1;
                //        tris[cnt++] = i;
                //        tris[cnt++] = i + verticesCount;

                //        tris[cnt++] = i + verticesCount;
                //        tris[cnt++] = ip1 + verticesCount;
                //        tris[cnt++] = ip1;
                //    }
            }

            if (topClosedOffset != -1)
            {
                for (int i = 0; i < verticesCount; i++)
                {
                    tris[cnt++] = topClosedOffset + i;
                    tris[cnt++] = topClosedCenterOffset;
                    tris[cnt++] = topClosedOffset + ((i == verticesCount - 1) ? 0 : (i + 1));
                }
            }

            if (bottomClosedOffset != -1)
            {
                for (int i = 0; i < verticesCount; i++)
                {
                    tris[cnt++] = bottomClosedCenterOffset;
                    tris[cnt++] = bottomClosedOffset + i;
                    tris[cnt++] = bottomClosedOffset + ((i == verticesCount - 1) ? 0 : (i + 1));
                }
            }



            mesh.vertices = vertices;
            mesh.normals = normals;
            mesh.uv = uvs;
            mesh.triangles = tris;


            return mesh;
        }
        public static Mesh CreateCirclePlaneMesh(float radius, int verticesCount)
        {
            Mesh mesh = new Mesh();

            verticesCount = Mathf.Max(3, verticesCount);
            verticesCount = verticesCount - 1;

            Vector3[] vertices = new Vector3[verticesCount + 1];
            Vector3[] normals = new Vector3[verticesCount + 1];
            Vector2[] uvs = new Vector2[verticesCount + 1];
            int[] tris = new int[verticesCount * 3];

            float uvDiameter = 1 / (radius * 2f);
            int cnt = 0;

            vertices[verticesCount] = Vector3.zero;
            normals[verticesCount] = Vector3.up;
            uvs[verticesCount] = new Vector2(0.5f, 0.5f);

            float angleFactor = 2 * Mathf.PI / verticesCount;
            for (int i = 0; i < verticesCount; i++)
            {
                float angle = angleFactor * i;
                float angleSin = Mathf.Sin(angle);
                float angleCos = Mathf.Cos(angle);


                vertices[i] = new Vector3(radius * angleCos, 0, radius * angleSin);
                normals[i] = Vector3.up;
                uvs[i] = new Vector2(-vertices[i].x * uvDiameter + 0.5f, -vertices[i].z * uvDiameter + 0.5f);


                tris[cnt++] = i;
                tris[cnt++] = verticesCount;
                tris[cnt++] = (i == verticesCount - 1) ? 0 : (i + 1);
            }

            mesh.vertices = vertices;
            mesh.normals = normals;
            mesh.uv = uvs;
            mesh.triangles = tris;

            return mesh;
        }
        //public static Mesh CreateCirclePlaneMesh3(float radius, int verticesCount)
        //{
        //    Mesh mesh = new Mesh();

        //    verticesCount = Mathf.Max(3, verticesCount);
        //    verticesCount = verticesCount / 3;

        //    Vector3[] vertices = new Vector3[verticesCount * 3];
        //    Vector3[] normals = new Vector3[verticesCount * 3];
        //    Vector2[] uvs = new Vector2[verticesCount * 3];
        //    int[] tris = new int[verticesCount * 3];

        //    float uvDiameter = 1 / (radius * 2f);
        //    int cnt = 0;
        //    int centerOffset = verticesCount * 2;

        //    for (int i = 0; i < verticesCount; i++)
        //    {
        //        float angle = 2 * Mathf.PI * i / verticesCount;
        //        float angleSin = Mathf.Sin(angle);
        //        float angleCos = Mathf.Cos(angle);
        //        float angleHalf = 2 * Mathf.PI * (i + 0.5f) / verticesCount; // for degenerated normals at cone tips
        //        float angleHalfSin = Mathf.Sin(angleHalf);
        //        float angleHalfCos = Mathf.Cos(angleHalf);

        //        vertices[i] = new Vector3(radius * angleCos, 0, radius * angleSin);
        //        normals[i] = Vector3.up;
        //        uvs[i] = new Vector2(-vertices[i].x * uvDiameter + 0.5f, -vertices[i].z * uvDiameter + 0.5f);

        //        vertices[verticesCount + i] = vertices[i];
        //        normals[verticesCount + i] = normals[i];
        //        uvs[verticesCount + i] = uvs[i];

        //        vertices[centerOffset + i] = Vector3.zero;
        //        normals[centerOffset + i] = Vector3.up;
        //        uvs[centerOffset + i] = new Vector2(0.5f, 0.5f);


        //        tris[cnt++] = i;
        //        tris[cnt++] = centerOffset + i;
        //        tris[cnt++] = (i == verticesCount - 1) ? verticesCount : (i + verticesCount + 1);
        //    }

        //    mesh.vertices = vertices;
        //    mesh.normals = normals;
        //    mesh.uv = uvs;
        //    mesh.triangles = tris;

        //    return mesh;
        //}


        //public static Mesh CreateCirclePlaneMesh2(float radius, int verticesCount)
        //{
        //    Mesh mesh = new Mesh();

        //    verticesCount = Mathf.Max(10, verticesCount);

        //    verticesCount = verticesCount / 2;

        //    Vector3[] vertices = new Vector3[verticesCount * 2];
        //    Vector3[] normals = new Vector3[verticesCount * 2];
        //    Vector2[] uvs = new Vector2[verticesCount * 2];
        //    int[] tris = new int[verticesCount * 3];
        //    float diameter = radius * 2f;
        //    int cnt = 0;

        //    for (int i = 0; i < verticesCount; i++)
        //    {
        //        float angle = 2 * Mathf.PI * i / verticesCount;
        //        float angleSin = Mathf.Sin(angle);
        //        float angleCos = Mathf.Cos(angle);
        //        float angleHalf = 2 * Mathf.PI * (i + 0.5f) / verticesCount; // for degenerated normals at cone tips
        //        float angleHalfSin = Mathf.Sin(angleHalf);
        //        float angleHalfCos = Mathf.Cos(angleHalf);

        //        vertices[i] = new Vector3(radius * angleCos, 0, radius * angleSin);
        //        normals[i] = Vector3.up;
        //        //uvs[i] = new Vector2(1.0F * i / verticesCount, 0);
        //        uvs[i] = vertices[i] / diameter;

        //        vertices[verticesCount + i] = Vector3.zero;
        //        normals[verticesCount + i] = Vector3.up;
        //        //  uvs[i + verticesCount] = new Vector2(1.0f * i / verticesCount, 1);
        //        uvs[verticesCount + i] = new Vector2(0.5f, 0.5f);


        //        tris[cnt++] = i;
        //        if (i == verticesCount - 1)
        //        {
        //            //  tris[cnt++] = verticesCount;
        //            tris[cnt++] = verticesCount + 1;
        //            tris[cnt++] = 0;
        //        }
        //        else
        //        {
        //            //  tris[cnt++] =  verticesCount;
        //            tris[cnt++] = i + verticesCount;
        //            tris[cnt++] = i + 1;
        //        }
        //    }

        //    mesh.vertices = vertices;
        //    mesh.normals = normals;
        //    mesh.uv = uvs;
        //    mesh.triangles = tris;

        //    return mesh;
        //}
        public static GameObject CreateCubeGameObject()
        {
            GameObject go = new GameObject("Cube");
            MeshFilter mf = go.AddComponent<MeshFilter>();
            mf.mesh = CreateCubeMesh(Vector3.one);
            MeshRenderer mr = go.AddComponent<MeshRenderer>();
            mr.material = new Material(Shader.Find("Diffuse"));

            return go;
        }
        public static Mesh CreateCubeMesh(Vector3 size)
        {
            Mesh mesh = new Mesh();

            Vector3 halfSize = size * 0.5f;

            Vector3[] vertices;
            Vector3[] normals = new Vector3[24];
            Vector2[] uvs = new Vector2[24];
            int[] tris;

            vertices = new Vector3[]{           
                //forward
                new Vector3(halfSize.x,-halfSize.y,halfSize.z),
                new Vector3(halfSize.x,halfSize.y,halfSize.z),
                new Vector3(-halfSize.x,halfSize.y,halfSize.z),
                new Vector3(-halfSize.x,-halfSize.y,halfSize.z),                  

                //back 
                new Vector3(halfSize.x, halfSize.y, -halfSize.z),
                new Vector3(halfSize.x, -halfSize.y, -halfSize.z), 
                new Vector3( -halfSize.x,-halfSize.y,-halfSize.z),
                new Vector3(-halfSize.x, halfSize.y, -halfSize.z),
               
                //top         
                new Vector3(halfSize.x,halfSize.y,halfSize.z),   
                new Vector3(halfSize.x, halfSize.y, -halfSize.z),
                new Vector3(-halfSize.x, halfSize.y, -halfSize.z),
                new Vector3(-halfSize.x,halfSize.y,halfSize.z),

                //bottom             
                new Vector3(halfSize.x, -halfSize.y, -halfSize.z), 
                new Vector3(halfSize.x,-halfSize.y,halfSize.z),
                new Vector3(-halfSize.x,-halfSize.y,halfSize.z),
                new Vector3( -halfSize.x,-halfSize.y,-halfSize.z),
                  
                //right        
                new Vector3(halfSize.x, -halfSize.y, -halfSize.z), 
                new Vector3(halfSize.x, halfSize.y, -halfSize.z),
                new Vector3(halfSize.x,halfSize.y,halfSize.z),
                new Vector3(halfSize.x,-halfSize.y,halfSize.z),

                //left
                new Vector3(-halfSize.x,-halfSize.y,halfSize.z),
                new Vector3(-halfSize.x,halfSize.y,halfSize.z),
                new Vector3(-halfSize.x, halfSize.y, -halfSize.z),
                new Vector3( -halfSize.x,-halfSize.y,-halfSize.z),
                    
            };


            tris = new int[]{                 
                0,1,2,
                2,3,0, 
                4,5,6,
                6,7,4,
                8,9,10,
                10,11,8,
                12,13,14,
                14,15,12,
                16,17,18,
                18,19,16,           
                20,21,22,
                22,23,20,
            };

            Vector3 normal;
            for (int i = 0; i < vertices.Length; i += 4)
            {


                if (i < 4)
                    normal = new Vector3(0, 0, 1);
                else if (i < 8)
                    normal = new Vector3(0, 0, -1);
                else if (i < 12)
                    normal = new Vector3(0, 1, 0);
                else if (i < 16)
                    normal = new Vector3(0, -1, 0);
                else if (i < 20)
                    normal = new Vector3(1, 0, 0);
                else
                    normal = new Vector3(-1, 0, 0);

                normals[i] = normal;
                normals[i + 1] = normal;
                normals[i + 2] = normal;
                normals[i + 3] = normal;

                uvs[i] = new Vector2(0, 0);
                uvs[i + 1] = new Vector2(0, 1);
                uvs[i + 2] = new Vector2(1, 1);
                uvs[i + 3] = new Vector2(1, 0);

            }


            mesh.vertices = vertices;
            mesh.normals = normals;
            mesh.uv = uvs;
            mesh.triangles = tris;

            return mesh;
        }
        #region  Line
        public static int FindPointToLineClosestIndex(Vector2 p0, Vector2 p1, Vector2[] points, int offset, int length)
        {
            float a = p0.y - p1.y;
            float b = p1.x - p0.x;
            float c = p0.x * p1.y - p1.x * p0.y;
            int index = -1;
            float min = a * p0.x + b * p0.y + c;

            if (min < 0) min = -min;

            float dist;
            for (int i = offset, end = offset + length; i < end; i++)
            {
                dist = a * points[i].x + b * points[i].y + c;

                if (dist < 0) dist = -dist;

                if (dist < min)
                {

                    index = i;
                    min = dist;
                }
            }

            return index;
        }
        public static Vector2 PointToLineCrossPoint(Vector2 p0, Vector2 p1, Vector2 p)
        {
            Vector2 dir = p1 - p0;
            Vector2 w = p - p0;

            float b = Vector2.Dot(w, dir) / Vector2.Dot(dir, dir);

            return p0 + b * dir;

        }
        public static Vector3 PointToLineCrossPoint(Vector3 p0, Vector3 p1, Vector3 p)
        {
            Vector3 dir = p1 - p0;
            Vector3 w = p - p0;

            float b = Vector3.Dot(w, dir) / Vector3.Dot(dir, dir);

            return p0 + b * dir;

        }
        public static Vector2 PointToSegmentCrossPoint(Vector2 p0, Vector2 p1, Vector2 p)
        {
            Vector2 v = p1 - p0;
            Vector2 w = p - p0;

            float c1 = Vector2.Dot(w, v);
            if (c1 <= 0)
                return p0;
            float c2 = Vector2.Dot(v, v);

            if (c2 <= c1)
                return p1;

            return p0 + (c1 / c2) * v;
        }
        public static bool PointToSegmentCrossPoint(Vector2 p0, Vector2 p1, Vector2 p, out Vector2 crossPoint)
        {
            Vector2 v = p1 - p0;
            Vector2 w = p - p0;

            float c1 = Vector2.Dot(w, v);
            if (c1 <= 0)
            {
                crossPoint = Vector2.zero;
                return false;
            }
            float c2 = Vector2.Dot(v, v);

            if (c2 <= c1)
            {
                crossPoint = Vector2.zero;
                return false;
            }
            crossPoint = p0 + c1 / c2 * v;
            return true;
        }
        public static float PointToLineDistance(Vector2 p0, Vector2 p1, Vector2 p)
        {
            return Vector2.Distance(p, PointToLineCrossPoint(p0, p1, p));
        }
        public static float PointToSegmentDistance(Vector2 p0, Vector2 p1, Vector2 p)
        {
            return Vector2.Distance(p, PointToSegmentCrossPoint(p0, p1, p));
        }
        public static bool PointToSegmentDistance(Vector2 p0, Vector2 p1, Vector2 p, out float distance)
        {
            Vector2 _p;
            if (PointToSegmentCrossPoint(p0, p1, p, out _p))
            {
                distance = Vector2.Distance(p, _p);
                return true;
            }
            distance = 0;
            return false;
        }

        #endregion

    }
}