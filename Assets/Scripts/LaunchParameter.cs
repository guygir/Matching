using UnityEngine;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public class LaunchParameter : MonoBehaviour
{
    // List of valid custom string values
    List<string> validStrings;

    // Start is called before the first frame update
    void StartProcess()
    {
        // Retrieve the value of the custom string parameter from the URL
        string customString = GetCustomString();
        if (!string.IsNullOrEmpty(customString))
        {
            if (IsCustomStringValid(customString))
            {
                Debug.Log("Custom string is valid: " + customString);
                FindObjectOfType<CardLoader>().oneSubjectGame = true;
                // Add your desired logic here when the custom string is valid
                FindObjectOfType<MenuManager>().StartGameOnSpecificSubject(customString);
            }
            else
            {
                Debug.Log("Custom string is invalid: " + customString);
                // Add your desired logic here when the custom string is invalid
                // Do nothing
            }
        }
        else
        {
            Debug.Log("No string");
        }
    }

    public void SetAllPossibleStrings(List<string> subjects)
    {
        validStrings = subjects;
        StartProcess();
    }

    // Function to retrieve the custom string from the URL
    private string GetCustomString()
    {

        string url = Application.absoluteURL;
        Debug.Log("URL IS: " + url);
        // Check if the URL contains the customString parameter
        if (url.Contains("?customString="))
        {
            // Extract the value of the customString parameter
            int startIndex = url.IndexOf("?customString=") + 14;
            int endIndex = url.IndexOf("&", startIndex);
            if (endIndex == -1)
            {
                endIndex = url.Length;
            }
            string customString = url.Substring(startIndex, endIndex - startIndex);
            Debug.Log(customString);
            // Return the customString value
            return customString;
        }

        return null;
    }

    // Function to check if the custom string is valid
    private bool IsCustomStringValid(string customString)
    {
        return validStrings.Contains(customString);
    }
}
