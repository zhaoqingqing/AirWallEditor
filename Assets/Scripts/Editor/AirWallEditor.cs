using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Reflection;

[CustomEditor(typeof(AirWall))]
public class AirWallEditor : Editor
{
    private enum State
    {
        Main, Add, Edit
    }

    private State _uiState;
    private AirWall _airWall;
    [SerializeField]
    public List<Vector3> _tempPoints;
    private int _currentPoint = -1;
    private int _sceneMask = 0;
    private float _minDisntance;
    private Ray ray;
    private RaycastHit hit;

    /// <summary>
    /// 当前AirWall的Points
    /// </summary>
    [SerializeField]
    public Vector3[] points
    {
        get
        {
            if (_airWall != null) return _airWall.points;
            return null;
        }
    }

    //用于在属性面板上显示airwalls
    protected SerializedObject _serializedObject;
    protected SerializedProperty _szPoints;

    private void OnEnable()
    {
        Tools.hidden = true;

        _minDisntance = EditorPrefs.GetFloat("AirWallMinDistance", 1f);

        if (_sceneMask == 0)
            _sceneMask = 1 << LayerMask.NameToLayer("Scene");

        _airWall = Selection.activeGameObject.GetComponent<AirWall>();
        if (_airWall.points != null)
        {
            _tempPoints = new List<Vector3>(_airWall.points);
            if (_airWall.points.Length == 0)
                _airWall.points = null;
        }
        else
        {
            _tempPoints = new List<Vector3>();
        }

        _serializedObject = new SerializedObject(this);
        _szPoints = _serializedObject.FindProperty("_tempPoints");
    }

    private void OnDisable()
    {
        Tools.hidden = false;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        //EditorGUILayout.LabelField("airwall count:" + AirwallCount);
        //显示序列化属性

        _serializedObject.Update();
        EditorGUI.BeginChangeCheck();
        if (_szPoints == null)
        {
            _szPoints = _serializedObject.FindProperty("_tempPoints");
        }
        if (_szPoints != null)
        {
            EditorGUILayout.LabelField("Points Count:" + _tempPoints.Count);
            //属性没有HideInInspector就不必重绘了
            //EditorGUILayout.PropertyField(_szPoints, true);
        }
        if (EditorGUI.EndChangeCheck())
        {
            _serializedObject.ApplyModifiedProperties();
        }

        GUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel("Min Distance");
        float distance = EditorGUILayout.Slider(_minDisntance, AirWallDef.AirWallMinDistance, AirWallDef.AirWallMaxDistance);
        if (distance != _minDisntance)
        {
            EditorPrefs.SetFloat("AirWallMinDistance", distance);
            _minDisntance = distance;
        }
        GUILayout.EndHorizontal();

        //direction axis
        GUILayout.BeginHorizontal();
        var newDir = (AirWallDir)EditorGUILayout.EnumPopup(new GUIContent("direction axis:"), _airWall.DirType);
        if (newDir != _airWall.DirType)
        {
            _airWall.DirType = newDir;
        }
        GUILayout.EndHorizontal();
    }


    private void OnSceneGUI()
    {
        HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
        Handles.BeginGUI();
        GUI.Box(new Rect(Screen.width - 160, Screen.height - 250, 150, 200), "AirWall Tool Bar");
        switch (_uiState)
        {
            case State.Main:
                StateMain();
                break;
            case State.Add:
                StateAdd();
                break;
            case State.Edit:
                StateEdit();
                break;
        }
        Handles.EndGUI();
    }

    void StateMain()
    {
        if (_airWall.points == null)
        {
            if (GUI.Button(new Rect(Screen.width - 135, Screen.height - 230, 100, 50), "Add Points"))
            {
                _uiState = State.Add;
                _tempPoints = new List<Vector3>();
            }
        }
        else
        {
            if (GUI.Button(new Rect(Screen.width - 135, Screen.height - 170, 100, 50), "Edit Points"))
            {
                _uiState = State.Edit;
                _tempPoints = new List<Vector3>(_airWall.points);
                _currentPoint = -1;
            }
        }
        if (GUI.Button(new Rect(Screen.width - 135, Screen.height - 110, 100, 50), "Delete Points"))
        {
            _airWall.points = null;
            _currentPoint = -1;
            HandleUtility.Repaint();
        }

        if (_airWall.points != null)
        {
            for (int idx = 0; idx < _airWall.points.Length; ++idx)
            {
                Handles.color = _currentPoint == idx ? Color.blue : Color.white;
                int hashCode = GetHashCode();
                Handles.Label(_airWall.points[idx], "  T" + (idx + 1) + "_P" + (idx + 1));
                //给每个节点画个圆
                float size = HandleUtility.GetHandleSize(_airWall.points[idx]);
                int controllIDBeforeHandle = GUIUtility.GetControlID(hashCode, FocusType.Passive);
                Handles.ScaleValueHandle(0, _airWall.points[idx], Quaternion.identity, size, Handles.SphereCap, 0);

                int controllIDAfterHandle = GUIUtility.GetControlID(hashCode, FocusType.Passive);
                if (controllIDBeforeHandle < GUIUtility.hotControl && GUIUtility.hotControl < controllIDAfterHandle)
                    _currentPoint = idx;
                Handles.DrawPolyLine(_airWall.points);
            }
        }
    }

    void StateAdd()
    {
        if (_tempPoints.Count > 1 && GUI.Button(new Rect(Screen.width - 135, Screen.height - 230, 100, 50), "Save"))
        {
            _airWall.points = _tempPoints.ToArray();
            _uiState = State.Main;
            _currentPoint = -1;
        }
        if (_tempPoints.Count > 0 && GUI.Button(new Rect(Screen.width - 135, Screen.height - 170, 100, 50), "Delete Last"))
        {
            _tempPoints.RemoveAt(_tempPoints.Count - 1);
        }
        if (GUI.Button(new Rect(Screen.width - 135, Screen.height - 110, 100, 50), "Cancel"))
        {
            _uiState = State.Main;
            _currentPoint = -1;
            HandleUtility.Repaint();
        }

        if (_tempPoints.Count > 0)
        {
            Handles.color = Color.green;
            for (int idx = 0; idx < _tempPoints.Count; ++idx)
            {
                Handles.Label((Vector3)_tempPoints[idx], "  P" + (idx + 1));
                float size = HandleUtility.GetHandleSize(_tempPoints[idx]);
                int controllIDBeforeHandle = GUIUtility.GetControlID(FocusType.Passive);
                Handles.ScaleValueHandle(0, _tempPoints[idx], Quaternion.identity, size, Handles.SphereCap, 0);
                if (_currentPoint == idx)
                    _tempPoints[idx] = Handles.PositionHandle(_tempPoints[idx], Quaternion.identity);
                int controllIDAfterHandle = GUIUtility.GetControlID(FocusType.Passive);
                if (controllIDBeforeHandle < GUIUtility.hotControl && GUIUtility.hotControl < controllIDAfterHandle)
                    _currentPoint = idx;
            }
            Handles.DrawPolyLine(_tempPoints.ToArray());
        }

        ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);

        bool flag = Physics.Raycast(ray, out hit, Mathf.Infinity, _sceneMask);
        if (flag)
        {
            if (!(Event.current.mousePosition.x > Screen.width - 160 && Event.current.mousePosition.y > Screen.height - 250))
            {
                if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
                {
                    if (_tempPoints.Count > 2 && Vector3.Distance(hit.point, _tempPoints[0]) < _minDisntance)
                        _tempPoints.Add(_tempPoints[0]);
                    else
                        _tempPoints.Add(hit.point);
                    _currentPoint = -1;
                }
            }
            Handles.Label(hit.point, "\n   P" + (_tempPoints.Count + 1));
            float size = HandleUtility.GetHandleSize(hit.point) * 0.2f;
            Handles.CircleCap(0, hit.point, Quaternion.LookRotation(hit.normal), size);
            HandleUtility.Repaint();

            Color oldColor = Handles.color;
            Color color = Color.red;
            color.a = 0.1f;
            Handles.color = color;
            Handles.DrawSolidDisc(hit.point, hit.normal, _minDisntance);
            Handles.color = oldColor;
        }
    }

    void StateEdit()
    {
        if (_currentPoint != -1 && GUI.Button(new Rect(Screen.width - 135, Screen.height - 230, 100, 50), "Insert Point"))
        {
            if (_currentPoint == _tempPoints.Count - 1)
            {
                Vector3 insert = (2 * _tempPoints[_currentPoint] - _tempPoints[_currentPoint - 1]);
                _tempPoints.Add(insert);
            }
            else
            {
                Vector3 insert = Vector3.zero;
                if (_currentPoint >= 1)
                {
                    insert = (_tempPoints[_currentPoint] + _tempPoints[_currentPoint - 1]) * 0.5f;
                }
                else if (_currentPoint >= 0)
                {
                    insert = _tempPoints[_currentPoint] * 0.5f;
                }
                _tempPoints.Insert(_currentPoint + 1, insert);
            }
        }
        if (_currentPoint != -1 && _tempPoints.Count > 2 && (GUI.Button(new Rect(Screen.width - 135, Screen.height - 170, 100, 50), "Delete Point")))
        {
            _tempPoints.RemoveAt(_currentPoint);
            _currentPoint = -1;
            HandleUtility.Repaint();
        }
        if (GUI.Button(new Rect(Screen.width - 135, Screen.height - 110, 100, 50), "Save"))
        {
            _airWall.points = _tempPoints.ToArray();
            _uiState = State.Main;
            _currentPoint = -1;
        }

        if (_tempPoints.Count > 0)
        {
            Handles.color = Color.blue;
            for (int idx = 0; idx < _tempPoints.Count; ++idx)
            {
                Handles.Label(_tempPoints[idx], "  P" + (idx + 1));
                float size = HandleUtility.GetHandleSize(_tempPoints[idx]) * 1.5f;
                int controllIDBeforeHandle = GUIUtility.GetControlID(FocusType.Passive);
                Handles.ScaleValueHandle(0, _tempPoints[idx], Quaternion.identity, size, Handles.SphereCap, 0);
                if (_currentPoint == idx)
                    _tempPoints[idx] = Handles.PositionHandle(_tempPoints[idx], Quaternion.identity);
                int controllIDAfterHandle = GUIUtility.GetControlID(FocusType.Passive);
                if (controllIDBeforeHandle < GUIUtility.hotControl && GUIUtility.hotControl < controllIDAfterHandle)
                    _currentPoint = idx;
            }
            Handles.DrawPolyLine(_tempPoints.ToArray());
        }

        if (Event.current.keyCode == KeyCode.Escape)
            _currentPoint = -1;

        if (_currentPoint == -1)
        {
            ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, _sceneMask))
            {
                if (!(Event.current.mousePosition.x > Screen.width - 160 && Event.current.mousePosition.y > Screen.height - 250))
                {
                    if (Event.current.type == EventType.MouseDrag && Event.current.button == 0)
                    {
                        if (_currentPoint != -1 && _currentPoint < _tempPoints.Count)
                        {
                            _tempPoints[_currentPoint] = hit.point;
                        }
                    }
                }
                HandleUtility.Repaint();
            }
        }
    }
}
