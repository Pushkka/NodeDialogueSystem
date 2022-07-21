using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
#endif

public class CustomGUI
{
    public static void GuiLine(int i_height = 1)
    {
        Rect rect = EditorGUILayout.GetControlRect(false, i_height);
        rect.height = i_height;
        EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 1));
    }
    public static void GuiLine(float posx, float posy, int i_height = 1)
    {
        Rect rect = new Rect(posx, posy, EditorGUIUtility.currentViewWidth - 14 - posx, i_height);
        EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 1));
    }


    public static void DrawBox(float Xpos, float Ypos, string text)
    {
        GUIStyle myStyle = new GUIStyle();
        Vector2 size;

        size = myStyle.CalcSize(new GUIContent(text));

        GUI.Box(new Rect(Xpos - size.x / 2 -10, Ypos, size.x + 20, size.y + 6), text);

    }
    public static void DrawBox(float Xpos, float Ypos, string text, Color col)
    {
        GUIStyle myStyle = new GUIStyle();
        Vector2 size;

        size = myStyle.CalcSize(new GUIContent(text));

        Color lastCol = GUI.backgroundColor;
        col.a = 1;
        GUI.backgroundColor = col;

        GUI.Box(new Rect(Xpos - size.x / 2 -10, Ypos, size.x + 20, size.y + 6), text);

        GUI.backgroundColor = lastCol;
    }


    public static void DrawBoxLayout(string mess, int Size)
    {
        GUILayout.Box(mess, GUILayout.Width(Size), GUILayout.Height(Size));
    }
    public static void DrawBoxLayout(Sprite image, int Size)
    {
        if (image == null)
        {
            GUILayout.Box("Null", GUILayout.Width(Size), GUILayout.Height(Size));
            return;
        }
        Texture2D croppedTexture = image.texture;
        if (image.texture.isReadable)
        {
            croppedTexture = new Texture2D((int)image.rect.width, (int)image.rect.height);
            var pixels = image.texture.GetPixels((int)image.textureRect.x,
                                                    (int)image.textureRect.y,
                                                    (int)image.rect.width,
                                                    (int)image.rect.height);
            croppedTexture.SetPixels(pixels);
            croppedTexture.Apply();
        }
        GUILayout.Box(croppedTexture, GUILayout.Width(Size), GUILayout.Height(Size));
    }


    public static Texture2D GetTexture(Sprite image)
    {
        Texture2D croppedTexture = image.texture;
        if (image.texture.isReadable)
        {
            croppedTexture = new Texture2D((int)image.rect.width, (int)image.rect.height);
            var pixels = image.texture.GetPixels((int)image.textureRect.x,
                                                    (int)image.textureRect.y,
                                                    (int)image.rect.width,
                                                    (int)image.rect.height);
            croppedTexture.SetPixels(pixels);
            croppedTexture.Apply();
        }
        return croppedTexture;
    }


    public static void DrawBorder(Rect rect, Color col, int Size)
    {
        EditorGUI.DrawRect(new Rect(rect.x - Size, rect.y - Size, rect.width + Size + Size, rect.height + Size + Size), col);
        //EditorGUI.DrawRect(new Rect(rect.x - Size, rect.y - Size, Size, rect.height + Size + Size), col);
        //EditorGUI.DrawRect(new Rect(rect.x, rect.y - Size, rect.width + Size, Size), col);
        //EditorGUI.DrawRect(new Rect(rect.x, rect.y + rect.height, rect.width + Size, Size), col);
        //EditorGUI.DrawRect(new Rect(rect.x + rect.width, rect.y, Size, rect.height), col);
    }


    public static void DrawLine(Vector3 endPos, Vector3 startPos)
    {
        Vector3 Dir = endPos - startPos;
        Vector3 startTan = startPos + Dir * 0.5f;
        Vector3 endTan = endPos - Dir * 0.5f;
        Color shadowCol = new Color(1, 1, 1, 0.3f);

        for (int i = 0; i < 6; i++) // Draw a shadow
            Handles.DrawBezier(startPos, endPos, startTan, endTan, shadowCol, null, (i + 1) * 1);
        Handles.DrawBezier(startPos, endPos, startTan, endTan, Color.white, null, 3f);
    } 
    public static void DrawLine(Vector3 endPos, Vector3 startPos, Color Col)
    {
        Vector3 Dir = endPos - startPos;
        Vector3 startTan = startPos + Dir * 0.5f;
        Vector3 endTan = endPos - Dir * 0.5f;
        Color shadowCol = Col * 0.2f;

        for (int i = 0; i < 6; i++) // Draw a shadow
            Handles.DrawBezier(startPos, endPos, startTan, endTan, shadowCol, null, (i + 1) * 1);
        Handles.DrawBezier(startPos, endPos, startTan, endTan, Col, null, 4f);
    }
    public static void DrawArrow(Vector3 startPos, Vector3 endPos, Color Col)
    {
        startPos -= Vector3.forward * 50;
        endPos -= Vector3.forward * 40;
        Vector3 Dir = endPos - startPos;
        Vector3 startTan = startPos + Dir * 0.5f;
        Vector3 endTan = endPos - Dir * 0.5f;
        Color shadowCol = Col * 0.2f;

        Handles.BeginGUI();
        for (int i = 0; i < 6; i++) // Draw a shadow
            Handles.DrawBezier(startPos, endPos, startTan, endTan, shadowCol, null, (i + 1) * 1);
        Handles.DrawBezier(startPos, endPos, startTan, endTan, Col, null, 4f);
        Handles.color = Col;
        Handles.DrawSolidArc(startPos + Dir / 1.9f, Vector3.forward, -Dir, -25, 20);
        Handles.DrawSolidArc(startPos + Dir / 1.9f, Vector3.forward, -Dir, 25, 20);
        Handles.EndGUI();
    }
}
