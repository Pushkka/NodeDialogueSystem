using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class ConectionParameters
{
    public int ID = -1;
    public DialogParameter.inttype intType;
    public int intState = 0;
    public bool state;

    public ConectionParameters(int id)
    {
        ID = id;
    }
    public string GetIntType()
    {
        if (intType == DialogParameter.inttype.Equals)
            return "==";
        if (intType == DialogParameter.inttype.NotEquals)
            return "!=";
        if (intType == DialogParameter.inttype.Greater)
            return ">";
        if (intType == DialogParameter.inttype.Less)
            return "<";
        return "==";
    }
    public static bool EqualParameters(List<ConectionParameters> a1, List<ConectionParameters> a2)
    {
        if (a1 == null || a2 == null)
            return false;

        if (a1.Count != a2.Count)
            return false;

        for (int i = 0; i < a1.Count; i++)
        {
            if (a1[i].ID != a2[i].ID || a1[i].intState != a2[i].intState || a1[i].intType != a2[i].intType || a1[i].state != a2[i].state)
                return false;
        }
        return true;
    }
}
