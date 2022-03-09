using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SubWeaponUI : MonoBehaviour
{
    /// <summary>
    /// アイコン
    /// </summary>
    public Image Icon
    {
        get
        {
            if (icon == null)
            {
                icon = transform.GetChild(0).GetComponent<Image>();
            }
            return icon;
        }
    }
    private Image icon = null;

    /// <summary>
    /// テキスト
    /// </summary>
    public Text Text
    {
        get
        {
            if (text == null)
            {
                text = transform.GetChild(1).GetComponent<Text>();
            }
            return text;
        }
    }
    private Text text = null;
}
