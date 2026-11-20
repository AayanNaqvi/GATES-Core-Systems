using UnityEngine;

public static class TextManager
{
    public static bool textOnScreen;


    public static void FindText()
    {
        GameObject found = GameObject.FindWithTag("TextBox");
        textOnScreen = found != null;
    }



}
