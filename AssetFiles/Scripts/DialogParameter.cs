[System.Serializable]
public class DialogParameter
{
    public string ParameterName = "";
    public type Type;
    public enum type
    {
        Bool,
        Int
    }
    public enum inttype
    {
        Greater,
        Less,
        Equals,
        NotEquals
    }
    public int intState = 0;
    public bool State = false;

    public DialogParameter(string name)
    {
        ParameterName = name;
    }

    public void Set(ConectionParameters param)
    {
        if (Type == type.Bool)
            Set(param.state);
        else
            Set(param.intState);
    }
    void Set(bool val)
    {
        State = val;
    }
    void Set(int val)
    {
        intState = val;
    }

    public bool Check(ConectionParameters param)
    {
        if (Type == type.Bool)
        {
            if (State == param.state)
                return true;
            return false;
        }
        else
        {
            if (param.intType == inttype.Equals && param.intState == intState)
                return true;
            if (param.intType == inttype.NotEquals && param.intState != intState)
                return true;
            if (param.intType == inttype.Greater && param.intState > intState)
                return true;
            if (param.intType == inttype.Less && param.intState < intState)
                return true;
            return false;
        }
    }
}
