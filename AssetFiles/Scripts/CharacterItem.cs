using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
#endif

[CreateAssetMenu(fileName = "New Character", menuName = "Node Dialogue Sytem/Character")]
public class CharacterItem : ScriptableObject
{
    public string Name;
    public bool CustomColor = false;
    public Color Color = new Color(0.3f, 0.3f, 0.3f, 1);
    public List<BodyParts<List<BodyImg>>> BodyParts = new List<BodyParts<List<BodyImg>>>();

    [System.Serializable]
    public class BodyImg
    {
        public Sprite ImgSprite = null;
        public GameObject ImgPref = null;
    }

    public void UpdateParts()
    {
        BodyParts = BodyParts<List<BodyImg>>.Sorting(BodyParts, null);
    }
    public string[] GetCharacterImages(int BodyID)
    {
        string[] names = new string[0];

        if (BodyParts.Count > BodyID)
        {
            names = new string[BodyParts[BodyID].Content.Count];

            for (int i = 0; i < names.Length; i++)
            {
                if (BodyParts[BodyID].BodyName != "")
                    names[i] =BodyParts[BodyID].BodyName;
                else
                    names[i] = $"Part_{i + 1}";
            }
        }

        return names;
    }
}




#if UNITY_EDITOR
[CustomEditor(typeof(CharacterItem))]
[CanEditMultipleObjects]
public class CharacterItemEditor : Editor
{
    public CharacterItem t;

    public SerializedProperty Name;
    public SerializedProperty Color;
    public SerializedProperty CustomColor;
    public SerializedProperty BodyParts;
    private void OnEnable()
    {
        t = (CharacterItem)target;
        t.UpdateParts();

        //Find Propertise
        Name = serializedObject.FindProperty("Name");
        Color = serializedObject.FindProperty("Color");
        CustomColor = serializedObject.FindProperty("CustomColor");
        BodyParts = serializedObject.FindProperty("BodyParts");

        DialogSettings.Instance.UpdateCharters();

        foreach (var item in t.BodyParts)
            if (item.Content == null)
                item.Content = new List<CharacterItem.BodyImg>();
    }

    private void OnDisable()
    {
        EditorUtility.SetDirty(target);
        AssetDatabase.SaveAssets();
    }
    public override void OnInspectorGUI()
    {
        //DrawDefaultInspector();
        serializedObject.Update();

        GUILayout.BeginHorizontal();
            if (t.BodyParts.Count > 0  && t.BodyParts[0].Content.Count > 0 && t.BodyParts[0].Content[0].ImgSprite != null)
            {
                CustomGUI.DrawBoxLayout(t.BodyParts[0].Content[0].ImgSprite, 35);
            }
            else
                GUILayout.Box("Null", GUILayout.Width(35), GUILayout.Height(35));
        GUILayout.Space(5);
        GUILayout.BeginVertical();
        //GUILayout.Space(10);
        EditorGUILayout.PropertyField(Name, new GUIContent("Character Name:"));
        GUILayout.BeginHorizontal();
        EditorGUILayout.PropertyField(Color, new GUIContent("Character Color:"), GUILayout.Width(EditorGUIUtility.currentViewWidth-90));
        CustomColor.boolValue = EditorGUILayout.Toggle(CustomColor.boolValue);
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
        GUILayout.EndHorizontal();

        string CharterBodyParts = "□ Charter Body Parts";
        if (BodyParts.isExpanded)
            CharterBodyParts = "■ Charter Body Parts";

        if (GUILayout.Button(CharterBodyParts, GUILayout.Height(35)))
        {
            if (BodyParts.isExpanded)
                BodyParts.isExpanded = false;
            else
                BodyParts.isExpanded = true;
        }
        //Если открыт список частей тела
        if (BodyParts.isExpanded)
        {
            //EditorGUI.indentLevel+=2;
            for (int i = 0; i < t.BodyParts.Count; i++)
            {
                SerializedProperty ItemProp = BodyParts.GetArrayElementAtIndex(i);
                GUILayout.BeginHorizontal();
                GUILayout.Space(5);


                    if (t.BodyParts[i].Content.Count > 0 && t.BodyParts[i].Content[0].ImgSprite != null)
                    {
                            CustomGUI.DrawBoxLayout(t.BodyParts[i].Content[0].ImgSprite, 35);
                    }
                    else
                        GUILayout.Box("Null", GUILayout.Width(35), GUILayout.Height(35));

                string BodyName = $"□ {t.BodyParts[i].BodyName}";
                if (ItemProp.isExpanded)
                    BodyName = $"■ {t.BodyParts[i].BodyName}";

                if (GUILayout.Button(BodyName, GUILayout.Height(35)))
                {
                    if (ItemProp.isExpanded)
                        ItemProp.isExpanded = false;
                    else
                        ItemProp.isExpanded = true;
                };

                GUILayout.EndHorizontal();

                if (ItemProp.isExpanded)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(54);
                    GUILayout.BeginVertical();


                    ///////////////////////////Отобразить все элементы выбранной части
                    GUILayout.Space(5);
                    CustomGUI.GuiLine(1);
                    GUILayout.Space(5);
                    SerializedProperty content = ItemProp.FindPropertyRelative("Content");
                    int ContentLength = t.BodyParts[i].Content.Count;
                    for (int a = 0; a < ContentLength; a++)
                    {
                        SerializedProperty CurContent = content.GetArrayElementAtIndex(a);
                        SerializedProperty PartName = ItemProp.FindPropertyRelative("BodyName");
                        SerializedProperty ImgSprite = CurContent.FindPropertyRelative("ImgSprite");
                        SerializedProperty ImgPref = CurContent.FindPropertyRelative("ImgPref");

                        GUILayout.BeginHorizontal();

                        ImgSprite.objectReferenceValue = (Sprite)EditorGUILayout.ObjectField(ImgSprite.objectReferenceValue, typeof(Sprite), false, GUILayout.Width(135), GUILayout.Height(100));
                        
                        //DrawImage(t.BodyParts[i].Images[a].ImgSprite, t.BodyParts[i].Images[a].ImgPrefIcon, 100);
                        GUILayout.Space(5);
                        GUILayout.BeginVertical();
                        if (t.BodyParts[i].BodyName != "")
                            GUILayout.Label($"{PartName.stringValue}_Part", EditorStyles.boldLabel);
                        else
                            GUILayout.Label($"Part_{a + 1}", EditorStyles.boldLabel);
                        GUILayout.Space(10);

                        PartName.stringValue = EditorGUILayout.TextField("Name of this part:", PartName.stringValue);

                        if (DialogSettings.Instance.ImgType == DialogSettings.imgType.Prefab)
                            ImgPref.objectReferenceValue = (GameObject)EditorGUILayout.ObjectField("Prefab:", ImgPref.objectReferenceValue, typeof(GameObject), false);

                        GUILayout.EndVertical();
                        GUILayout.EndHorizontal();
                        GUILayout.Space(5);
                        CustomGUI.GuiLine(1);
                        GUILayout.Space(5);
                    }

                    if (GUILayout.Button("Add New Part"))
                    {
                        content.InsertArrayElementAtIndex(content.arraySize);
                    }

                    if (ContentLength > 0)
                    {
                        if (GUILayout.Button("Remove Last Part"))
                        {
                            content.DeleteArrayElementAtIndex(content.arraySize - 1);
                        }
                    }

                    GUILayout.EndVertical();
                    GUILayout.EndHorizontal();
                }
            }
            //EditorGUI.indentLevel-=2;
        }

        if (serializedObject.hasModifiedProperties)
        {
            if(NodeWindowEditor.instance)
                NodeWindowEditor.instance.Repaint();
            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif
