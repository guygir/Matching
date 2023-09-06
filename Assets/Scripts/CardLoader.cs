using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using UnityEngine.Networking;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using System.Linq;

public class CardLoader : MonoBehaviour
{
    private Dictionary<string, List<Sprite>> cardDatabase;
    public Dictionary<string, string> engToHebSubjects;
    private string currentSubject;
    private bool first = true;
    private bool loaded = false;

    public bool oneSubjectGame = false;
    private string zipURL = "https://rega1.co.il/wp-content/uploads/2023/06/cards.zip";
    //private string zipURL = "https://game0115.wpcomstaging.com/wp-content/uploads/2023/06/cards.zip";
    //private string zipURL = "https://www.mediafire.com/file/8kefbhzz1ojg4dx/cards.zip/file";
    //private string zipURL = "https://drive.google.com/uc?export=download&id=13khxU-DbLUpaW_lo-j7kdcZca-fhU5er";
    private string extractPath;
    public Sprite cardSizeExample;

    public static CardLoader instance;

    private void Awake()
    {
        //NOW REMOVED: HandleURL();
        if(instance==null)
            instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
    }
    //2023/06

    /*
    public void HandleURL()
    {
        string URL1="https://rega1.co.il/wp-content/uploads/";
        string URL2 = DateTime.Now.Year.ToString();
        string URL3 = "/";
        string URL4= DateTime.Now.Month.ToString();
        if (URL4.Length == 1)
            URL4 = "0" + URL4;
        string URL5= "/cards.zip";
        zipURL = URL1 + URL2 + URL3 + URL4 + URL5;
    }

    IEnumerator Start()
    {

        cardDatabase =new Dictionary<string, List<Sprite>>();
        yield return StartCoroutine(InitializeExtractPath());

        string cacheBuster = DateTime.Now.Ticks.ToString();
        string urlWithCacheBuster = zipURL + "?cache=" + cacheBuster;
        UnityWebRequest www = UnityWebRequest.Get(urlWithCacheBuster);
        yield return www.SendWebRequest();

        string zipPath = Path.Combine(Application.persistentDataPath, "cards.zip");
        System.IO.File.WriteAllBytes(zipPath, www.downloadHandler.data);

        CleanupExtractPath();
        ZipFile.ExtractToDirectory(zipPath, extractPath, true);
        LoadCards();
        System.IO.File.Delete(zipPath);

    }
    */

    public void HandleURL(DateTime date)
    {
        string URL1 = "https://rega1.co.il/wp-content/uploads/";
        string URL2 = date.Year.ToString();
        string URL3 = "/";
        string URL4 = date.Month.ToString("D2");
        string URL5 = "/GamesData.zip";
        zipURL = URL1 + URL2 + URL3 + URL4 + URL5;
    }

    IEnumerator Start()
    {
        cardDatabase = new Dictionary<string, List<Sprite>>();
        yield return StartCoroutine(InitializeExtractPath());

        DateTime currentDate = DateTime.Now;
        DateTime startDate = new DateTime(2023, 1, 1);

        while (currentDate >= startDate)
        {
            string zipPath = "";
            bool success = true;

            HandleURL(currentDate);

            string cacheBuster = DateTime.Now.Ticks.ToString();
            string urlWithCacheBuster = zipURL + "?cache=" + cacheBuster;
            UnityWebRequest www = UnityWebRequest.Get(urlWithCacheBuster);
            yield return www.SendWebRequest();

            try
            {
                zipPath = Path.Combine(Application.persistentDataPath, "GamesData_" + currentDate.ToString("MM-yyyy") + ".zip");
                System.IO.File.WriteAllBytes(zipPath, www.downloadHandler.data);

                CleanupExtractPath();
                ZipFile.ExtractToDirectory(zipPath, extractPath, true);
                LoadCards();
            }
            catch (Exception e)
            {
                success = false;
                //Debug.LogError("Error processing file for " + currentDate.ToString("MM-yyyy") + ": " + e.Message);
            }
            finally
            {
                if (!string.IsNullOrEmpty(zipPath))
                {
                    System.IO.File.Delete(zipPath);
                }
                currentDate = currentDate.AddMonths(-1);
            }

            yield return null; // Optional: yield to allow other processes to run between iterations
        }
    }
    private IEnumerator InitializeExtractPath()
    {
        yield return null; // Wait for a frame to ensure Start() has finished

        extractPath = Path.Combine(Application.persistentDataPath, "GamesData");
    }

    private void CleanupExtractPath()
    {
        if (Directory.Exists(extractPath))
        {
            Directory.Delete(extractPath, true);
        }
    }

    void LoadCards()
    {
        extractPath = Path.Combine(extractPath, "Matching");
        string cardsFolderPath = Path.Combine(extractPath, "cards");
        Debug.Log(cardsFolderPath);
        if (!Directory.Exists(cardsFolderPath))
        {
            Debug.LogError("Unable to find the 'cards' folder.");
            return;
        }

        // Get the subject folders inside the 'cards' folder
        string[] subjectFolders = Directory.GetDirectories(cardsFolderPath, "*");

        // Iterate over the subject folders
        foreach (string subjectFolder in subjectFolders)
        {
            //string subjectName = TranslateToUnicodeEscape(Path.GetFileName(subjectFolder));
            string subjectName = Path.GetFileName(subjectFolder);

            if (first)
            {
                currentSubject = subjectName;
                first = false;
            }
            Debug.Log(subjectName);
            // Get the card files in the subject folder
            //string[] cardFiles = Directory.GetFiles(subjectFolder, "*.png");
            string[] cardFiles = Directory.GetFiles(subjectFolder, "*.*", SearchOption.AllDirectories)
                             .Where(file => file.ToLower().EndsWith(".png") || file.ToLower().EndsWith(".jpg") || file.ToLower().EndsWith(".jpeg"))
                             .ToArray();
            List<Sprite> currentCards = new List<Sprite>();
            foreach (string cardFile in cardFiles)
            {
                // Load the PNG file into a Sprite
                Debug.Log(cardFile);
                Sprite cardSprite = LoadSpriteFromFile(cardFile);
                if (cardSprite != null)
                {
                    currentCards.Add(cardSprite);
                }
            }
            cardDatabase.Add(subjectName, currentCards);
        }
        //
        string txtPath = Path.Combine(extractPath, "Translate.txt");
        StartCoroutine(ParseTxtFile(txtPath));
        //
        FindObjectOfType<MenuManager>().LoadSubjects(engToHebSubjects);
        loaded = true;
    }

    private IEnumerator ParseTxtFile(string txtFilePath)
    {
        List<string> subjects = ListAllSubjects();
        engToHebSubjects = new Dictionary<string, string>();
        // Check if the file exists

        if (File.Exists(txtFilePath))
        {
            // Read all lines from the TXT file
            string[] lines = File.ReadAllLines(txtFilePath);

            // Iterate over each line in the file
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];

                // Split the line into columns (assuming columns are separated by commas)
                string[] columns = line.Split(',');

                // Ensure the line has at least two columns
                if (columns.Length >= 2)
                {
                    string leftColumnValue = columns[0];
                    string rightColumnValue = columns[1];

                    char[] charArray = rightColumnValue.ToCharArray();
                    Array.Reverse(charArray);
                    string reversedString = new string(charArray);

                    // Check if the left column value exists in the ABC list
                    if (subjects.Contains(leftColumnValue))
                    {
                        engToHebSubjects.Add(leftColumnValue, reversedString);
                    }
                }
            }
        }
        else
        {
            Debug.Log("TXT file does not exist: " + txtFilePath);
        }

        yield return null;
    }

    /*
    public string TranslateToUnicodeEscape(string hebrewWord)
    {
        StringBuilder sb = new StringBuilder();

        foreach (char c in hebrewWord)
        {
            sb.Append("\\u");
            sb.Append(((int)c).ToString("X4"));
        }

        return sb.ToString();
    }

    public string TranslateFromUnicodeEscape(string unicodeEscapedWord)
    {
        StringBuilder sb = new StringBuilder();

        int currentIndex = 0;
        while (currentIndex < unicodeEscapedWord.Length)
        {
            if (unicodeEscapedWord[currentIndex] == '\\' && unicodeEscapedWord[currentIndex + 1] == 'u')
            {
                string unicodeValue = unicodeEscapedWord.Substring(currentIndex + 2, 4);
                int characterCode = int.Parse(unicodeValue, System.Globalization.NumberStyles.HexNumber);
                sb.Append((char)characterCode);
                currentIndex += 6;
            }
            else
            {
                sb.Append(unicodeEscapedWord[currentIndex]);
                currentIndex++;
            }
        }

        return sb.ToString();
    }
    */

    public bool GetLoaded()
    {
        return loaded;
    }

    Sprite LoadSpriteFromFile(string filePath)
    {
        // Load the PNG file as a Texture2D
        byte[] fileData = System.IO.File.ReadAllBytes(filePath);
        Texture2D texture = new Texture2D(2, 2); // Create a new texture
        texture.LoadImage(fileData); // Load the image data
        texture.filterMode = FilterMode.Point; // Optional: Set the filter mode to point (pixelated)
        //WE ASSUME CARD IS SQUARE
        float f = HandleRatio(texture);
        // Create a Sprite from the Texture2D
        Sprite sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(1 / 2f, 1 / 2f), 128* HandleRatio(texture));

        return sprite;
    }

    public float HandleRatio(Texture2D texture)
    {
        return texture.width / cardSizeExample.textureRect.width;
        //return 1;
    }

    public float HandleKnownRatio(float f)
    {
        return f / cardSizeExample.textureRect.width;
    }

    public List<string> ListAllSubjects()
    {
        List<string> subjects = new List<string>();
        foreach (string key in cardDatabase.Keys)
        {
            // Access the key and perform desired operations
            subjects.Add(key);
        }
        return subjects;
    }

    public List<Sprite> ListAllSprites(string subject)
    {
        List<Sprite> cards = new List<Sprite>();
        if (cardDatabase.ContainsKey(subject))
        {
            List<Sprite> sprites = cardDatabase[subject];

            // Iterate over the sprites in the list
            foreach (Sprite sprite in sprites)
            {
                cards.Add(sprite);
            }
        }
        else
        {
            // Handle the case where the key is not found in the dictionary
            Debug.Log("Key not found: " + subject);
        }
        return cards;
    }

    public void ChangeCurrentSubject(string name)
    {
        currentSubject = name;
    }

    public string GetCurrentSubject()
    {
        return currentSubject;
    }
}
