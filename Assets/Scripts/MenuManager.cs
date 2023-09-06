using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using TMPro;
using Unity.VisualScripting;
using UnityEngine.SceneManagement;
using System;

public class MenuManager : MonoBehaviour
{
    public TMP_Dropdown dropdown;
    CardLoader cardLoader;
    [SerializeField] Button startGame;
    private List<string> mixedSubjects;
    Dictionary<string, string> dictOfSubjects;
    List<string> gameSubjects;
    void Start()
    {
        cardLoader = FindObjectOfType<CardLoader>();
        if (cardLoader.GetLoaded())
        {
            LoadSubjects(cardLoader.engToHebSubjects);
            cardLoader.ChangeCurrentSubject(cardLoader.ListAllSubjects()[0]);
        }
        //dropdown.onValueChanged.AddListener(OnDropdownValueChanged);
        /*
        string filePath = Path.Combine(Application.dataPath, "MatchingGameData.csv");
        string[] lines = File.ReadAllLines(filePath);
        string[] values = lines[0].Split(',');
        List<string> options = new List<string>();
        foreach (string value in values)
        {
            options.Add(value);
        }
        dropdown.ClearOptions();
        dropdown.AddOptions(options);
        PlayerPrefs.SetInt("chosenValue", 0);
        */
    }

    public void LoadSubjects(Dictionary<string,string> dict)
    {
        dictOfSubjects = dict;
        CardLoader cardLoader=FindObjectOfType<CardLoader>();
        List<string> subjects=cardLoader.ListAllSubjects();
        gameSubjects = subjects;
        FindObjectOfType<LaunchParameter>().SetAllPossibleStrings(gameSubjects);
        dropdown.ClearOptions();
        /*
        foreach (string subject in subjects)
        {
            TMP_Dropdown.OptionData newOption = new TMP_Dropdown.OptionData(cardLoader.TranslateToUnicodeEscape(subject));
            dropdown.options.Add(newOption);
            dropdown.RefreshShownValue();
        }
        */
        SetDropDown(gameSubjects, dictOfSubjects);
        dropdown.AddOptions(mixedSubjects);
        dropdown.onValueChanged.AddListener(OnDropdownValueChanged);
        startGame.interactable=true;
    }

    public void SetDropDown(List<string> subjects, Dictionary<string,string> dict)
    {
        mixedSubjects = new List<string>();
        for (int i = 0; i < subjects.Count; i++)
        {
            if (dict.ContainsKey(subjects[i]))
            {
                mixedSubjects.Add(dict[subjects[i]]);
            }
            else
            {
                mixedSubjects.Add(subjects[i]);
            }
        }
    }

    public void StartGameOnSpecificSubject(string subject)
    {
        cardLoader.ChangeCurrentSubject(subject);
        startGame.onClick.Invoke();
    }

    public void LoadGame()
    {
        SceneManager.LoadScene("GameScene");
    }

    private void OnDropdownValueChanged(int value)
    {
        //PlayerPrefs.SetInt("chosenValue", value);
        //cardLoader.ChangeCurrentSubject(cardLoader.TranslateToUnicodeEscape(dropdown.options[value].text));
        //cardLoader.ChangeCurrentSubject(dropdown.options[value].text);
        cardLoader.ChangeCurrentSubject(gameSubjects[value]);
    }
}
