using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ExampleTextAnimator : MonoBehaviour
{
    public TMP_Text TextField;

    public void PlayAnimation(string _text,float _delay)
    {
        StopAllCoroutines();

        StartCoroutine(Animation(_text, Mathf.Lerp(0.25f, 0.01f, _delay)));
    }
    public void PlayAnimation(string _text, Node.CharacterPos _pos, float _delay)
    {
        StopAllCoroutines();

        if (_pos == Node.CharacterPos.Left)
            TextField.alignment = TextAlignmentOptions.Left;
        else
            TextField.alignment = TextAlignmentOptions.Right;

        StartCoroutine(Animation(_text, Mathf.Lerp(0.25f, 0.01f, _delay)));
    }

    private IEnumerator Animation(string text, float delay)
    {
        string TextAnim = "";
        if (text == "")
            TextField.text = text;

        if (delay == 0)
            TextField.text = text;
        else
            for (int i = 0; i < text.Length; i++)
            {
                TextAnim += text[i];
                TextField.text = TextAnim;
                yield return new WaitForSeconds(delay);
            }
    }
}
