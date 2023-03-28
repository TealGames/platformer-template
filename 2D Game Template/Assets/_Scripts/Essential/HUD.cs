using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Manages the appearance of the player's heads up display and changes images, values and appearances based on the actions of the player and their inventory
/// </summary>

public class HUD : MonoBehaviour
{
    public static HUD Instance;

    [SerializeField][Tooltip("Text displayed in the upper half of the screen, usually displaying level names and sections")] private TextMeshProUGUI headingText;
    [SerializeField] [Tooltip("Large sized buttom screen text")] private TextMeshProUGUI largeText;
    [SerializeField] [Tooltip("Medium sized buttom screen text")] private TextMeshProUGUI mediumText;
    [SerializeField] [Tooltip("Small sized buttom screen text")] private TextMeshProUGUI smallText;

    

    public enum TextType
    {
        None,
        Small,
        Medium,
        Large,
        Title
    }

    [SerializeField] private TextMeshProUGUI coinAmountText;

    [SerializeField] private Weapon weaponScript;

    [SerializeField] private GameObject[] health;
    private int currentHealthIcons;
    private int maxHealthIcons;

    // Start is called before the first frame update
    void Awake()
    {
        if (Instance == null) Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);

        
    }

    private void Start()
    {
        PlayerCharacter.Instance.OnPlayerHealthChanged += ChangeHealthIcons;
        PlayerCharacter.Instance.OnPlayerCurrencyChanged += ChangeCurrencyText;

        //getting player's max health (since current player's health is set to max health at start)
        //and making that amount of health icons visible
        maxHealthIcons = PlayerCharacter.Instance.GetHealth();
        for (int i = 0; i < health.Length; i++)
        {
            if (i < maxHealthIcons) health[i].SetActive(true);
            else health[i].SetActive(false);
        }
        coinAmountText.text = PlayerCharacter.Instance.GetCurrency().ToString();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void ChangeHealthIcons(int newHealth)
    {
        //loops through each health icon sets the current amount of health icons
        //by finding the last enabled health image component
        for (int i=0; i<maxHealthIcons; i++)
        {
            if (health[i].GetComponent<Image>().enabled)
            {
                if (i == maxHealthIcons - 1) currentHealthIcons = maxHealthIcons;
                else continue;
            }
            else
            {
                currentHealthIcons = i;
                break;
            }
        }
        

        if (currentHealthIcons!= newHealth)
        {
            //take damage
            if (currentHealthIcons > newHealth)
            {
                for (int i = currentHealthIcons - 1; i > newHealth-1; i--)
                {
                    health[i].GetComponent<Animator>().SetTrigger("healthDamage_trig");
                    UnityEngine.Debug.Log($"{i} health is damaged!");
                }
            }

            //heal
            else if (currentHealthIcons<newHealth)
            {
                for (int i= currentHealthIcons; i< newHealth; i++ )
                {
                    health[i].GetComponent<Animator>().SetTrigger("healthHeal_trig");
                    UnityEngine.Debug.Log($"{i} health is healed!");
                }
            }
        }
    }

    public void ChangeCurrencyText(int newCurrencyAmount)
    {
        coinAmountText.text = newCurrencyAmount.ToString();
    }

    public void EnableText(TextType textType, string text)
    {
        UnityEngine.Debug.Log("Enable text called!");
        TextMeshProUGUI textToEnable;
        switch (textType)
        {
            case (TextType.Small):
                textToEnable = smallText; break;
            case (TextType.Medium):
                textToEnable = mediumText; break;
            case (TextType.Large):
                textToEnable = largeText; break;
            case (TextType.Title):
                textToEnable = headingText; break;
            default: textToEnable = null; break;
        }

        if (textToEnable==null)
        {
            UnityEngine.Debug.LogWarning($"The argument {textType.ToString()} does not match any of the text type switch cases!");
            return;
        }
        textToEnable.text = text;
        textToEnable.gameObject.SetActive(true);
    }

    public void DisableText(TextType textType)
    {
        TextMeshProUGUI textToEnable;
        switch (textType)
        {
            case (TextType.Small):
                textToEnable = smallText; break;
            case (TextType.Medium):
                textToEnable = mediumText; break;
            case (TextType.Large):
                textToEnable = largeText; break;
            case (TextType.Title):
                textToEnable = headingText; break;
            default: textToEnable = null; break;
        }

        if (textToEnable == null)
        {
            UnityEngine.Debug.LogWarning($"The argument {textType.ToString()} does not match any of the text type switch cases!");
            return;
        }
        textToEnable.gameObject.SetActive(false);
        textToEnable.text = "";
        //sets color gradient to default white
        textToEnable.colorGradientPreset = TMP_ColorGradient.CreateInstance<TMP_ColorGradient>();
    }

    public TextMeshProUGUI ReturnText(TextType textType)
    {
        switch (textType)
        {
            case (TextType.Small):
                return smallText;
            case (TextType.Medium):
                return mediumText;
            case (TextType.Large):
                return largeText;
            case (TextType.Title):
                return headingText;
            default: return null;
        }
    }
}