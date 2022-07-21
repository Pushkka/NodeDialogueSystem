using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
#endif

public class DialogSettings : ScriptableObject
{
    public static DialogSettings Instance;
    public enum imgType { Sprite, Prefab }
    public imgType ImgType;
    //public enum posType { LeftRight, NativeCenter }
    //public posType PosType;

    //public string[] Localization;
    public List<string> PartNames = new List<string>();
    public List<CharacterItem> Charters = new List<CharacterItem>();

    public Vector2 NodeSize = new Vector2(200, 100);

    public Color BackgroundColor = new Color(0.17f, 0.17f, 0.17f, 1);
    public Color SelectionColor = Color.cyan * 0.4f;
    public Color StartNodeColor = new Color(0.77f, 0.45f, 0.08f, 1);
    public Color DefaultNodeColor = new Color(0.31f, 0.31f, 0.31f, 1);
    public Color SelectionNodeColor = Color.blue * 0.5f;
    public Color ActionConectionColor = Color.white;
    public Color TimeConectionColor = new Color(0.12f, 0.63f, 0.12f, 1);
    public Color AnswerConectionColor = new Color(0.25f, 0.43f, 0.69f, 1);
    public Color SelectionConectionColor = Color.cyan;

    [MenuItem("Window/Node Dialogue Sytem/Settings")]
    public static void OpenSettings()
    {
        if (!Instance)
            SetInstance();

        Selection.activeObject = Instance;
    }
    public static void SetInstance()
    {
        if (!Instance)
        {
            Object[] Assets = AssetDatabase.LoadAllAssetsAtPath(@"Assets\NodeDialogueSystem\SettingsFile.asset");
            if(Assets.Length > 0)
            { 
                Instance = (DialogSettings)Assets[0];
            }
            else
            {
                DialogSettings asset = ScriptableObject.CreateInstance<DialogSettings>();
                asset.name = $"SettingsFile";
                asset.hideFlags = HideFlags.HideInHierarchy;
                AssetDatabase.CreateAsset(asset, $@"Assets\NodeDialogueSystem\{asset.name}.asset");
                EditorUtility.SetDirty(asset);

                Instance = asset;
                SaveSettings();
            }
        }
    }

    public static void SaveSettings()
    {
        AssetDatabase.SaveAssets();
    }
    public void UpdateCharters()
    {
        for (int i = 0; i < Charters.Count; i++)
        {
            if (Charters[i] == null)
                Charters.RemoveAt(i);
        }

        string[] guids2 = AssetDatabase.FindAssets("t:CharacterItem");
        List<CharacterItem> FindedChars = new List<CharacterItem>();
        foreach (var item in guids2)
        {
            CharacterItem FindedChar = (CharacterItem)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(item), typeof(CharacterItem));
           
            FindedChars.Add(FindedChar);
            if (!Charters.Contains(FindedChar))
            {
                Charters.Add(FindedChar);
            }
        }
    }  
    public string[] GetCharacters()
    {
        UpdateCharters();

        string[] names = new string[Charters.Count + 1];
        names[0] = "Talking to Yourself";
        for (int i = 0; i < names.Length - 1; i++)
        {
            names[i + 1] = Charters[i].Name;
        }

        return names;
    }

    public int FindCharacter(CharacterItem Char)
    {
        return Charters.IndexOf(Char);
    }
    public string[] GetCharactersBodyParts()
    {
        return PartNames.ToArray();
    }
}
[CustomEditor(typeof(DialogSettings))]
public class DialogSettingsEditor : Editor
{
    public DialogSettings t;

    public SerializedProperty ImgType;
    public SerializedProperty PosType;
    public SerializedProperty PartNames;

    public SerializedProperty NodeSize;

    public SerializedProperty BackgroundColor;
    public SerializedProperty SelectionColor;
    public SerializedProperty StartNodeColor;
    public SerializedProperty DefaultNodeColor;
    public SerializedProperty SelectionNodeColor;
    public SerializedProperty ActionConectionColor;
    public SerializedProperty TimeConectionColor;
    public SerializedProperty AnswerConectionColor;
    public SerializedProperty SelectionConectionColor;

    private void OnEnable()
    {
        t = (DialogSettings)target;

        ImgType = serializedObject.FindProperty("ImgType");
        PosType = serializedObject.FindProperty("PosType");
        PartNames = serializedObject.FindProperty("PartNames");

        NodeSize = serializedObject.FindProperty("NodeSize");

        BackgroundColor = serializedObject.FindProperty("BackgroundColor");
        SelectionColor = serializedObject.FindProperty("SelectionColor");
        StartNodeColor = serializedObject.FindProperty("StartNodeColor");
        DefaultNodeColor = serializedObject.FindProperty("DefaultNodeColor");
        SelectionNodeColor = serializedObject.FindProperty("SelectionNodeColor");
        ActionConectionColor = serializedObject.FindProperty("ActionConectionColor");
        TimeConectionColor = serializedObject.FindProperty("TimeConectionColor");
        AnswerConectionColor = serializedObject.FindProperty("AnswerConectionColor");
        SelectionConectionColor = serializedObject.FindProperty("SelectionConectionColor");
    }
    private void OnDisable()
    {
        DialogSettings.SaveSettings();
    }
    public override void OnInspectorGUI()
    {
        //DrawDefaultInspector();
        serializedObject.Update();

        ImgType.enumValueIndex = (int)(DialogSettings.imgType)EditorGUILayout.EnumPopup("Image Type:", t.ImgType);

        GUILayout.Space(10);
        CustomGUI.GuiLine(2);
        GUILayout.Space(10);

        //PosType.enumValueIndex = (int)(DialogSettings.posType)EditorGUILayout.EnumPopup("Position Type:", t.PosType);

        //GUILayout.Space(10);
        //CustomGUI.GuiLine(2);
        //GUILayout.Space(10);

        GUILayout.Label("Character Body Parts:");

        EditorGUI.indentLevel += 2;
        for (int i = 0; i < PartNames.arraySize; i++)
        {
            SerializedProperty PartName = PartNames.GetArrayElementAtIndex(i);
            PartName.stringValue = EditorGUILayout.TextField($"Part_{i + 1}", PartName.stringValue);
        }
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Add New"))
        {
            PartNames.InsertArrayElementAtIndex(PartNames.arraySize);
        }
        if (t.PartNames.Count > 0)
            if (GUILayout.Button("Remove Last"))
                PartNames.DeleteArrayElementAtIndex(PartNames.arraySize - 1);
        GUILayout.EndHorizontal();
        EditorGUI.indentLevel -= 2;

        GUILayout.Space(10);
        CustomGUI.GuiLine(2);
        GUILayout.Space(10);

        NodeSize.vector2Value= EditorGUILayout.Vector2Field("Node size:", NodeSize.vector2Value);

        GUILayout.Space(10);
        CustomGUI.GuiLine(2);
        GUILayout.Space(10);

        GUILayout.BeginHorizontal();
        GUILayout.Label("Colors:");
        if (GUILayout.Button("Reset"))
        {
            BackgroundColor.colorValue = new Color(0.17f, 0.17f, 0.17f, 1);
            SelectionColor.colorValue = Color.cyan * 0.4f;
            StartNodeColor.colorValue = new Color(0.77f, 0.45f, 0.08f, 1);
            DefaultNodeColor.colorValue = new Color(0.31f, 0.31f, 0.31f, 1);
            SelectionNodeColor.colorValue = Color.blue * 0.5f;
            ActionConectionColor.colorValue = Color.white;
            TimeConectionColor.colorValue = new Color(0.12f, 0.63f, 0.12f, 1);
            AnswerConectionColor.colorValue = new Color(0.25f, 0.43f, 0.69f, 1);
            SelectionConectionColor.colorValue = Color.cyan;
        }
        GUILayout.EndHorizontal();


        GUILayout.BeginHorizontal();
        GUILayout.Label("Background:", GUILayout.Width(125));
        BackgroundColor.colorValue = EditorGUILayout.ColorField(BackgroundColor.colorValue);
        GUILayout.EndHorizontal();
        
        GUILayout.BeginHorizontal();
        GUILayout.Label("Selection:", GUILayout.Width(125));
        SelectionColor.colorValue = EditorGUILayout.ColorField(SelectionColor.colorValue);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Start Node:", GUILayout.Width(125));
        StartNodeColor.colorValue = EditorGUILayout.ColorField(StartNodeColor.colorValue);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Default Node:", GUILayout.Width(125));
        DefaultNodeColor.colorValue = EditorGUILayout.ColorField(DefaultNodeColor.colorValue);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Selection Node:", GUILayout.Width(125));
        SelectionNodeColor.colorValue = EditorGUILayout.ColorField(SelectionNodeColor.colorValue);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Action Conection:", GUILayout.Width(125));
        ActionConectionColor.colorValue = EditorGUILayout.ColorField(ActionConectionColor.colorValue);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Time Conection:", GUILayout.Width(125));
        TimeConectionColor.colorValue = EditorGUILayout.ColorField(TimeConectionColor.colorValue);
        GUILayout.EndHorizontal();
        
        GUILayout.BeginHorizontal();
        GUILayout.Label("Answer Conection:", GUILayout.Width(125));
        AnswerConectionColor.colorValue = EditorGUILayout.ColorField(AnswerConectionColor.colorValue);
        GUILayout.EndHorizontal();
        
        GUILayout.BeginHorizontal();
        GUILayout.Label("Selection Conection:", GUILayout.Width(125));
        SelectionConectionColor.colorValue = EditorGUILayout.ColorField(SelectionConectionColor.colorValue);
        GUILayout.EndHorizontal();

        

        if (serializedObject.hasModifiedProperties)
        {
            if (NodeWindowEditor.instance)
                NodeWindowEditor.instance.Repaint();
            serializedObject.ApplyModifiedProperties();
        }
    }
}
