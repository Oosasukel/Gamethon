using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Linq;

public class ShareManager : MonoBehaviour, IInteractable
{
    private GameManager gameManager;
    private Share share;
    public TextMeshProUGUI nameUi;
    public TextMeshProUGUI currentValueUi;
    public TextMeshProUGUI amountUi;
    public AudioSource audioSource;
    public TextMeshProUGUI cashAmountUi;
    public TextMeshProUGUI variationUi;
    public TextMeshProUGUI profitUi;
    public double investedCash = 0;
    public GameObject tips;
    public int buySellCount = 10;

    public RectTransform graphContainer;
    public Sprite circleSprite;

    public int graphMaxPoints = 50;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void Start()
    {
        HideTips();

        gameManager = FindObjectOfType<GameManager>();

        gameManager.OnFinishSetupShares += setupShare;
        gameManager.OnChangeWeek += handleWeekChange;
    }

    private GameObject CreateCircle(Vector2 anchoredPosition)
    {
        GameObject gameObject = new GameObject("circle", typeof(Image));
        gameObject.transform.SetParent(graphContainer, false);
        gameObject.GetComponent<Image>().sprite = circleSprite;
        RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = new Vector2(3, 3);
        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(0, 0);
        return gameObject;
    }

    private void ShowGraph()
    {
        foreach (Transform child in graphContainer.transform) GameObject.Destroy(child.gameObject);

        var itemsToShow = 20;
        var skip = Math.Max(0, gameManager.currentWeek - itemsToShow);
        var take = Math.Min(itemsToShow, gameManager.currentWeek);
        List<float> valueList = share.Values.Skip(skip).Take(take).Select(value => value.Value).ToList();

        float graphHeight = graphContainer.sizeDelta.y;
        float xSize = (graphContainer.sizeDelta.x) / (valueList.Count + 1);

        float yMaximum = Mathf.Max(valueList.ToArray());

        GameObject lastCircleGameObject = null;
        for (int i = 0; i < valueList.Count; i++)
        {
            float xPosition = xSize + i * xSize;
            float yPosition = (valueList[i] / yMaximum) * graphHeight;
            GameObject circleGameObject = CreateCircle(new Vector2(xPosition, yPosition));
            if (lastCircleGameObject != null)
            {
                CreateDotConnection(lastCircleGameObject.GetComponent<RectTransform>().anchoredPosition, circleGameObject.GetComponent<RectTransform>().anchoredPosition);
            }
            lastCircleGameObject = circleGameObject;
        }
    }

    private void CreateDotConnection(Vector2 dotPositionA, Vector2 dotPositionB)
    {
        GameObject gameObject = new GameObject("dotConnection", typeof(Image));
        gameObject.transform.SetParent(graphContainer, false);
        gameObject.GetComponent<Image>().color = new Color(1, 1, 1, .5f);
        RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
        Vector2 dir = (dotPositionB - dotPositionA).normalized;
        float distance = Vector2.Distance(dotPositionA, dotPositionB);
        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(0, 0);
        rectTransform.sizeDelta = new Vector2(distance, 1f);
        rectTransform.anchoredPosition = dotPositionA + dir * distance * .5f;
        rectTransform.localEulerAngles = new Vector3(0, 0, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);
    }

    public void ShowTips()
    {
        tips.SetActive(true);
    }

    public void HideTips()
    {
        tips.SetActive(false);
    }

    private void setupShare()
    {
        int shareIndex = transform.GetSiblingIndex();

        if (shareIndex < gameManager.sharesCount)
        {
            share = gameManager.shares[shareIndex];
            nameUi.text = share.Name;

            currentValueUi.text = $"R$ {share.Values[gameManager.currentWeek].Value.ToString("N2")}";
        }
    }

    private void handleWeekChange()
    {
        if (share != null)
        {
            currentValueUi.text = $"R$ {share.Values[gameManager.currentWeek].Value.ToString("N2")}";
            updateCashAmount();

            ShowGraph();
        }
    }

    private void updateCashAmount()
    {
        if (share != null)
        {
            var currentValue = share.Values[gameManager.currentWeek];
            double cash = currentValue.Value * share.BelongToThePlayerCount;
            cashAmountUi.text = $"R$ {cash.ToString("N2")}";
            profitUi.text = $"Lucro: R$ {(cash - investedCash).ToString("N2")}";

            variationUi.text = currentValue.Variation >= 0 ? "+ " : "- ";
            variationUi.text += $"{Mathf.Abs((float)currentValue.Variation).ToString("N2")}%";
        }
    }

    public void Buy()
    {
        if (share != null)
        {
            int buyCount = buySellCount;

            double price = buyCount * share.Values[gameManager.currentWeek].Value;

            if (gameManager.PocketCash >= price)
            {
                investedCash += price;

                gameManager.PocketCash -= price;
                share.BelongToThePlayerCount += buyCount;
                amountUi.text = share.BelongToThePlayerCount.ToString();
                updateCashAmount();

                audioSource.Play();
            }
            else
            {
                // @TODO não pode comprar
            }
        }
    }

    public void Sell()
    {
        if (share != null)
        {
            int sellCount = buySellCount;

            if (share.BelongToThePlayerCount >= sellCount)
            {
                double sellPrice = sellCount * share.Values[gameManager.currentWeek].Value;

                investedCash -= sellPrice;

                gameManager.PocketCash += sellPrice;
                share.BelongToThePlayerCount -= sellCount;
                amountUi.text = share.BelongToThePlayerCount.ToString();
                updateCashAmount();

                audioSource.Play();
            }
            else
            {
                // @TODO não pode vender
            }
        }
    }
}
