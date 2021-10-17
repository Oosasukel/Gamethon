using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using TMPro;
using System.Globalization;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public Action OnFinishSetupShares;
    public Action OnChangeWeek;
    public Action OnChangeMonth;
    public Action OnGameOver;

    private double pocketCash = 100;
    public double PocketCash
    {
        get => pocketCash; set
        {
            pocketCash = value;
            pocketCashText.text = $"R$ {pocketCash.ToString("N2")}";
        }
    }

    private double allCash = 100;
    public double AllCash
    {
        get => allCash; set
        {
            allCash = value;
            allCashText.text = $"R$ {allCash.ToString("N2")}";
        }
    }

    public double savingsCash = 0;

    public bool gameOver = false;

    public float monthTimeInSeconds = 5.0f;
    public TextMeshProUGUI pocketCashText;
    public GameObject gameOverUi;
    public TextMeshProUGUI totalProfitText;
    public TextMeshProUGUI allCashText;
    public TextMeshProUGUI yearText;
    public Slider slider;
    public int sharesCount = 5;

    public List<Share> shares;

    public int currentMonth = 0;
    public int currentYear = 0;
    public int currentWeek = 0;

    void Start()
    {
        slider.minValue = 0;
        slider.maxValue = 11;

        gameOverUi.SetActive(false);

        shares = Utils.GetShares(sharesCount);

        if (OnFinishSetupShares != null)
        {
            OnFinishSetupShares();
        }

        pocketCashText.text = $"R$ {pocketCash.ToString("N2")}";
        allCashText.text = $"R$ {allCash.ToString("N2")}";

        StartCoroutine("MonthCounting");
        StartCoroutine("WeekCounting");
    }

    private void UpdateYearProgress()
    {
        if ((currentMonth) % 12 == 0)
        {
            yearText.text = $"Ano: {(currentMonth / 12) + 1}";
        }

        slider.value = currentMonth % 12;
    }

    IEnumerator MonthCounting()
    {
        yield return new WaitForSeconds(monthTimeInSeconds);

        currentMonth++;

        gameOver = (currentMonth / 12) == 10;
        if (!gameOver)
        {
            PocketCash += 100;
            UpdateYearProgress();

            if (OnChangeMonth != null)
            {
                OnChangeMonth();
            }

            updateAllCash();

            StartCoroutine("MonthCounting");
        }
        else
        {
            gameOverUi.SetActive(true);
            totalProfitText.text = $"Lucro total: R$ {(AllCash - 12000).ToString("N2")}";

            if (OnGameOver != null)
            {
                OnGameOver();
            }
        }
    }

    IEnumerator WeekCounting()
    {
        var weekTimeInSeconds = (monthTimeInSeconds / 30) * 7;
        yield return new WaitForSeconds(weekTimeInSeconds);

        if (!gameOver)
        {
            currentWeek++;

            if (OnChangeWeek != null)
            {
                OnChangeWeek();
            }

            updateAllCash();

            StartCoroutine("WeekCounting");
        }
    }

    private void updateAllCash()
    {

        double all = PocketCash + savingsCash;
        foreach (var share in shares)
        {
            all += share.Values[currentWeek].Value * share.BelongToThePlayerCount;
        }
        AllCash = all;
    }
}

public class Share
{
    public int Id { get; set; }
    public string Name { get; set; }
    public List<ShareValue> Values { get; set; }
    public int BelongToThePlayerCount { get; set; }
}

public class ShareValue
{
    public string Date { get; set; }
    public float Value { get; set; }
    public decimal Variation { get; set; }
}

public class Utils
{
    private static Share GetShareWithValues(string file)
    {
        var stock = new Share();
        stock.Values = new List<ShareValue>();
        stock.BelongToThePlayerCount = 0;

        StreamReader strReader = new StreamReader(file);

        bool endOfLine = false;
        for (int i = 0; !endOfLine; i++)
        {
            string dataString = strReader.ReadLine();

            if (i == 0) continue;

            if (dataString == null)
            {
                endOfLine = true;
                break;
            }

            var dataValues = dataString.Split(',');

            var date = dataValues[0].ToString();
            var value = dataValues[1].ToString();
            var variation = dataValues[2].ToString();

            var newShareValue = new ShareValue()
            {
                Date = date,
                Value = float.Parse(value) / 100,
                Variation = Convert.ToDecimal(variation, new CultureInfo("en-US"))
            };
            stock.Values.Add(newShareValue);
        }

        return stock;
    }
    public static List<Share> GetShares(int count)
    {
        var shares = new List<Share>();

        var files = Directory.GetFiles(Path.Combine(Application.streamingAssetsPath, "data/shares"), "*.csv");

        Shuffle(files);

        for (int i = 0; i < count; i++)
        {
            var file = files[i];

            var newShares = GetShareWithValues(file);
            newShares.Id = i + 1;
            newShares.Name = Path.GetFileName(file).Replace(".csv", "");

            shares.Add(newShares);
        }

        return shares;
    }

    private static void Shuffle<T>(T[] list)
    {
        int n = list.Length;
        while (n > 1)
        {
            n--;
            int k = UnityEngine.Random.Range(0, n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
}