using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(AirWalls))]
public class AirWallsEditor : Editor
{
    private float _wallHeight;
    private GUIStyle _guiStyle;
    private GameObject _collideMain;

    [MenuItem("GameObject/Create Other/AirWalls")]
    public static void Create()
    {
        GameObject go = GameObject.Find("AirWalls");
        if (go == null)
        {
            go = new GameObject("AirWalls");
            go.AddComponent<AirWalls>();
            go.isStatic = true;
            go.layer = LayerMask.NameToLayer("AirWall");
            go.transform.position = Vector3.zero;
            Selection.activeGameObject = go;
        }
    }

    private void OnEnable()
    {
        _wallHeight = EditorPrefs.GetFloat("AirWallHeight", 10f);
        if (_guiStyle == null)
        {
            _guiStyle = EditorStyles.miniButton;
            _guiStyle.fontSize = 20;
            _guiStyle.fontStyle = FontStyle.Bold;
        }
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        
        GUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel("Wall Height");
        float height = EditorGUILayout.Slider(_wallHeight, 1f, 20f);
        if (height != _wallHeight)
        {
            EditorPrefs.SetFloat("AirWallHeight", height);
            _wallHeight = height;
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Add", _guiStyle))
        {
            _collideMain = Selection.activeGameObject;
            GameObject airWall = new GameObject("AirWall");
            airWall.AddComponent<AirWall>();
            airWall.transform.SetParent(_collideMain.transform);
            airWall.isStatic = true;
            airWall.layer = LayerMask.NameToLayer("AirWall");
            Selection.activeGameObject = airWall;
        }
        if (GUILayout.Button("Bake", _guiStyle))
        {
            BakeAirWall();
        }
        if (GUILayout.Button("Bingo", _guiStyle))
        {
            BingGoAirWalls();
        }
        GUILayout.EndHorizontal();
    }

    private void BakeAirWall()
    {
        _collideMain = Selection.activeGameObject;
        List<AirWall> airWalls = new List<AirWall>();
        int count = _collideMain.transform.childCount;
        for (int ii = 0; ii < count; ++ii)
        {
            AirWall airWall = _collideMain.transform.GetChild(ii).GetComponent<AirWall>();
            if (airWall != null)
            {
                int childCount = airWall.transform.childCount;
                if (childCount > 0)
                {
                    for (int jj = childCount - 1; jj >= 0; --jj)
                    {
                        DestroyImmediate(airWall.transform.GetChild(jj).gameObject);
                    }
                }
                airWalls.Add(airWall);
            }
        }

        for (int ii = 0; ii < airWalls.Count; ++ii)
        {
            Vector3[] points = airWalls[ii].points;
            for (int jj = 0; jj < points.Length - 1; ++jj)
            {
                GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cube.name = "SubWall" + (jj + 1);
                cube.layer = LayerMask.NameToLayer("AirWall");
                cube.transform.SetParent(airWalls[ii].transform);
                Vector3 relativePos = points[jj] - points[jj + 1];
                relativePos.y = 0;
                Quaternion rotation = Quaternion.LookRotation(relativePos);
                cube.transform.rotation = rotation;
                cube.transform.position = (points[jj] + points[jj + 1]) * 0.5f;
                float distance = Vector2.Distance(new Vector2(points[jj].x, points[jj].z), new Vector2(points[jj + 1].x, points[jj + 1].z));
                cube.transform.localScale = new Vector3(0.1f, _wallHeight * 2f, distance);
                cube.isStatic = true;
            }
        }
    }

    private void BingGoAirWalls()
    {
        _collideMain = Selection.activeGameObject;
        _collideMain.AddComponent<ParticleSystem>();
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        int count = _collideMain.transform.childCount;
        for (int ii = 0; ii < count; ++ii)
        {
            GameObject go = _collideMain.transform.GetChild(ii).gameObject;
            if (go.GetComponent<AirWall>() != null)
            {
                for (int jj = 0; jj < go.transform.childCount; ++jj)
                {
                    Transform child = go.transform.GetChild(jj);
                    if (child.GetComponent<MeshFilter>() == null && child.GetComponent<MeshRenderer>() == null)
                    {
                        MeshFilter filter = child.gameObject.AddComponent<MeshFilter>();
                        filter.sharedMesh = cube.GetComponent<MeshFilter>().sharedMesh;
                        MeshRenderer render = child.gameObject.AddComponent<MeshRenderer>();
                        render.sharedMaterial = _collideMain.GetComponent<Renderer>().sharedMaterial;
                    }
                    else
                    {
                        DestroyImmediate(go.GetComponentInChildren<MeshRenderer>());
                        DestroyImmediate(go.GetComponentInChildren<MeshFilter>());
                    }
                }
            }
        }
        DestroyImmediate(cube);
        DestroyImmediate(_collideMain.GetComponent<ParticleSystem>());
    }
}
