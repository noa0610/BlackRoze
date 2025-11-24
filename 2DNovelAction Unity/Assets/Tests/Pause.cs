using BlackRose.Core.Models.Units;
using UnityEditor;
using UnityEngine;

public class Pause : EditorUtility
{
    [MenuItem("Tools/BlackRose/Pause %&p")]
    public static void PauseGame()
    {
        var p = UnitBase._isPlaying;
        UnitManager.instance.Pause(p);
    }
}