using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
#endif
public class DialogPlayer : MonoBehaviour
{
    //---------IMPORTANT---------
    [Header("Gameobjects")]
    public GameObject AnswerPrefab;
    public GameObject PlayerPrefab;
    private GameObject CurPlayerPrefab;

    [Header("Transforms")]
    public Transform LeftPos; 
    public Transform RightPos; 
    public Transform AnswerArea;

    //[HideInInspector]
    public Dialog MyDialog;
    //[HideInInspector]
    public Node CurentNode;
    [HideInInspector]
    public DialogSettings.imgType ImgType;

    //---------IMPORTANT---------

    [Header("Text Animator Example")]
    public ExampleTextAnimator NameAnim;
    public ExampleTextAnimator TextAnim;

    public void StartDialog(Dialog dialog)
    {
        MyDialog = Instantiate(dialog);
        CurentNode = MyDialog.StartNode;
        NextState();
    }
    public void NextState()
    {
        if (CurPlayerPrefab)
            Destroy(CurPlayerPrefab);

        ClearAnswerArea();

        if (CurentNode && MyDialog)
        {
            //Set Parameters Values
            SetParametersValues();

            //Spawn Characters and Text
            SpawnCharacterAndText();

            //Check connections
            CheckNextConnections();
        }
        else
        {
            Debug.Log("END DIALOG");

            NameAnim.PlayAnimation("", 1);
            TextAnim.PlayAnimation("", 1);

            MyDialog = null;
        }
    }

    // Select Answer
    public void ChoseState(int Chose)
    {
        Chose = Mathf.Clamp(Chose, 0, CurentNode.Connections.Count - 1);
        CurentNode = CurentNode.Connections[Chose].To;

        NextState();
    }

    void SpawnCharacterAndText()
    {
        if (CurentNode.Character) //If Node Has Character
        {
            if (CurentNode.Position == Node.CharacterPos.Left)
            {
                CurPlayerPrefab = Instantiate(PlayerPrefab, LeftPos.position, LeftPos.rotation, LeftPos);
                CurPlayerPrefab.GetComponent<CharacterPrefab>().Spawn(CurentNode, ImgType);
            }
            else
            {
                CurPlayerPrefab = Instantiate(PlayerPrefab, RightPos.position, RightPos.rotation, RightPos);
                CurPlayerPrefab.GetComponent<CharacterPrefab>().Spawn(CurentNode, ImgType);

                CurPlayerPrefab.transform.localScale = new Vector3(-CurPlayerPrefab.transform.localScale.x, CurPlayerPrefab.transform.localScale.y, CurPlayerPrefab.transform.localScale.z);
            }

            NameAnim.PlayAnimation(CurentNode.Character.Name, CurentNode.Position, 1);
            TextAnim.PlayAnimation(CurentNode.text, CurentNode.textSpeed);
        }
        else // Talking to yourself 
        {
            NameAnim.PlayAnimation("", CurentNode.Position, 1);
            TextAnim.PlayAnimation(CurentNode.text, CurentNode.textSpeed);
        }
    }
    void SetParametersValues()
    {
        MyDialog.SetParameters(CurentNode.OutputParam);
    }
    void CheckNextConnections()
    {
        Node OldNode = CurentNode;
        if (CurentNode.Connections.Count > 0)
        {
            // Check Connections that have Parameters
            for (int i = 0; i < CurentNode.Connections.Count; i++)
            {
                if (CurentNode.Connections[i].InputParam.Count > 0 && MyDialog.CheckParameters(CurentNode.Connections[i].InputParam)) // // IF Connection have TRUE parameter
                {
                    if (CurentNode.Connections[i].SameParam.Length > 1 || CurentNode.Connections[i].Type == Connection.ConnectionType.Answer) // IF Connection have same parameters || Answer
                    {
                        foreach (int item in CurentNode.Connections[i].SameParam)
                        {
                            GameObject CurAnswerPrefab = Instantiate(AnswerPrefab, AnswerArea);

                            //CHANGABLE
                            CurAnswerPrefab.GetComponent<Button>().onClick.AddListener(delegate { ChoseState(item); });
                            CurAnswerPrefab.GetComponent<ExampleTextAnimator>().PlayAnimation(CurentNode.Connections[item].Answer, 1f);
                            //CHANGABLE

                            //Debug.Log(CurentNode.Connections[item].Answer);
                        }
                        return;
                    }
                    else
                    {
                        if(CurentNode.Connections[i].Type == Connection.ConnectionType.Time)
                            StartCoroutine(SetSkipDelay(CurentNode.Connections[i].Time));
                        CurentNode = CurentNode.Connections[i].To;
                        break;
                    }
                }
                else
                    continue;
            }

            // Check Connections that dont have Parameters
            for (int i = 0; i < CurentNode.Connections.Count; i++)
            {
                if (CurentNode.Connections[i].InputParam.Count == 0) // IF Connection dont have parameters
                {
                    if (CurentNode.Connections[i].SameParam.Length > 1 || CurentNode.Connections[i].Type == Connection.ConnectionType.Answer) // IF Connection have same parameters || Answer
                    {
                        foreach (int item in CurentNode.Connections[i].SameParam)
                        {
                            GameObject CurAnswerPrefab = Instantiate(AnswerPrefab, AnswerArea);

                            //CHANGABLE
                            CurAnswerPrefab.GetComponent<Button>().onClick.AddListener(delegate { ChoseState(item); });
                            CurAnswerPrefab.GetComponent<ExampleTextAnimator>().PlayAnimation(CurentNode.Connections[item].Answer, 1f);
                            //CHANGABLE

                            //Debug.Log(CurentNode.Connections[item].Answer);
                        }
                        return;
                    }
                    else
                    {
                        if (CurentNode.Connections[i].Type == Connection.ConnectionType.Time)
                            StartCoroutine(SetSkipDelay(CurentNode.Connections[i].Time));
                        CurentNode = CurentNode.Connections[i].To;
                        break;
                    }
                }
                else
                    continue;
            }

            //If Dont Have Next Node
            if(OldNode == CurentNode)
                CurentNode = null;
        }
        else
            CurentNode = null;
    }

    IEnumerator SetSkipDelay(float Delay)
    {
        yield return new WaitForSeconds(Delay);
        NextState();
    }
    void ClearAnswerArea()
    {
        foreach (Transform child in AnswerArea)
        {
            Destroy(child.gameObject);
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(DialogPlayer))]
public class DialogPlayerEditor : Editor
{
    DialogPlayer t;

    private void OnEnable()
    {
        DialogSettings.SetInstance();

        t = (DialogPlayer)target;
        t.ImgType = DialogSettings.Instance.ImgType;
    }
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        serializedObject.Update();


        if (serializedObject.hasModifiedProperties)
        {
            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif