using System;
using UnityEngine;

[Serializable]
public class AirWall : MonoBehaviour 
{
    //让这个值显示出来，方便调试
    //[HideInInspector]
    [SerializeField]
    public Vector3[] points;

    [HideInInspector]
    [SerializeField]
    public AirWallDir DirType = AirWallDir.None;

    [HideInInspector]
    [SerializeField]
    public float WallThick = 0.1f;
}
