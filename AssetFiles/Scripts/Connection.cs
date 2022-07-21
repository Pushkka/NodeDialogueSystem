using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
#endif

[System.Serializable]
public class Connection : ScriptableObject
{
    public Node From;
    public Node To;


    public ConnectionType Type;
    public string Answer = "Your Answer";
    public float Time = 0;

    //[System.NonSerialized]
    public int[] SameParam = new int[0];
    public List<ConectionParameters> InputParam = new List<ConectionParameters>();


    private Vector2 _midPoint;
    private Vector2 Start;
    private Vector2 End;

    public enum ConnectionType
    {
        Action,
        Time,
        Answer
    }

    public void set(Node from, Node to)
    {
        From = from;
        To = to;
    }
    public void Draw()
    {
        Start = From.GetMidPoint();
        End = To.GetMidPoint();



        if (To.Connections.FindIndex(x => x.To == From) != -1)
        {
            Vector2 smesh = (End - Start).normalized * 10;
            smesh = new Vector2(smesh.y, -smesh.x);
            Start -= smesh;
            End -= smesh;
        }

        _midPoint = Start + (End - Start) * 0.5f;

        Color OldColor = GUI.backgroundColor;


        if (SameParam.Length >= 2)
        {
            CustomGUI.DrawArrow(Start, End, DialogSettings.Instance.AnswerConectionColor);

            if (Answer != "")
            {
                GUI.backgroundColor = DialogSettings.Instance.AnswerConectionColor;
                DrawText(Answer);
            }
            else
            {
                GUI.backgroundColor = Color.red;
                DrawText("Empty!!!");
            }
        }
        else
        {
            switch (Type)
            {
                case ConnectionType.Action:
                    CustomGUI.DrawArrow(Start, End, DialogSettings.Instance.ActionConectionColor);
                    break;
                case ConnectionType.Time:
                    CustomGUI.DrawArrow(Start, End, DialogSettings.Instance.TimeConectionColor);

                    GUI.backgroundColor = DialogSettings.Instance.TimeConectionColor;
                    DrawText($"{Time} sec");
                    break;
                case ConnectionType.Answer:
                    CustomGUI.DrawArrow(Start, End, DialogSettings.Instance.AnswerConectionColor);

                    if (Answer != "")
                    {
                        GUI.backgroundColor = DialogSettings.Instance.AnswerConectionColor;
                        DrawText(Answer);
                    }
                    else
                    {
                        GUI.backgroundColor = Color.red;
                        DrawText("Empty!!!");
                    }
                    break;
            }
            GUI.backgroundColor = OldColor;
        }


        for (int i = 0; i < Selection.objects.Length; i++)
        {
            if (Selection.objects[i] == this)
            {
                CustomGUI.DrawArrow(Start, End, DialogSettings.Instance.SelectionConectionColor);
                break;
            }
        }
    }
    public Vector2 GetConMidPoint(float Percent)
    {
        Start = From.GetMidPoint();
        End = To.GetMidPoint();
        Vector2 _midPoint = Start + (End - Start) * Percent;

        return _midPoint;
    }
    public void DrawText(string text)
    {
        float Ysmesh;
        GUIStyle myStyle = new GUIStyle();
        Vector2 size;

        size = myStyle.CalcSize(new GUIContent(text));

        Ysmesh = -8 - size.y;
        if (End.y < Start.y)
            Ysmesh = 8;


        CustomGUI.DrawBox(_midPoint.x, _midPoint.y + Ysmesh, text);
        //GUI.Box(new Rect(_midPoint.x - size.x / 2, _midPoint.y + Ysmesh, size.x + 20, size.y + 6), text);

    }
    public bool Contains(Vector2 Point)
    {
        Vector2 line = (End - Start);
        float len = line.magnitude;
        line.Normalize();

        Vector2 v = Point - Start;
        float d = Vector2.Dot(v, line);
        d = Mathf.Clamp(d, 0f, len);

        if(Vector2.Distance(Start + line * d, Point) < 10)
            return true;
        else
            return false;
    }
}




#if UNITY_EDITOR
[CustomEditor(typeof(Connection))]
[CanEditMultipleObjects]
public class ConnectionEditor : Editor
{
    public Connection t;

    public SerializedProperty Type;
    public SerializedProperty Answer;
    public SerializedProperty Time;
    public SerializedProperty InputParam;
    private void OnEnable()
    {
        t = (Connection)target;

        Type = serializedObject.FindProperty("Type");
        Answer = serializedObject.FindProperty("Answer");
        Time = serializedObject.FindProperty("Time");
        InputParam = serializedObject.FindProperty("InputParam");
    }

    private void OnDisable()
    {
        NodeWindowEditor.instance.SaveDialog();
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        if (t.SameParam.Length >= 2)
        {
            GUILayout.Label("Branching detected");

            GUILayout.Space(10);
            CustomGUI.GuiLine(2);
            GUILayout.Space(10);

            GUILayout.Label("Answer:");
            Answer.stringValue = EditorGUILayout.TextArea(Answer.stringValue, GUILayout.Height(50));
            GUILayout.Space(10);
            CustomGUI.GuiLine(2);
            GUILayout.Space(10);

        }
        else
        {
            Type.enumValueIndex = (int)(Connection.ConnectionType)EditorGUILayout.EnumPopup("Connection Type:", t.Type);

            GUILayout.Space(10);
            CustomGUI.GuiLine(2);
            GUILayout.Space(10);

            switch (t.Type)
            {
                case Connection.ConnectionType.Time:
                    GUILayout.Label("Time Delay:");
                    Time.floatValue = EditorGUILayout.FloatField(Time.floatValue);
                    GUILayout.Space(10);
                    CustomGUI.GuiLine(2);
                    GUILayout.Space(10);
                    break;
                case Connection.ConnectionType.Answer:
                    GUILayout.Label("Answer:");
                    Answer.stringValue = EditorGUILayout.TextArea(Answer.stringValue, GUILayout.Height(50));
                    GUILayout.Space(10);
                    CustomGUI.GuiLine(2);
                    GUILayout.Space(10);
                    break;
            }
        }

        if (NodeWindowEditor.instance.mDialogue.Parameters.Count > 0)
        {

            if (InputParam.hasMultipleDifferentValues)
            {
                GUILayout.Label("Has Multiple Different Parameters!", EditorStyles.boldLabel);
            }
            else
            {
                GUILayout.Label("Available on parameters value:", EditorStyles.boldLabel);

                for (int i = 0; i < InputParam.arraySize; i++)
                {
                    SerializedProperty CurOutputParam = InputParam.GetArrayElementAtIndex(i);
                    SerializedProperty ParamID = CurOutputParam.FindPropertyRelative("ID");
                    SerializedProperty Paramstate = CurOutputParam.FindPropertyRelative("state");
                    SerializedProperty ParamintState = CurOutputParam.FindPropertyRelative("intState");
                    SerializedProperty intType = CurOutputParam.FindPropertyRelative("intType");

                    if (ParamID.intValue >= NodeWindowEditor.instance.mDialogue.GetParameters().Length)
                    {
                        InputParam.DeleteArrayElementAtIndex(i);
                        break;
                    }


                    GUILayout.BeginHorizontal();
                    ParamID.intValue = EditorGUILayout.Popup(ParamID.intValue, NodeWindowEditor.instance.mDialogue.GetParameters(), GUILayout.Width(110));
                    if (NodeWindowEditor.instance.mDialogue.Parameters[ParamID.intValue].Type == DialogParameter.type.Bool)
                    {
                        Paramstate.boolValue = EditorGUILayout.Toggle(Paramstate.boolValue, GUILayout.Width(20));
                        GUILayout.Label(Paramstate.boolValue.ToString());
                    }
                    else
                    {
                        ParamintState.intValue = EditorGUILayout.IntField(ParamintState.intValue);
                        intType.enumValueIndex = (int)(DialogParameter.inttype)EditorGUILayout.EnumPopup(t.InputParam[i].intType);
                    }
                    if (GUILayout.Button("-", GUILayout.Width(20)))
                    {
                        InputParam.DeleteArrayElementAtIndex(i);
                        break;
                    }
                    GUILayout.EndHorizontal();
                }
                if (GUILayout.Button("Add New"))
                    InputParam.InsertArrayElementAtIndex(InputParam.arraySize);
            }
        }

        if (serializedObject.hasModifiedProperties)
        {
            serializedObject.ApplyModifiedProperties();

            NodeWindowEditor.instance.Repaint();
            NodeWindowEditor.instance.CheckConnections();
        }
    }
}
#endif
