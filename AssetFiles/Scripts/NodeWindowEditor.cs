using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
#endif

public class NodeWindowEditor : EditorWindow
{
    public static NodeWindowEditor instance;
    public Dialog mDialogue;



    public float zoomScale = 1.0f;
    public Vector2 ScrollPosition;


    private Vector2 offset;
    private Vector2 drag;
    private Vector2 OldScreenSize = new Vector2(Screen.width, Screen.height);
    private bool draging;


    private bool OpenParameters;
    private bool CtrlD;
    private bool Connecting;
    private Object[] LastSelected = new Object[0];
    private bool Selecting;
    private Rect SelectRect = new Rect();
    public List<Node> SelectedNodes = new List<Node>();
    private List<Connection> SelectedConnection = new List<Connection>();

    //Open Asset
    [MenuItem("Window/Node Dialogue Sytem/Dialogue Editor")]
    public static NodeWindowEditor OpenEditorWindow()
    {
        NodeWindowEditor window = GetWindow<NodeWindowEditor>();
        window.titleContent = new GUIContent("Node Based Editor");
        return window;
    }

    [UnityEditor.Callbacks.OnOpenAsset(1)]
    public static bool OnOpenAsset(int instanceID, int line)
    {
        Dialog Dialogue = EditorUtility.InstanceIDToObject(instanceID) as Dialog;
        if(Dialogue!= null)
        {
            NodeWindowEditor Editor = OpenEditorWindow();
            Editor.mDialogue = Dialogue;
            return true;
        }
        return false;
    }

    private void OnEnable()
    {
        instance = this;
        DialogSettings.SetInstance();
    }
    private void OnDisable()
    {
        SaveDialog();
    }

    public void SaveDialog()
    {
        EditorUtility.SetDirty(mDialogue);
        mDialogue.SaveAsset();
        mDialogue.UpdateAssets();
        Repaint();
    }
    //Editor Window
    private void OnGUI()
    {
        Matrix4x4 oldMatrix = GUI.matrix;
        GUI.EndGroup();

        GUI.BeginGroup(new Rect(1f, 21f, Screen.width / zoomScale - 2, Screen.height / zoomScale - 23 / zoomScale));
        Matrix4x4 translation = Matrix4x4.TRS(new Vector2(0, 21), Quaternion.identity, Vector3.one);
        Matrix4x4 scale = Matrix4x4.Scale(new Vector3(zoomScale, zoomScale, 1.0f));
        GUI.matrix = translation * scale * translation.inverse * GUI.matrix;

        EditorGUI.DrawRect(new Rect(0, 0, Screen.width / zoomScale, Screen.height / zoomScale), DialogSettings.Instance.BackgroundColor);
        DrawGrid(20, 0.2f, Color.gray * 0.1f);
        DrawGrid(100, 0.4f, Color.gray * 0.1f);

        if (mDialogue != null)
        {
            CheckSelection();
            
            //Callback All Events
            if (!GlobalEvents(Event.current)) //Global
                if (!ProcessNodeEvents(Event.current)) //Node
                    if (!ProcessConnectEvents(Event.current)) //Connections
                        ProcessEvents(Event.current); //Window
           

            if (mDialogue.nodes != null)
            {
                //Draw All Transition Line
                for (int i = 0; i < mDialogue.Connections.Count; i++)
                {
                    mDialogue.Connections[i].Draw();
                }
                GUI.backgroundColor = Color.gray;
                if (mDialogue.Parameters.Count > 0)
                    DrawSameConnection();

                //Draw New Transition Line
                if (Connecting)
                {
                    foreach (var item in SelectedNodes)
                        CustomGUI.DrawArrow(item.GetMidPoint(), Event.current.mousePosition, Color.white);

                    GUI.changed = true;
                }

                //Draw Nodes
                for (int i = 0; i < mDialogue.nodes.Count; i++)
                {
                    bool start = false;
                    if (mDialogue.StartNode == mDialogue.nodes[i])
                        start = true;
                    mDialogue.nodes[i].Draw(start);
                }

                if (Selecting)
                {
                    SelectRect.size = -SelectRect.position + Event.current.mousePosition;
                    EditorGUI.DrawRect(SelectRect, DialogSettings.Instance.SelectionColor);
                }
            }



            GUI.matrix = oldMatrix;
            GUI.backgroundColor = Color.gray;

            GUIStyle CAlign = new GUIStyle(EditorStyles.label);
            CAlign.alignment = TextAnchor.MiddleCenter;

            if (!OpenParameters)
            {
                EditorGUI.DrawRect(new Rect(15, 15, 210, 50), DialogSettings.Instance.BackgroundColor * 0.5f);
                GUILayout.BeginArea(new Rect(20, 20, 200, 40));
                GUILayout.Label("PARAMETERS:", CAlign);
                CustomGUI.GuiLine(1);
                if (GUILayout.Button("OPEN"))
                    OpenParameters = true;
            }
            else
            {
                Rect windowArea = new Rect(15, 15, 210, (EditorGUIUtility.singleLineHeight + 2.7f) * (3 + mDialogue.Parameters.Count) + 18);
                EditorGUI.DrawRect(windowArea, DialogSettings.Instance.BackgroundColor * 0.5f);
                GUILayout.BeginArea(new Rect(20, 20, 200, (EditorGUIUtility.singleLineHeight + 2.7f) * (3 + mDialogue.Parameters.Count) + 8));
                GUILayout.Label("PARAMETERS:", CAlign);
                CustomGUI.GuiLine(1);
                for (int i = 0; i < mDialogue.Parameters.Count; i++)
                {
                    GUILayout.BeginHorizontal();
                    mDialogue.Parameters[i].ParameterName = EditorGUILayout.TextField(mDialogue.Parameters[i].ParameterName);
                    mDialogue.Parameters[i].Type = (DialogParameter.type)EditorGUILayout.EnumPopup(mDialogue.Parameters[i].Type);
                    if (GUILayout.Button("-"))
                    {
                        mDialogue.Parameters.RemoveAt(i);
                        CheckConnections();
                        SaveDialog();
                        break;
                    }
                    GUILayout.EndHorizontal();
                    //if (mDialogue.Parameters[i].ParameterName != Parametername)
                    //{
                    //    mDialogue.Parameters[i].ParameterName = Parametername;
                    //    //Changes = true;
                    //}
                }
                if (GUILayout.Button("Add New"))
                    mDialogue.Parameters.Add(new DialogParameter($"Parameter_" + (mDialogue.Parameters.Count + 1)));
                CustomGUI.GuiLine(1);
                if (GUILayout.Button("CLOSE"))
                    OpenParameters = false;
            }
            GUILayout.EndArea();
        }

        

        GUI.EndGroup();

        EditorGUI.DrawRect(new Rect(Screen.width - 340, 30, 110, 30), DialogSettings.Instance.BackgroundColor * 0.5f);
        if(GUI.Button(new Rect(Screen.width - 335, 35, 100, 20), "Manual Save"))
        {
            SaveDialog();
        }

        GUI.BeginGroup(new Rect(0, 21f, Screen.width, Screen.height));
    }

    //—ÔËÒÓÍ ‰ÂÈÒÚ‚ËÈ ¬≈«ƒ≈
    private bool GlobalEvents(Event e)
    {
        switch (e.type)
        {
            case EventType.MouseUp:
                if (e.button == 0)
                {
                    if (Selecting)
                    {
                        DeSelectNode();
                        for (int i = 0; i < mDialogue.nodes.Count; i++)
                        {
                            if (mDialogue.nodes[i].—onsists(SelectRect))
                                SelectNode(i);
                        }
                        for (int i = 0; i < mDialogue.Connections.Count; i++)
                        {
                            if(SelectRect.Contains(mDialogue.Connections[i].GetConMidPoint(0.5f), true))
                                SelectConection(i);
                        }

                        Selecting = false;
                        GUI.changed = true;
                    }
                    if (draging)
                    {
                        draging = false;
                    }
                }
                break;
            case EventType.KeyDown:
                if (e.keyCode == KeyCode.Delete)
                {
                    if (SelectedConnection.Count > 0)
                    {
                        Connecting = false;
                        foreach (var item in SelectedConnection)
                            mDialogue.DeleteConnection(item);
                        DeSelectNode();
                    }
                    if (SelectedNodes.Count>0)
                    {
                        Connecting = false;
                        foreach (var item in SelectedNodes)
                            mDialogue.DeleteNode(item);
                        DeSelectNode();
                    }
                    CheckConnections();
                    mDialogue.UpdateAssets();
                    return true;
                }
                if (e.keyCode == KeyCode.Escape)
                {
                    if (Connecting)
                    {
                        Connecting = false;
                        return true;
                    }
                    if (SelectedNodes.Count > 0)
                    {
                        DeSelectNode();
                        GUI.changed = true;
                        return true;
                    }
                }
                if (e.control && e.keyCode == KeyCode.D)
                {
                    if (SelectedNodes.Count > 0 && !CtrlD)
                    {
                        Vector2 startPos = SelectedNodes[0].GetMidPoint();
                        foreach (var item in SelectedNodes)
                            mDialogue.CrerateNode(item.GetMidPoint() - startPos + e.mousePosition, item);
                    }
                    CtrlD = true;
                    CheckConnections();
                }
                if (e.control && e.keyCode == KeyCode.Z)
                {
                    GUI.changed = true;
                    CheckConnections();
                    mDialogue.UpdateAssets();
                    return true;
                }
                break;
            case EventType.KeyUp:
                if (e.keyCode == KeyCode.D)
                    CtrlD = false;
                break;
            case EventType.ScrollWheel:
                float zoom = 0;
                if (e.delta.y > 0)
                    zoom = -1;
                else
                    zoom = 1;


                zoomScale += 0.2f * zoom;
                zoomScale = Mathf.Clamp(zoomScale, 0.2f, 3f);
                Vector2 newScreenSize = new Vector2(Screen.width, Screen.height) / zoomScale;

                Vector2 mousePosNormal = new Vector2( Mathf.InverseLerp(0, OldScreenSize.x, e.mousePosition.x), Mathf.InverseLerp(0, OldScreenSize.y, e.mousePosition.y));

                Vector2 ScreenDiference = OldScreenSize - newScreenSize;

                OnDrag(-ScreenDiference * mousePosNormal, 1);

                OldScreenSize = newScreenSize;

                Repaint();
                break;

            case EventType.MouseDrag:
                if (Selecting)
                {
                    GUI.changed = true;
                    return true;
                }
                if (draging)
                {
                    foreach (var item in SelectedNodes)
                    {
                        item.Drag(e.delta);
                    }
                    GUI.changed = true;
                    return true;
                }
                break;
        }
        return false;
    }
    //—ÔËÒÓÍ ‰ÂÈÒÚ‚ËÈ Ì‡ ÌÓ‰Â
    private bool ProcessNodeEvents(Event e)
    {
        for (int i = mDialogue.nodes.Count - 1; i >= 0; i--)
        {
            Node CurentNode = mDialogue.nodes[i];
            if (CurentNode.Contains(e.mousePosition))
            {
                switch (e.type)
                {
                    case EventType.MouseDown:
                        if (e.button == 0) ////////À Ã
                        {
                            if (e.clickCount == 2)
                            {
                                //DOUBLE CLICK

                                if (CurentNode.Character)
                                    Selection.activeObject = CurentNode.Character;

                                GUI.changed = true;
                            }
                            else
                            {
                                //CLICK

                                if (Connecting)
                                {
                                    foreach (var item in SelectedNodes)
                                        mDialogue.CreateConnection(item, CurentNode);
                                    Connecting = false;
                                    draging = false; 
                                    CheckConnections();
                                }
                                else
                                {
                                    if (!SelectedNodes.Contains(CurentNode))
                                    {
                                        if (!e.shift)
                                            DeSelectNode();
                                        SelectNode(i);
                                    }
                                    draging = true;
                                }

                                GUI.changed = true;
                            }
                            return true;
                        }

                        if (e.button == 1) ////////œ Ã
                        {
                            NodeProcessContextMenu(e.mousePosition, i);
                            GUI.changed = true;
                            return true;
                        }
                        break;
                    case EventType.MouseDrag:
                        if (e.button == 0)
                        {
                            draging = true;
                        }
                        break;
                }
            }
        }
        return false;
    }

    //—ÔËÒÓÍ ‰ÂÈÒÚ‚ËÈ Ì‡ ÎËÌËˇı
    private bool ProcessConnectEvents(Event e)
    {
        for (int i = mDialogue.Connections.Count - 1; i >= 0; i--)
        {
            Connection CurentConnect = mDialogue.Connections[i];
            if (CurentConnect.Contains(e.mousePosition))
            {
                switch (e.type)
                {
                    case EventType.MouseDown:
                        if (e.button == 0) ////////À Ã
                        {
                            if (!SelectedConnection.Contains(CurentConnect))
                            {
                                if (!e.shift)
                                    DeSelectNode();
                                SelectConection(i);
                            }
                            GUI.changed = true;
                            return true;
                        }

                        if (e.button == 1) ////////œ Ã
                        {
                            Connecting = false;
                            ConnectProcessContextMenu(i);
                            return true;
                        }
                        break;
                }
            }
        }
        return false;
    }
    //—ÔËÒÓÍ ‰ÂÈÒÚ‚ËÈ
    private void ProcessEvents(Event e)
    {
        drag = Vector2.zero;

        switch (e.type)
        {
            case EventType.MouseDown:
                if (e.button == 0) ////////À Ã
                {
                    DeSelectNode();
                    Connecting = false;
                    GUI.changed = true;
                }

                if (e.button == 1) ////////œ Ã
                {
                    DeSelectNode();
                    Connecting = false;
                    ProcessContextMenu(e.mousePosition);
                }
                break;
            case EventType.MouseDrag:
                if (e.button == 0 && e.delta.magnitude < 10) ////////— Ã
                {
                    SelectRect.position = e.mousePosition;
                    Selecting = true;
                }
                if (e.button == 2) ////////— Ã
                {
                    OnDrag(e.delta, 1f);
                }
                break;
        }
    }


    //œÂÂÏÂ˘ÂÌËÂ
    private void OnDrag(Vector2 delta, float BackroundModif)
    {
        drag = delta;
        offset += drag * BackroundModif;

        if (mDialogue.nodes != null)
        {
            for (int i = 0; i < mDialogue.nodes.Count; i++)
            {
                mDialogue.nodes[i].Drag(delta);
            }
        }

        GUI.changed = true;
    }

    //ÃÂÌ˛ œ Ã
    private void ProcessContextMenu(Vector2 mousePosition)
    {
        GenericMenu genericMenu = new GenericMenu();
        genericMenu.AddItem(new GUIContent("Create Empty state"), false, () => mDialogue.CrerateNode(mousePosition));
        genericMenu.ShowAsContext();
    }
    //ÃÂÌ˛ œ Ã ƒÀﬂ ÕŒƒ€
    private void NodeProcessContextMenu(Vector2 mousePosition, int nodeID)
    {
        DeSelectNode();
        Connecting = false;

        GenericMenu genericMenu = new GenericMenu();
        genericMenu.AddItem(new GUIContent("Make Transition"), false, () =>
        {
            SelectNode(nodeID);
            Connecting = true;
        });
        genericMenu.AddItem(new GUIContent("Continue With Empty state"), false, () =>
        {
            Node NextNode = mDialogue.CrerateNode(mDialogue.nodes[nodeID].GetMidPoint() + new Vector2(150, -50));
            mDialogue.CreateConnection(mDialogue.nodes[nodeID], NextNode);
            SelectNode(nodeID);
            CheckConnections();
        });
        genericMenu.AddItem(new GUIContent("Set as Start state"), false, () => mDialogue.SetStartNode(mDialogue.nodes[nodeID]));
        genericMenu.AddItem(new GUIContent("Delete"), false, () => mDialogue.DeleteNode(mDialogue.nodes[nodeID]));
        genericMenu.ShowAsContext();
    }
    //ÃÂÌ˛ œ Ã ƒÀﬂ Connection
    private void ConnectProcessContextMenu(int connectID)
    {
        DeSelectNode();
        Connecting = false;

        GenericMenu genericMenu = new GenericMenu();
        genericMenu.AddItem(new GUIContent("Delete"), false, () => mDialogue.DeleteConnection(mDialogue.Connections[connectID]));
        genericMenu.ShowAsContext();
    }


    public void CheckConnections()
    {
        //Debug.Log("Checking");
        int paramCount = NodeWindowEditor.instance.mDialogue.GetParameters().Length;
        for (int i = 0; i < mDialogue.Connections.Count; i++)
        {
            for (int a = 0; a < mDialogue.Connections[i].InputParam.Count; a++)
            {
                if (mDialogue.Connections[i].InputParam[a].ID >= paramCount)
                {
                    mDialogue.Connections[i].InputParam.RemoveAt(a);
                    a--;
                }
            }



            Connection CurCon = mDialogue.Connections[i];

            List<Connection> SameParameters = CurCon.From.Connections.FindAll(x => ConectionParameters.EqualParameters(CurCon.InputParam, x.InputParam) == true);



            if (SameParameters.Count >= 2)
            {
                mDialogue.Connections[i].SameParam = new int[SameParameters.Count];

                for (int a = 0; a < SameParameters.Count; a++)
                    mDialogue.Connections[i].SameParam[a] = CurCon.From.Connections.IndexOf(SameParameters[a]);
            }
            else
            {
                mDialogue.Connections[i].SameParam = new int[0];
            }
        }
    }
    private void CheckSelection()
    {
        if (Selection.objects.Length > 0)
        {
            if (LastSelected.Length != Selection.objects.Length || LastSelected[0] != Selection.objects[0] || LastSelected[LastSelected.Length - 1] != Selection.objects[Selection.objects.Length - 1])
            {
                LastSelected = Selection.objects;
                SelectedNodes.Clear();
                SelectedConnection.Clear();
                for (int i = 0; i < Selection.objects.Length; i++)
                {
                    if (Selection.objects[i].GetType() == typeof(Node))
                    {
                        Node selected = mDialogue.nodes.Find(x => x == Selection.objects[i]);
                        if (selected)
                            SelectedNodes.Add(selected);
                        continue;
                    }
                    if (Selection.objects[i].GetType() == typeof(Connection))
                    {
                        Connection selected = mDialogue.Connections.Find(x => x == Selection.objects[i]);
                        if (selected)
                            SelectedConnection.Add(selected);
                        continue;
                    }
                }
            }
        }
        else
        {
            LastSelected = new Object[0];
            SelectedNodes.Clear();
            SelectedConnection.Clear();
        }
    }
    private void SelectNode(int ID)
    {
        //SaveDialog();
        List<Object> selected = new List<Object>();
        selected.AddRange(Selection.objects);
        selected.Add((Object)mDialogue.nodes[ID]);
        Selection.objects = selected.ToArray();
    }
    private void SelectConection(int ID)
    {
        //SaveDialog();
        List<Object> selected = new List<Object>();
        selected.AddRange(Selection.objects);
        selected.Add((Object)mDialogue.Connections[ID]);
        Selection.objects = selected.ToArray();
    }
    private void DeSelectNode()
    {
        Selection.objects = null;
    }

    //√–¿‘» ¿

    private void DrawSameConnection()
    {
        List<Connection> AlreadyAdded = new List<Connection>();
        for (int i = 0; i < mDialogue.Connections.Count; i++)
        {
            Connection CurCon = mDialogue.Connections[i];
            List<Connection> From = CurCon.From.Connections;


            string text = "";
            if (CurCon.InputParam.Count > 0)
            {
                for (int a = 0; a < CurCon.InputParam.Count; a++)
                {
                    text += mDialogue.Parameters[CurCon.InputParam[a].ID].ParameterName;

                    if (mDialogue.Parameters[CurCon.InputParam[a].ID].Type == DialogParameter.type.Bool)
                        text += $" == {CurCon.InputParam[a].state}";
                    else
                        text += $" {CurCon.InputParam[a].GetIntType()} {CurCon.InputParam[a].intState}";

                    if (a < CurCon.InputParam.Count - 1)
                        text += "\n";
                }
            }

            if (CurCon.SameParam.Length >= 2)
            {
                if (AlreadyAdded.Contains(CurCon))
                    continue;
                for (int a = 1; a < CurCon.SameParam.Length; a++)
                {
                    if (CurCon.SameParam[a] >= From.Count)
                        break;
                    Vector2 Start = From[CurCon.SameParam[a - 1]].GetConMidPoint(0.3f);
                    Vector2 End = From[CurCon.SameParam[a]].GetConMidPoint(0.3f);

                    AlreadyAdded.Add(From[CurCon.SameParam[a - 1]]);

                    Vector2 _midPoint = Start + (End - Start) * 0.5f;

                    CustomGUI.DrawLine(Start, End, DialogSettings.Instance.AnswerConectionColor);

                    if (a == CurCon.SameParam.Length / 2 && CurCon.InputParam.Count > 0)
                    {
                        CustomGUI.DrawBox(_midPoint.x, _midPoint.y - 10, text, DialogSettings.Instance.BackgroundColor * 0.8f);
                    }
                }
                AlreadyAdded.Add(From[CurCon.SameParam[CurCon.SameParam.Length - 1]]);
            }
            else
            if(CurCon.InputParam.Count > 0)
            {
                CustomGUI.DrawBox(CurCon.GetConMidPoint(0.3f).x, CurCon.GetConMidPoint(0.3f).y - 20, text, DialogSettings.Instance.BackgroundColor * 0.8f);
            }

            //else
            //    text = "No parameters";

        }
    }
    public void DrawGrid(float gridSpacing, float gridOpacity, Color gridColor)
    {
        int widthDivs = (int)((Mathf.CeilToInt(position.width / gridSpacing) + 1) / zoomScale);
        int heightDivs = (int)((Mathf.CeilToInt(position.height / gridSpacing) + 1) / zoomScale);

        Handles.BeginGUI();
        Handles.color = new Color(gridColor.r, gridColor.g, gridColor.b, gridOpacity);

        //offset += drag * 0.5f;
        Vector3 newOffset = new Vector3(offset.x % gridSpacing, offset.y % gridSpacing, 0);

        for (int i = 0; i < widthDivs + 1; i++)
        {
            Handles.DrawLine(new Vector3(gridSpacing * i, -gridSpacing, 0) + newOffset, new Vector3(gridSpacing * i, (position.height + 500) / zoomScale, 0f) + newOffset);
        }

        for (int j = 0; j < heightDivs + 1; j++)
        {
            Handles.DrawLine(new Vector3(-gridSpacing, gridSpacing * j, 0) + newOffset, new Vector3((position.width + 500) / zoomScale, gridSpacing * j, 0f) + newOffset);
        }

        Handles.color = Color.white;
        Handles.EndGUI();
    }

}