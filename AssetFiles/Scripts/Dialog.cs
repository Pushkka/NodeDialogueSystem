using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
#endif


[CreateAssetMenu(fileName = "New Dialog", menuName = "Dialogue Pro/Dialog")]
public class Dialog : ScriptableObject
{
    public List<Node> nodes = new List<Node>();
    public List<Connection> Connections = new List<Connection>();
    public Node StartNode = null;
    public List<DialogParameter> Parameters = new List<DialogParameter>();
    

    public void SetParameters(List<ConectionParameters> param)
    {
        foreach (var item in param)
            Parameters[item.ID].Set(item);
    }
    public bool CheckParameters(List<ConectionParameters> param)
    {
        foreach (var item in param)
        {
            if (Parameters[item.ID].Check(item) == false)
                return false;
        }
        return true;
    }
    public string[] GetParameters()
    {
        string[] AllParameters = new string[Parameters.Count];

        for (int i = 0; i < AllParameters.Length; i++)
        {
            AllParameters[i] = Parameters[i].ParameterName;
        }
        return AllParameters;
    }

    public void SaveAsset()
    {
        try
        {
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }
        catch (System.Exception)
        {
        }
    }

    public void CreateConnection(Node From, Node To)
    {
        if (From.Connections.FindIndex(x => x.To == To) != -1)
        {
            Debug.LogError("Connection Already Exist");
            return;
        }

        //Undo.IncrementCurrentGroup();
        //Undo.RecordObject(this, "Make Transition");

        Connection asset;
        //Undo.RegisterCreatedObjectUndo(asset = ScriptableObject.CreateInstance<Connection>(), "Create");
        asset = ScriptableObject.CreateInstance<Connection>();
        asset.set(From, To);
        asset.name = $"Transition {From.name} - {To.name}";
        asset.hideFlags = HideFlags.HideInHierarchy;
        From.Connections.Add(asset);
        Connections.Add(asset);

        AssetDatabase.AddObjectToAsset(asset, AssetDatabase.GetAssetPath(this));

        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();
    }
    public void DeleteConnection(Connection connect)
    {
       //Undo.RecordObject(this, "Delete Connection");

        Connections.Remove(connect);

        //Undo.DestroyObjectImmediate(connect);
        AssetDatabase.RemoveObjectFromAsset(connect);
        DestroyImmediate(connect);

        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();
        UpdateAssets();
    }

    public Node CrerateNode(Vector2 position)
    {
       //Undo.RecordObject(this, "Create New Node");

        Node asset;
        //Undo.RegisterCreatedObjectUndo(asset = ScriptableObject.CreateInstance<Node>(), "Create");
        asset = ScriptableObject.CreateInstance<Node>();
        asset.SetPos(position);
        asset.name = "Node";
        asset.hideFlags = HideFlags.HideInHierarchy;

        AssetDatabase.AddObjectToAsset(asset, AssetDatabase.GetAssetPath(this));
        nodes.Add(asset);
        if (StartNode == null)
            SetStartNode(nodes[0]);


        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();

        return asset;
    }
    public void CrerateNode(Vector2 position, Node original)
    {
       //Undo.RecordObject(this, "Duplicate Node");

        Node asset;
        //Undo.RegisterCreatedObjectUndo(asset = ScriptableObject.Instantiate<Node>(original), "Duplicate");
        asset = ScriptableObject.Instantiate<Node>(original);
        asset.SetPos(position);
        asset.name = asset.GetInstanceID().ToString();
        for (int i = 0; i < asset.Connections.Count; i++)
        {
            Connection assetConnect = ScriptableObject.Instantiate<Connection>(asset.Connections[i]);
            assetConnect.From = asset;
            asset.Connections[i] = assetConnect;
            Connections.Add(assetConnect);
            AssetDatabase.AddObjectToAsset(assetConnect, AssetDatabase.GetAssetPath(this));
        }
        asset.hideFlags = HideFlags.HideInHierarchy;

        AssetDatabase.AddObjectToAsset(asset, AssetDatabase.GetAssetPath(this));
        nodes.Add(asset);
        if(StartNode == null)
            SetStartNode(nodes[0]);


        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();
    }
    public void DeleteNode(Node node)
    {
        //Undo.RecordObject(this, "Delete Node");
        //Undo.RegisterCompleteObjectUndo(this, "Delete Node");
        foreach (Connection item in Connections.FindAll(x => x.From == node || x.To == node))
        {
            //Undo.DestroyObjectImmediate(item);
            AssetDatabase.RemoveObjectFromAsset(item);
            DestroyImmediate(item);
        }

        nodes.Remove(node);
        if (node == StartNode)
        {
            if (nodes.Count > 0)
                StartNode = nodes[0];
            else
                StartNode = null;
        }

        //Undo.DestroyObjectImmediate(node);
        AssetDatabase.RemoveObjectFromAsset(node);
        DestroyImmediate(node);

        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();
        UpdateAssets();
    }
    public void SetStartNode(Node node)
    {
        StartNode = node;
        
        AssetDatabase.SaveAssets();
    }

    public void UpdateAssets()
    {
        Object[] data = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetOrScenePath(this));

        nodes.Clear();
        Connections.Clear();
        for (int i = 0; i < data.Length; i++)
        {
            if(data[i].GetType() == typeof(Node))
            {
                nodes.Add((Node)data[i]);
                continue;
            }
            if(data[i].GetType() == typeof(Connection))
            {
                Connections.Add((Connection)data[i]);
                continue;
            }
        }

        foreach (Node item in nodes)
            item.Connections.RemoveAll(element => element == null);

        foreach (Connection item in Connections)
           if(!item.From.Connections.Contains(item))
                item.From.Connections.Add(item);

    }
}



#if UNITY_EDITOR
[CustomEditor(typeof(Dialog))]
public class DialogEditor : Editor
{
    private void OnEnable()
    {
        Dialog t = (Dialog)target;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
    }
}
#endif
