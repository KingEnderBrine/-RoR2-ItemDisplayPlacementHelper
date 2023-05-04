using TMPro;
using UnityEngine;

public class InvariatCultureDecimalValidator : MonoBehaviour
{
    private void Awake()
    {
        GetComponent<TMP_InputField>().onValidateInput = Validate;
    }

    private char Validate(string text, int pos, char ch)
    {
        if (pos != 0 || text.Length <= 0 || (text[0] != '-'))
        {
            if (ch >= '0' && ch <= '9' || ch == '-' && pos == 0 || ch == '.' && !text.Contains("."))
            {
                return ch;
            }
        }
        return char.MinValue;
    }
}
