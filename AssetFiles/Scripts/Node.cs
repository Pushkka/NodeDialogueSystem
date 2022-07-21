using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
#endif

[System.Serializable]
public class Node : ScriptableObject
{
    private Vector2 _midPoint;
    [SerializeField]
    private Rect rect = new Rect(100, 100, 200, 100);


    public CharacterItem Character;
    public CharacterPos Position;
    public enum CharacterPos
    {
        Left,
        Right
    }
    
    // Contains Names And Selected Parts ID
    public List<BodyParts<int>> BodyParts = new List<BodyParts<int>>();

    public string text;
    public float textSpeed = 1f;

    public List<Connection> Connections = new List<Connection>();

    public List<ConectionParameters> OutputParam = new List<ConectionParameters>();




    public void SetPos(Vector2 pos)
    {
        rect.x = pos.x;
        rect.y = pos.y;
        _midPoint = SetMidPoint();
    }
    public void Drag(Vector2 delta)
    {
        rect.position += delta;
        _midPoint = SetMidPoint();
    }


    public bool Сonsists(Rect Area)
    {
        if (Area.Contains(_midPoint, true))
            return true;

        return false;
    }

    public bool Contains(Vector2 Position)
    {
        return rect.Contains(Position);
    }
    public void Draw(bool StartNode)
    {
        Color NodeColor = DialogSettings.Instance.DefaultNodeColor;
        rect.size = DialogSettings.Instance.NodeSize;

        //Если выбрана нода
        for (int i = 0; i < Selection.objects.Length; i++)
        {
            if (Selection.objects[i] == this)
            {
                CustomGUI.DrawBorder(rect, DialogSettings.Instance.SelectionNodeColor, 3);
                break;
            }
        }

        //Если это стартовая надо
        if (StartNode)
        {
            NodeColor = DialogSettings.Instance.StartNodeColor;
            GUI.backgroundColor = DialogSettings.Instance.StartNodeColor;
        }
        //Если это обычная нода
        else
        {
            GUI.backgroundColor = Color.gray;

            //Если не выбран персонаж
            if (Character && Character.CustomColor)
                NodeColor = Character.Color;
        }

        EditorGUI.DrawRect(rect, NodeColor);

        int Padding = 5;

        GUILayout.BeginArea(new Rect(rect.x + Padding, rect.y + Padding, rect.width - Padding * 2, rect.height - Padding * 2));

        GUI.skin.box.normal.textColor = Color.white * 0.9f;
        GUI.skin.label.normal.textColor = Color.white * 0.9f;

        Color temp = GUI.backgroundColor;
        GUIStyle LAlign = EditorStyles.label; 
        GUIStyle BoxRAlign = new GUIStyle(GUI.skin.box);
        LAlign.alignment = TextAnchor.LowerRight;
        BoxRAlign.alignment = TextAnchor.UpperLeft;
        //BoxRAlign.normal.textColor = Color.white * 0.9f;

        if (!Character)
        {
            GUILayout.Label("Talking to Yourself");
            GUILayout.Box(text, BoxRAlign, GUILayout.Width(rect.width - Padding * 2), GUILayout.Height(rect.height - Padding - 25));
        }
        else
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(Character.Name); 
            int MissingParts = BodyParts.FindAll(x => x.Content == -1).Count;
            if (MissingParts > 0)
            {
                GUILayout.Label($"{MissingParts} MISSING", LAlign);
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (Character && Character.BodyParts.Count > 0 && BodyParts.Count > 0 && Character.BodyParts[0].Content.Count > 0 && BodyParts[0].Content != -1)
                CustomGUI.DrawBoxLayout(Character.BodyParts[0].Content[BodyParts[0].Content].ImgSprite, 70);
            else
            {
                if (Character)
                    GUI.backgroundColor = Color.red;
                CustomGUI.DrawBoxLayout("Null", 70);
            }
            GUI.backgroundColor = temp;

            GUILayout.Box(text, BoxRAlign, GUILayout.Width(rect.width - Padding - 80), GUILayout.Height(rect.height - Padding - 25));
            GUILayout.EndHorizontal();
        }
        GUILayout.EndArea();

        if (OutputParam.Count > 0)
        {
            EditorGUI.DrawRect(new Rect(rect.x + Padding, rect.y + rect.height, 130, OutputParam.Count * 20), NodeColor * 0.8f);
            float nextHeight = 0;
            for (int i = 0; i < OutputParam.Count; i++)
            {
                if (OutputParam[i].ID >= NodeWindowEditor.instance.mDialogue.Parameters.Count)
                {
                    OutputParam.RemoveAt(i);
                    i--;
                }
                if (NodeWindowEditor.instance.mDialogue.Parameters[OutputParam[i].ID].Type == DialogParameter.type.Bool)
                {
                    GUI.Label(new Rect(rect.x + Padding, rect.y + rect.height + nextHeight, 130, 18), $"{NodeWindowEditor.instance.mDialogue.Parameters[OutputParam[i].ID].ParameterName} -> {OutputParam[i].state}");
                }
                else
                    GUI.Label(new Rect(rect.x + Padding, rect.y + rect.height + nextHeight, 130, 18), $"{NodeWindowEditor.instance.mDialogue.Parameters[OutputParam[i].ID].ParameterName} -> {OutputParam[i].intState}");
                nextHeight += 20;
            }
        }
    }

    private Vector2 SetMidPoint()
    {
        return new Vector2(rect.x + rect.width / 2, rect.y + rect.height / 2);
    }
    public Vector2 GetMidPoint()
    {
        return _midPoint;
    }
}



#if UNITY_EDITOR
[CustomEditor(typeof(Node))]
public class NodeEditor : Editor
{
    public Node t;

    // Scrol percent foreach BodyParts
    float[] ScrolBar;
    //charters name list
    string[] Charters;
    //charter BodyPart names list
    string[] BodyPartsNames;


    float LH = EditorGUIUtility.singleLineHeight;

    public SerializedProperty SelectedCharter;
    public SerializedProperty text;
    public SerializedProperty textSpeed;
    public SerializedProperty bodyParts;
    public SerializedProperty OutputParam;

    bool PropertiesChanged = false;
    int _SelectedCharter = 0;
    private void OnEnable()
    {
        t = (Node)target;

        Charters = DialogSettings.Instance.GetCharacters();
        BodyPartsNames = DialogSettings.Instance.GetCharactersBodyParts();
        ScrolBar = new float[BodyPartsNames.Length];

        //Find Propertise
        text = serializedObject.FindProperty("text");
        textSpeed = serializedObject.FindProperty("textSpeed");
        bodyParts = serializedObject.FindProperty("BodyParts");
        OutputParam = serializedObject.FindProperty("OutputParam");

        //BodyParts
        t.BodyParts = BodyParts<int>.Sorting(t.BodyParts, -1);
    }

    private void OnDisable()
    {
        NodeWindowEditor.instance.SaveDialog();
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        PropertiesChanged = false;

        Color DefaultCol = GUI.backgroundColor;
        float WindowWidth = EditorGUIUtility.currentViewWidth - 14;
        float LineHight = EditorGUIUtility.singleLineHeight;
        float WindowsHight = 0;

        float SelectionHeight = 8;
        float TextHeight = 70;
        float PositionHeight = 170;
        float BodyPartsHeight = 170;
        float ParametersHeight = 200;

        ////////////////////////////////////////////////////////   CHARACTER SELECTION
        ///
        if (t.Character != null && t.Character.BodyParts.Count > 0 && t.Character.BodyParts[0].Content.Count > 0 && t.Character.BodyParts[0].Content[0].ImgSprite != null)
        {
            if (t.BodyParts[0].Content != -1 && t.Character.BodyParts[0].Content[t.BodyParts[0].Content].ImgSprite != null)
                GUI.Box(new Rect(18, SelectionHeight, 35, 35), CustomGUI.GetTexture(t.Character.BodyParts[0].Content[t.BodyParts[0].Content].ImgSprite));
            else
                GUI.Box(new Rect(18, SelectionHeight, 35, 35), CustomGUI.GetTexture(t.Character.BodyParts[0].Content[0].ImgSprite));
        }
        else
            GUI.Box(new Rect(18, SelectionHeight, 35, 35), "Null");


        _SelectedCharter = EditorGUI.Popup(new Rect(60, 14, WindowWidth - 60, LH), "Selected Character:", DialogSettings.Instance.FindCharacter(t.Character) + 1, Charters);
        if (DialogSettings.Instance.FindCharacter(t.Character) + 1 != _SelectedCharter)
            PropertiesChanged = true; 


        CustomGUI.GuiLine(18, SelectionHeight + 50, 3);

        WindowsHight = 100;
        ////////////////////////////////////////////////////////   TEXT
        ///
        text.stringValue = EditorGUI.TextArea(new Rect(18, TextHeight, WindowWidth - 18, LH * 3), text.stringValue);

        GUI.Label(new Rect(18, TextHeight + 60, 80, LH), "Text speed:");
        textSpeed.floatValue = EditorGUI.Slider(new Rect(98, TextHeight + 60, WindowWidth - 98,LH), textSpeed.floatValue, 0.01f, 1f);
        CustomGUI.GuiLine(18, TextHeight + 85, 3);

        WindowsHight += 100;

        if (!t.Character)
        {
            GUI.Label(new Rect(18, BodyPartsHeight, WindowWidth - 18, LH), "Talking to yourself mode");
            WindowsHight += 80;
        }
        else
        {
            ////////////////////////////////////////////////////////   Position
            ///
            //if (DialogSettings.Instance.PosType == DialogSettings.posType.LeftRight)
            //{
                float ButtonWidth = (WindowWidth - 28) * 0.5f;

                if (t.Position == Node.CharacterPos.Left)
                    GUI.backgroundColor = Color.gray;
                if (GUI.Button(new Rect(21, PositionHeight, ButtonWidth, 40), "Left"))
                {
                    t.Position = Node.CharacterPos.Left;
                    PropertiesChanged = true;
                }
                GUI.backgroundColor = DefaultCol;


                if (t.Position == Node.CharacterPos.Right)
                    GUI.backgroundColor = Color.gray;
                if (GUI.Button(new Rect(24 + ButtonWidth, PositionHeight, ButtonWidth, 40), "Right"))
                {
                    t.Position = Node.CharacterPos.Right;
                    PropertiesChanged = true;
                }
                GUI.backgroundColor = DefaultCol;


                CustomGUI.GuiLine(18, PositionHeight + 55, 3);
                BodyPartsHeight += 65;
                ParametersHeight += 65;
                WindowsHight += 65;
            //}
            ////////////////////////////////////////////////////////   BODY PARTS SELECTION
            ///

            GUI.Label(new Rect(18, BodyPartsHeight, WindowWidth - 18, LH), "Selected Body Parts:");
            float nextHeight = 0;

            //////////////////////// PARTS
            for (int i = 0; i < BodyPartsNames.Length; i++)
            {
                int Content = t.BodyParts[i].Content;

                CustomGUI.GuiLine(38, BodyPartsHeight + 27 + nextHeight, 1);

                string[] Images = t.Character.GetCharacterImages(i);

                if (Images.Length > 0)
                {
                    if (Content >= Images.Length)
                        Content = -1;

                    string labl = "";
                    if (Content >= 0)
                    {
                        if (t.Character.BodyParts[i].BodyName != "")
                            labl = $"{t.BodyParts[i].BodyName}: {t.Character.BodyParts[i].BodyName}_Part";
                        else
                            labl = $"{t.BodyParts[i].BodyName}: Part_{Content + 1}";
                    }
                    else
                    {
                        labl = $"CHOSE {BodyPartsNames[i].ToUpper()} PART";
                        GUI.backgroundColor = Color.red;
                    }

                    //////////////////////// Selected image
                    GUI.Label(new Rect(38, BodyPartsHeight + 30 + nextHeight, 120, LH), labl);
                    if (Content > -1 && t.Character.BodyParts[i].Content.Count > 0 && t.Character.BodyParts[i].Content[Content].ImgSprite != null)
                        GUI.Box(new Rect(38, BodyPartsHeight + 50 + nextHeight, 120, 120), CustomGUI.GetTexture(t.Character.BodyParts[i].Content[Content].ImgSprite));
                    else
                        GUI.Box(new Rect(38, BodyPartsHeight + 50 + nextHeight, 120, 120), "Null");

                    GUI.backgroundColor = DefaultCol;


                    //////////////////////// SCROL BAR
                    float ScrolWidth = 110 * Images.Length;
                    float ScrolRight = ScrolWidth - (WindowWidth - 170);
                    if (WindowWidth - 170 < ScrolWidth)
                        ScrolBar[i] = GUI.HorizontalScrollbar(new Rect(170, BodyPartsHeight + 158 + nextHeight, WindowWidth - 170, 20), ScrolBar[i], ScrolWidth - ScrolRight, 0, ScrolWidth);
                    else
                        ScrolBar[i] = 0;

                    //////////////////////// SCROL BAR ELEMENTS
                    GUI.BeginGroup(new Rect(170, BodyPartsHeight + 50 + nextHeight, WindowWidth - 180, 100));
                    GUI.BeginGroup(new Rect(-ScrolBar[i], 0, ScrolWidth, 100));
                    for (int a = 0; a < t.Character.BodyParts[i].Content.Count; a++)
                    {
                        if (t.Character.BodyParts[i].Content[a].ImgSprite != null)
                        {
                            if (GUI.Button(new Rect(0 + 110 * a, 0, 100, 100), CustomGUI.GetTexture(t.Character.BodyParts[i].Content[a].ImgSprite)))
                                Content = a;
                        }
                        else
                        {
                            if (GUI.Button(new Rect(0 + 110 * a, 0, 100, 100), "Null"))
                                Content = a;
                        }
                    }
                    GUI.EndGroup();
                    GUI.EndGroup();
                    nextHeight += 150;

                    if (Content != t.BodyParts[i].Content)
                    {
                        PropertiesChanged = true;
                        t.BodyParts[i].Content = Content;
                    }
                }
                else
                {
                    GUI.Label(new Rect(38, BodyPartsHeight + 30 + nextHeight, 200, LH), $"{BodyPartsNames[i].ToUpper()} parts is Empty");
                    nextHeight += 30;
                }
            }

            CustomGUI.GuiLine(38, BodyPartsHeight + 27 + nextHeight, 1);

            WindowsHight += nextHeight;
            ParametersHeight += nextHeight + 15;
        }



        ////////////////////////////////////////////////////////   PARAMETERS SELECTION
        ///

        if (NodeWindowEditor.instance.mDialogue.Parameters.Count > 0)
        {
            CustomGUI.GuiLine(18, ParametersHeight, 3);
            GUI.Label(new Rect(18, ParametersHeight + 5, 170, LineHight), "Set parameters value:");

            WindowsHight += 60;
            ParametersHeight += 28;
            float ParamHeight = 0;
            for (int i = 0; i < OutputParam.arraySize; i++)
            {
                SerializedProperty CurOutputParam = OutputParam.GetArrayElementAtIndex(i);
                SerializedProperty ParamID = CurOutputParam.FindPropertyRelative("ID");
                SerializedProperty Paramstate = CurOutputParam.FindPropertyRelative("state");
                SerializedProperty ParamintState = CurOutputParam.FindPropertyRelative("intState");


                if (ParamID.intValue >= NodeWindowEditor.instance.mDialogue.GetParameters().Length)
                {
                    OutputParam.DeleteArrayElementAtIndex(i);
                    break;
                }


                ParamID.intValue = EditorGUI.Popup(new Rect(18, ParametersHeight + ParamHeight, WindowWidth - 120, LineHight), ParamID.intValue, NodeWindowEditor.instance.mDialogue.GetParameters());


                if (NodeWindowEditor.instance.mDialogue.Parameters[t.OutputParam[i].ID].Type == DialogParameter.type.Bool)
                {
                    Paramstate.boolValue = EditorGUI.Toggle(new Rect(WindowWidth - 98, ParametersHeight + ParamHeight, 10, LineHight), Paramstate.boolValue);
                    GUI.Label(new Rect(WindowWidth - 80, ParametersHeight + ParamHeight, 50, LineHight), Paramstate.boolValue.ToString());
                }
                else
                    ParamintState.intValue = EditorGUI.IntField(new Rect(WindowWidth - 98, ParametersHeight + ParamHeight, 75, LineHight), ParamintState.intValue);


                if (GUI.Button(new Rect(WindowWidth - 20, ParametersHeight + ParamHeight, 20, LineHight), "-"))
                {
                    OutputParam.DeleteArrayElementAtIndex(i);
                    break;
                }
                ParamHeight += LineHight + 2;
            }
            if (GUI.Button(new Rect(18, ParametersHeight + ParamHeight, WindowWidth - 20, LineHight + 2), "Add New"))
                OutputParam.InsertArrayElementAtIndex(OutputParam.arraySize);


            WindowsHight += ParamHeight;
        }


        GUILayout.Space(WindowsHight);


        CheckModifiedProperties();
    }
    public void CheckModifiedProperties()
    {
        if (serializedObject.hasModifiedProperties || PropertiesChanged)
        {
            serializedObject.ApplyModifiedProperties();
            if (_SelectedCharter != 0)
            {
                t.Character = DialogSettings.Instance.Charters[_SelectedCharter - 1];
                t.name = $"{t.Character.Name}";
            }
            else
            {
                t.Character = null;
                t.name = "Talking to Yourself";
            }

            NodeWindowEditor.instance.Repaint();
        }
    }
}
#endif
