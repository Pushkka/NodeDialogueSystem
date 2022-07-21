using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.ComponentModel;
using System;
using System.Runtime.Serialization;

[Serializable]
public class BodyParts<T>
{
    public string BodyName;
    public T Content;


    public BodyParts(string name, T content)
    {
        BodyName = name;
        Content = content;
    }

    public static List<BodyParts<T>> Sorting(List<BodyParts<T>> Original, T content)
    {
        DialogSettings.SetInstance();
        List<BodyParts<T>> Sortet = Original;
        //Remove Old
        for (int i = 0; i < Sortet.Count; i++)
        {
            if (!DialogSettings.Instance.PartNames.Contains(Sortet[i].BodyName))
            {
                Sortet.RemoveAt(i);
                i--;
            }
        }
        //Add new
        for (int i = 0; i < DialogSettings.Instance.PartNames.Count; i++)
        {
            if (Sortet.Find(x => x.BodyName.Contains(DialogSettings.Instance.PartNames[i])) == null)
                Sortet.Add(new BodyParts<T>(DialogSettings.Instance.PartNames[i], content));
        }
        //Sort
        for (int i = 0; i < Sortet.Count; i++)
        {
            int id = DialogSettings.Instance.PartNames.FindIndex(x => x == Sortet[i].BodyName);
            if (i != id)
            {
                BodyParts<T> temp = Sortet[i];
                Sortet.RemoveAt(i);
                Sortet.Insert(id, temp);
            }
        }

        return Sortet;
    }
}
