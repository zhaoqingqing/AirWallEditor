using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(AirWalls))]
public class AirWallsEditor : Editor
{
    private float _wallHeight;
    private float _wallThick;
    private GUIStyle _guiStyle;
    private GameObject _collideMain;
    /// <summary>
    /// 当前场景所有的AirWall Component
    /// </summary>
    [SerializeField]
    public List<AirWall> airWalls = new List<AirWall>();
    //用于在属性面板上显示airwalls
    protected SerializedObject _serializedObject;
    protected SerializedProperty _szAirwalls;

    private int airwallCount = 0;

    private int AirwallCount
    {
        get
        {
            if (airWalls != null)
            {
                if (airWalls.Count > 0)
                {
                    airwallCount = airWalls.Count;
                }
                else
                {
                    var activeGameObject = Selection.activeGameObject;
                    if (activeGameObject != null)
                    {
                        int count = activeGameObject.transform.childCount;
                        for (int ii = 0; ii < count; ++ii)
                        {
                            var airwall = activeGameObject.transform.GetChild(ii).GetComponent<AirWall>();
                            if (airwall != null)
                            {
                                airWalls.Add(airwall);
                            }
                        }
                    }
                }
            }
            return airwallCount;
        }
    }

    //for horizontal
    //public bool isHorizontal = false;

    //check if need add scene layer
    public static void InitLayer()
    {

    }

    //check Scene has gameobject in "scene Layer"
    public static bool CheckSceneLayer()
    {
        bool result = false;

        var objs = GameObject.FindObjectsOfType<GameObject>();
        foreach (var gameObject in objs)
        {
            if (gameObject == null) continue;
            if (gameObject.layer == LayerMask.NameToLayer(AirWallDef.SceneLayerName))
            {
                var collider = gameObject.GetComponent(typeof(Collider));
                if (collider == null)
                {
                    Debug.LogErrorFormat("{0},not contains collider!", gameObject.name);
                    result = false;
                }
                else
                {
                    result = true;
                }
                break;
            }
        }
        return result;
    }

    //add menu in hierarchy 
    [MenuItem("GameObject/Create Other/AirWalls", false, 55000)]
    public static void CreateNew()
    {
        Create();
    }

    [MenuItem("GameObject/Create Other/AirWalls")]
    public static void Create()
    {
        if (CheckSceneLayer() == false)
        {
            EditorUtility.DisplayDialog("系统提示", "当前场景中没有Gameobject在Scene这个Layer(层)或没有Collider", "确定");
            return;
        }
        GameObject go = GameObject.Find("AirWalls");
        if (go == null)
        {
            go = new GameObject("AirWalls");
            go.AddComponent<AirWalls>();
            go.isStatic = true;
            go.layer = LayerMask.NameToLayer(AirWallDef.AirWallLayerName);
            go.transform.position = Vector3.zero;
            Selection.activeGameObject = go;
        }
    }

    private void OnEnable()
    {
        _wallHeight = EditorPrefs.GetFloat("AirWallHeight", AirWallDef.AirWallHeight);
        _wallThick = EditorPrefs.GetFloat(AirWallDef.StrAirWallThick, AirWallDef.AirWallThick);
        if (_guiStyle == null)
        {
            _guiStyle = EditorStyles.miniButton;
            _guiStyle.fontSize = 20;
            _guiStyle.fontStyle = FontStyle.Bold;
        }
        _serializedObject = new SerializedObject(this);
        _szAirwalls = _serializedObject.FindProperty("airWalls");
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        EditorGUILayout.LabelField("airwall count:" + AirwallCount);

        //显示序列化属性
        _serializedObject.Update();
        EditorGUI.BeginChangeCheck();
        if (_szAirwalls != null)
        {
            EditorGUILayout.PropertyField(_szAirwalls, true);
        }
        if (EditorGUI.EndChangeCheck())
        {
            _serializedObject.ApplyModifiedProperties();
        }


        GUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel("Wall Height");
        float height = EditorGUILayout.Slider(_wallHeight, AirWallDef.AirWallMinHeight, AirWallDef.AirWallMaxHeight);
        if (height != _wallHeight)
        {
            EditorPrefs.SetFloat("AirWallHeight", height);
            _wallHeight = height;
        }
        GUILayout.EndHorizontal();

        //thickness
        GUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel("Wall Thick");
        float thick = EditorGUILayout.Slider(_wallThick, AirWallDef.AirWallMinThick, AirWallDef.AirWallMaxThick);
        if (thick != _wallThick)
        {
            EditorPrefs.SetFloat(AirWallDef.StrAirWallThick, thick);
            _wallThick = thick;
        }
        GUILayout.EndHorizontal();

        //buttons
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Add", _guiStyle))
        {
            AddAirWall();
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

        //TODO Horizontal space
        //GUILayout.Space(20);
        //GUILayout.Label("Horizontal Wall");
        //GUILayout.BeginHorizontal();
        //if (GUILayout.Button("Add", _guiStyle))
        //{
        //    _collideMain = Selection.activeGameObject;
        //    GameObject airWall = new GameObject("AirWall");
        //    airWall.AddComponent<AirWall>();
        //    airWall.transform.SetParent(_collideMain.transform);
        //    airWall.isStatic = true;
        //    airWall.layer = LayerMask.NameToLayer("AirWall");
        //    Selection.activeGameObject = airWall;
        //}
        //if (GUILayout.Button("Bake", _guiStyle))
        //{
        //    BakeAirWall();
        //}
        //if (GUILayout.Button("Bingo", _guiStyle))
        //{
        //    BingGoAirWalls();
        //}
        //GUILayout.EndHorizontal();
    }

    private void AddAirWall()
    {
        _collideMain = Selection.activeGameObject;
        GameObject airWall = new GameObject("AirWall");
        airWall.AddComponent<AirWall>();
        airWall.transform.SetParent(_collideMain.transform);
        airWall.isStatic = true;
        airWall.layer = LayerMask.NameToLayer("AirWall");
        //fix in unity5.3.5 has error:ptr->GetHideFlags () == m_RequiredHideFlags
        airWall.hideFlags = HideFlags.None;
        Selection.activeGameObject = airWall;
    }

    private void BakeAirWall()
    {
        _collideMain = Selection.activeGameObject;
        if (_collideMain == null || airWalls == null)
        {
            return;
        }
        //如果Airwalls下还有其它的非AirWall，把它们整理到一个节点下
        List<Transform> otherList = new List<Transform>();
        var otherTrans = _collideMain.transform.FindChild("_Others");
        if (otherTrans == null)
        {
            otherTrans = new GameObject("_Others").transform;
            otherTrans.SetParent(_collideMain.transform);
            otherTrans.localPosition = Vector3.zero;
            otherTrans.localRotation = Quaternion.identity;
            otherTrans.localScale = Vector3.one;
        }

        if (airWalls.Count > 0)
        {
            airWalls.Clear();
        }

        //destory old child
        int count = _collideMain.transform.childCount;
        for (int ii = 0; ii < count; ++ii)
        {
            var child = _collideMain.transform.GetChild(ii);
            AirWall airWall = child.GetComponent<AirWall>();
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
            else { otherList.Add(child); }
        }

        for (int ii = 0; ii < otherList.Count; ++ii)
        {
            otherList[ii].SetParent(otherTrans);
        }
        //bug 设置 HideFlags之后，还是会报“ptr->GetHideFlags () == m_RequiredHideFlags”
        GameObject cubePrefab = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cubePrefab.hideFlags = HideFlags.None;
        //create new child
        for (int ii = 0; ii < airWalls.Count; ++ii)
        {
            //bug points 会丢失
            Vector3[] points = airWalls[ii].points;
            if (points == null || points.Length <= 0)
            {
                Debug.LogWarningFormat("airWalls[{0}] point null", ii);
                continue;
            }
            for (int jj = 0; jj < points.Length - 1; ++jj)
            {
                AirWall airWall = airWalls[ii].transform.GetComponent<AirWall>();
                GameObject cube = Instantiate(cubePrefab);
                cube.name = "SubWall" + (jj + 1);
                cube.layer = LayerMask.NameToLayer("AirWall");
                cube.transform.SetParent(airWalls[ii].transform);
                Vector3 relativePos = points[jj] - points[jj + 1];
                relativePos.y = 0;
                Quaternion rotation = Quaternion.LookRotation(relativePos);
                cube.transform.rotation = rotation;
                if (airWall != null)
                {
                    //position = point  - cube thick *0.5f
                    var pos = (points[jj] + points[jj + 1]) * 0.5f;
                    Vector3 newPos = pos;
                    switch (airWall.DirType)
                    {
                        case AirWallDir.Top:
                            newPos = new Vector3(pos.x + _wallThick * 0.5f, pos.y, pos.z);
                            break;
                        case AirWallDir.Bottom:
                            newPos = new Vector3(pos.x - _wallThick * 0.5f, pos.y, pos.z);
                            break;
                        case AirWallDir.Left:
                            newPos = new Vector3(pos.x, pos.y, pos.z + _wallThick * 0.5f);
                            break;
                        case AirWallDir.Right:
                            newPos = new Vector3(pos.x, pos.y, pos.z - _wallThick * 0.5f);
                            break;
                    }
                    cube.transform.position = newPos;
                }
                else
                {
                    Debug.LogError("not contains AirWall Component");
                }
                //axis z = lenth 
                float distance = Vector2.Distance(new Vector2(points[jj].x, points[jj].z), new Vector2(points[jj + 1].x, points[jj + 1].z));
                cube.transform.localScale = new Vector3(_wallThick, _wallHeight * 2f, distance);
                cube.isStatic = true;
            }
        }
        DestroyImmediate(cubePrefab);
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
