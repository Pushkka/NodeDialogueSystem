using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
#endif

public class CharacterPrefab : MonoBehaviour
{
    public List<BodyParts<BodyImg>> BodyParts = new List<BodyParts<BodyImg>>();

    [System.Serializable]
    public class BodyImg
    {
        public Image ImgSprite = null;
        public Transform ImgPref = null;
    }
    public void Spawn(Node Node, DialogSettings.imgType type)
    {
        for (int i = 0; i < Node.Character.BodyParts.Count; i++)
        {
            if (Node.BodyParts[i].Content == -1)
                continue;

            if(type == DialogSettings.imgType.Sprite)
            {
                BodyParts[i].Content.ImgSprite.sprite = Node.Character.BodyParts[i].Content[Node.BodyParts[i].Content].ImgSprite;
            }
            else
            {
                GameObject character = Instantiate(Node.Character.BodyParts[i].Content[Node.BodyParts[i].Content].ImgPref, BodyParts[i].Content.ImgPref.transform.position, 
                    BodyParts[i].Content.ImgPref.transform.rotation, BodyParts[i].Content.ImgPref);
            }
        }
    }
    private void Reset()
    {
        BodyParts = BodyParts<BodyImg>.Sorting(BodyParts, null);
    }
}



#if UNITY_EDITOR
[CustomEditor(typeof(CharacterPrefab))]
public class CharacterPrefabEditor : Editor
{
    CharacterPrefab t;

    public SerializedProperty BodyParts;
    public SerializedProperty Content;

    public void OnEnable()
    {
        t = (CharacterPrefab)target;
        t.BodyParts = BodyParts<CharacterPrefab.BodyImg>.Sorting(t.BodyParts, null);

        BodyParts = serializedObject.FindProperty("BodyParts");
    }


    public override void OnInspectorGUI()
    {
        //DrawDefaultInspector();
        serializedObject.Update();

        for (int i = 0; i < t.BodyParts.Count; i++)
        {
            if (t.BodyParts[i].Content == null)
                t.BodyParts[i].Content = new CharacterPrefab.BodyImg();

            SerializedProperty Content = BodyParts.GetArrayElementAtIndex(i).FindPropertyRelative("Content");
            SerializedProperty ImgSprite = Content.FindPropertyRelative("ImgSprite");
            SerializedProperty ImgPref = Content.FindPropertyRelative("ImgPref");

            GUILayout.BeginHorizontal();
            GUILayout.Label(t.BodyParts[i].BodyName, GUILayout.Width(100));
            if(DialogSettings.Instance.ImgType == DialogSettings.imgType.Sprite)
                ImgSprite.objectReferenceValue = (Image)EditorGUILayout.ObjectField(ImgSprite.objectReferenceValue, typeof(Image), true);
            if(DialogSettings.Instance.ImgType == DialogSettings.imgType.Prefab)
                ImgPref.objectReferenceValue = (Transform)EditorGUILayout.ObjectField(ImgPref.objectReferenceValue, typeof(Transform), true);
            GUILayout.EndHorizontal();
            GUILayout.Space(5);
            CustomGUI.GuiLine(2);
            GUILayout.Space(5);
        }


        if (GUILayout.Button("Reset"))
        {
            t.BodyParts.Clear();
            t.BodyParts = BodyParts<CharacterPrefab.BodyImg>.Sorting(t.BodyParts, null);
        }

        if (serializedObject.hasModifiedProperties)
        {
            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif
