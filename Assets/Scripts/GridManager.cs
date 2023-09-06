using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using System.Linq;
using UnityEngine.Tilemaps;
using Unity.Mathematics;
using UnityEngine.UI;

public class GridManager : MonoBehaviour
{
    [Serializable]
    public struct CardData
    {
        public string Name;
        public string ImageURL;
        //public Sprite Image;
    }
    CardData[] allCardsData;
    
    [SerializeField] private Tile tilePrefab;
    [SerializeField] private Transform camera;
    [SerializeField] private float offset = 0.1f;
    [SerializeField] private float checkDelay = 0.3f;
    [SerializeField] private TMP_Text pointsText;
    [SerializeField] public TMP_Text gameText, menuText;
    [SerializeField] public GameObject finishText;

    public Sprite[] cards;
    private int subjectIndex = 0;
    private Dictionary<Vector2, Tile> tiles;
    private int width, height;
    private int howManyAreFlipped = 0;  //0-2.
    [HideInInspector]
    public bool isChecking = false;
    private Tile current1, current2;
    private int points = 0;

    //
    public GameObject rightCard, leftCard;
    private Image rightRenderer, leftRenderer;
    public Sprite cardBack;


    private void Start()
    {
        //subjectIndex= PlayerPrefs.GetInt("chosenValue");
        //StartCoroutine(LoadCards());    // with chosen
        CardLoader cardLoader = FindObjectOfType<CardLoader>();
        if (cardLoader.oneSubjectGame)
            menuText.gameObject.SetActive(false);

        List<Sprite> cardsList= cardLoader.ListAllSprites(cardLoader.GetCurrentSubject());
        int len = cardsList.Count;
        cards = new Sprite[len];
        for(int i = 0; i < len; i++)
        {
            cards[i] = cardsList[i];
        }

        Tuple<int, int> dims = FindBestSquareShape(cards.Length * 2);
        width = dims.Item2;
        height= dims.Item1;
        GenerateGrid();
        rightRenderer = rightCard.GetComponent<Image>();
        leftRenderer = leftCard.GetComponent<Image>();

    }


    IEnumerator LoadCards()
    {
        string url = "https://drive.google.com/uc?export=download&id=14zfwJDWAqhPhJ9R_wdc7ER12vW-iy8H4";
        //this from playerprefs
        UnityWebRequest request = UnityWebRequest.Get(url);
        request.chunkedTransfer = false;
        yield return request.Send();

        if (request.isNetworkError)
        {
            Debug.Log("error?");
        }
        else
        {
            if (request.isDone)
            {
                allCardsData = JsonHelper.GetArray<CardData>(request.downloadHandler.text);
                Debug.Log(allCardsData);
                cards = new Sprite[allCardsData.Length];
                StartCoroutine(GetCardsImages());
            }
            else
            {
                Debug.Log("?");
            }
        }
        /*
        Tuple<int, int> dims = FindBestSquareShape(cards.Length * 2);
        width = dims.Item2;
        height = dims.Item1;
        GenerateGrid();
        */
        
    }

    IEnumerator GetCardsImages()
    {
        for(int i=0;i<allCardsData.Length;i++) {
            
            /*
            WWW w = new WWW(allCardsData[i].ImageURL);
            yield return w;
            */

            UnityWebRequest w = UnityWebRequestTexture.GetTexture(allCardsData[i].ImageURL);
            yield return w.SendWebRequest();

            if (w.error != null)
            {
                Debug.Log("Couldnt load this card");
            }
            else
            {
                if (w.isDone)
                {
                    //Texture2D tx = w.texture;
                    Texture2D tx=((DownloadHandlerTexture)w.downloadHandler).texture;
                    //allCardsData[i].Image = Sprite.Create(tx, new Rect(0f, 0f, tx.width, tx.height), Vector2.zero);
                    CardLoader cardLoader = FindObjectOfType<CardLoader>();
                    //cards[i]= Sprite.Create(tx, new Rect(0f,0f, tx.width, tx.height), new Vector2(1 / 2f, 1 / 2f), 128);
                    cards[i]= Sprite.Create(tx, new Rect(0f,0f, tx.width, tx.height), new Vector2(1 / 2f, 1 / 2f), 128/(tx.width/128));

                }
            }
            

        }
        Tuple<int, int> dims = FindBestSquareShape(cards.Length * 2);
        width = dims.Item2;
        height = dims.Item1;
        GenerateGrid();
    }

    private Tuple<int, int> FindBestSquareShape(int n)
    {
        var factors = new Tuple<int, int>[0];
        for (int i = 1; i <= Math.Sqrt(n); i++)
        {
            if (n % i == 0)
            {
                Array.Resize(ref factors, factors.Length + 1);
                factors[factors.Length - 1] = Tuple.Create(i, n / i);
            }
        }

        int bestDiff = int.MaxValue;
        Tuple<int, int> bestShape = null;
        foreach (var shape in factors)
        {
            int diff = Math.Abs(shape.Item1 - shape.Item2);
            if (diff < bestDiff)
            {
                bestDiff = diff;
                bestShape = shape;
            }
        }

        return bestShape;
    }

    private List<int> GenerateDatabase(int x)
    {
        int[] randomNumbers = Enumerable.Range(0, cards.Length).OrderBy(x => Guid.NewGuid()).Take(x).ToArray();
        var database = new List<int>();
        for (int i = 0; i < x; i++)
        {
            database.Add(randomNumbers[i]);
            database.Add(randomNumbers[i]);
        }
        return database;
    }

    private int ChooseRandomItem(List<int> database)
    {
        var random = new System.Random();
        int index = random.Next(database.Count);
        int randomItem = database[index];
        database.RemoveAt(index);
        return randomItem;
    }

    private void Update()
    {
        //Debug.Log(howManyAreFlipped + "," + isChecking);
        if (current1 != null && rightRenderer.sprite == cardBack)
            rightRenderer.sprite = current1.GetCard();
        if (current1 == null && rightRenderer.sprite != cardBack)
            rightRenderer.sprite = cardBack;
        if (current2 != null && leftRenderer.sprite == cardBack)
            leftRenderer.sprite = current2.GetCard();
        if (current2 == null && leftRenderer.sprite != cardBack)
            leftRenderer.sprite = cardBack;
        if (howManyAreFlipped == 2&&!isChecking)
        {
            isChecking = true;
            StartCoroutine(CheckMatch());
        }
    }

    public IEnumerator CheckMatch()
    {
        yield return new WaitForSeconds(checkDelay);
        if (current1.GetCard() == current2.GetCard())
        {
            BoxCollider2D collider1 = current1.gameObject.GetComponent<BoxCollider2D>();
            BoxCollider2D collider2 = current2.gameObject.GetComponent<BoxCollider2D>();
            if (collider1 != null)
            {
                collider1.enabled = false;
            }
            if (collider2 != null)
            {
                collider2.enabled = false;
            }
            points++;
            if (points == width*height/2)
            {
                gameText.gameObject.SetActive(false);
                finishText.gameObject.SetActive(true);
                pointsText.gameObject.SetActive(false);
                FindObjectOfType<AudioManager>().Play("Win");
            }
            else
            {
                pointsText.text = points.ToString();
                FindObjectOfType<AudioManager>().Play("Match");
            }
            howManyAreFlipped = 0;
            current1 = null;
            current2 = null;
            isChecking = false;
        }
        else
        {
            current1.ReturnToBack();
            current2.ReturnToBack();
            FindObjectOfType<AudioManager>().Play("Wrong");
        }
        /*
        howManyAreFlipped = 0;
        current1 = null;
        current2 = null;
        isChecking = false;
        */
    }
    void GenerateGrid()
    {
        tiles = new Dictionary<Vector2, Tile>();
        List<int> cardIndices;
        if (width * height >= 12) // only 12 cards max
        {
            width = 4;
            height = 3;
            cardIndices = GenerateDatabase(6); //THIS IS RELEVENT TO 12 CARDS!

        }
        else {
            cardIndices = GenerateDatabase(cards.Length);
        }
        CardLoader cardLoader = FindObjectOfType<CardLoader>();
        float ratioF=1;
        for (int i=0;i<width;i++)
        {
            for (int j = 0; j < height; j++)
            {
                Sprite chosenCard = cards[ChooseRandomItem(cardIndices)];
                ratioF = cardLoader.HandleKnownRatio(chosenCard.rect.width);
                var spawnedTile = Instantiate(tilePrefab, new Vector3(i * (1* ratioF + offset), j * (1* ratioF + offset)), Quaternion.identity);
                spawnedTile.transform.GetChild(0).transform.localScale = new Vector3(ratioF, ratioF, 1);
                spawnedTile.GetComponent<BoxCollider2D>().size = new Vector2(ratioF, ratioF);
                spawnedTile.name = $"Tile {i} {j}";
                spawnedTile.SetCard(chosenCard);
                tiles[new Vector2(i,j)]= spawnedTile; 
            }
        }
        camera.transform.position = new Vector3((float)width * (1*ratioF + offset) / 2 - 0.5f* ratioF-0.05f, (float)height * (1* ratioF + offset) / 2 - 0.5f* ratioF, -10);
    }

    public Tile GetTileAtPosition(Vector2 pos)
    {
        if (tiles.TryGetValue(pos, out var tile))
        {
            return tile;
        }
        return null;
    }

    public int HowManyAreFlipped()
    {
        return howManyAreFlipped;
    }

    public void ResetFlips()
    {
        howManyAreFlipped = 0;
    }

    public void AddFlipCounter()
    {
        howManyAreFlipped++;
        Debug.Log("X");
    }

    public void SetCurrent1(Tile tile)
    {
        current1 = tile;
    }

    public void SetCurrent2(Tile tile)
    {
        current2 = tile;
    }

    public Tile GetCurrent1()
    {
        return current1;
    }

    public Tile GetCurrent2()
    {
        return current2;
    }

    public void BackToMainMenu()
    {
        SceneManager.LoadScene("MenuScene");
    }

    public void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

}
