using UnityEditor;
using UnityEngine;

public class HswlfBaseEditor<TTarget> : Editor where TTarget : MonoBehaviour
{
    protected TTarget _target;

    protected void OnEnable()
    {
        _target = (TTarget)target;
    }

    protected T[] FindAll<T>() where T : Object
    {
        var values = new T[0];
        if (_target)
            values = _target.GetComponentsInChildren<T>(includeInactive: true);

        return values;
    }

    protected T SaveAsset<T>(string filePath) where T : ScriptableObject
    {
        if (!_target || string.IsNullOrEmpty(filePath))
            return null;

        T newAsset = CreateInstance<T>();

        AssetDatabase.CreateAsset(newAsset, filePath);

        return newAsset;
    }

    protected static bool BuildFoldout(string title, bool display)
    {
        const float height = 35f;
        var style = new GUIStyle("ShurikenModuleTitle");
        style.font = new GUIStyle(EditorStyles.label).font;
        style.fontSize = 14;
        style.fontStyle = FontStyle.Bold;
        style.border = new RectOffset(15, 7, 4, 4);
        style.fixedHeight = height;
        style.contentOffset = new Vector2(20f, -2f);

        var rect = GUILayoutUtility.GetRect(16f, height, style);
        GUI.Box(rect, title, style);

        var e = Event.current;

        var toggleRect = new Rect(rect.x + 4f, rect.y + (height / 4f), 13f, 13f);
        if (e.type == EventType.Repaint)
        {
            EditorStyles.foldout.Draw(toggleRect, false, false, display, false);
        }

        if (e.type == EventType.MouseDown && rect.Contains(e.mousePosition))
        {
            display = !display;
            e.Use();
        }

        return display;
    }

    protected static bool BuildButton(string label)
    {
        return GUILayout.Button(label, GUILayout.Height(35f));
    }
}
