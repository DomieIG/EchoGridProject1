using UnityEngine;
using TMPro;
using System.Collections;

public class TerminalComputer : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_InputField passwordField;

    [Header("Feedback Texts")]
    public TextMeshProUGUI accessGrantedText;
    public TextMeshProUGUI accessDeniedText;

    [Header("Panels")]
    public GameObject loginPanel;
    public GameObject dashboardPanel;

    [Header("Password Settings")]
    public string correctPassword = "111";
    public float successDelay = 1.0f;   // Time before switching to dashboard
    public float deniedDelay = 1.0f;    // Time Access Denied stays visible

    public BadgePrinter badgePrinter_mesh;

    private string currentInput = "";

    public void DebugPrintButton()
    {
        Debug.Log("PRINT BUTTON CLICKED!");
    }

    void Start()
    {
        passwordField.text = "";

        accessGrantedText?.gameObject.SetActive(false);
        accessDeniedText?.gameObject.SetActive(false);

        loginPanel?.SetActive(true);
        dashboardPanel?.SetActive(false);
    }

    public void ReceiveKeyPress(string key)
    {
        key = key.Trim();

        switch (key)
        {
            case "ENTER":
                SubmitPassword();
                return;

            case "BACK":
                if (currentInput.Length > 0)
                    currentInput = currentInput.Substring(0, currentInput.Length - 1);
                UpdateField();
                return;

            default:
                currentInput += key;
                UpdateField();
                return;
        }
    }
    public void PrintBadge()
    {
        badgePrinter_mesh?.StartPrinting();
    }


    private void UpdateField()
    {
        passwordField.text = currentInput;
    }

    private void SubmitPassword()
    {
        accessGrantedText.gameObject.SetActive(false);
        accessDeniedText.gameObject.SetActive(false);

        if (currentInput == correctPassword)
        {
            accessGrantedText.gameObject.SetActive(true);
            StartCoroutine(SuccessSequence());
        }
        else
        {
            StartCoroutine(AccessDeniedSequence());
        }
    }

    private IEnumerator AccessDeniedSequence()
    {
        accessDeniedText.gameObject.SetActive(true);

        yield return new WaitForSeconds(deniedDelay);

        accessDeniedText.gameObject.SetActive(false);

        currentInput = "";
        UpdateField();
    }

    private IEnumerator SuccessSequence()
    {
        yield return new WaitForSeconds(successDelay);

        dashboardPanel?.SetActive(true);
        loginPanel?.SetActive(false);
    }
}
