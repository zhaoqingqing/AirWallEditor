using UnityEngine;
using System.Collections;

/// <summary>
/// airwall const values
/// </summary>
public class AirWallDef
{
    //layer
    public const string SceneLayerName = "Scene";
    public const string AirWallLayerName = "AirWall";

    public const string StrAirWallThick= "AirWallThick";

    /// <summary>
    /// default height
    /// </summary>
    public static float AirWallHeight = 6.0f;
    public static float AirWallMinHeight = 1.0f;
    public static float AirWallMaxHeight = 20.0f;

    public static float AirWallMinDistance = 0.1f;
    public static float AirWallMaxDistance = 10.0f;

    public static float AirWallThick = 0.1f;
    public static float AirWallMinThick = 0.01f;
    public static float AirWallMaxThick = 4.0f;
}

public enum AirWallDir
{
    None,
    Top,
    Bottom,
    Left,
    Right
}
