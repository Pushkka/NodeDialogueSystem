using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExampleDialogueStart : MonoBehaviour
{
    public Dialog DialogToStart;
    public DialogPlayer _DialogPlayer;

    public void StartMyDialog()
    {
        _DialogPlayer.StartDialog(DialogToStart);
    }
}
