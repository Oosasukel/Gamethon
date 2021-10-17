using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SavingsManager : MonoBehaviour, IInteractable
{
    private GameManager gameManager;
    public TextMeshProUGUI cashAmountUi;
    public AudioSource audioSource;
    public TextMeshProUGUI profitUi;
    public double investedCash = 0;
    public GameObject tips;
    public double buySellCount = 100;
    public float monthlyIncrement = 0.003571f;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void Start()
    {
        HideTips();

        gameManager = FindObjectOfType<GameManager>();

        gameManager.OnChangeMonth += handleMonthChange;
    }

    public void ShowTips()
    {
        tips.SetActive(true);
    }

    public void HideTips()
    {
        tips.SetActive(false);
    }

    private void handleMonthChange()
    {
        gameManager.savingsCash *= (1 + monthlyIncrement);
        updateCashAmount();
    }

    private void updateCashAmount()
    {
        cashAmountUi.text = $"R$ {gameManager.savingsCash.ToString("N2")}";
        profitUi.text = $"Lucro: R$ {(gameManager.savingsCash - investedCash).ToString("N2")}";
    }

    public void Buy()
    {
        double amountToInvest = buySellCount;

        if (gameManager.PocketCash >= amountToInvest)
        {
            investedCash += amountToInvest;

            gameManager.PocketCash -= amountToInvest;
            gameManager.savingsCash += amountToInvest;

            updateCashAmount();

            audioSource.Play();
        }
        else
        {
            // @TODO não pode investir
        }
    }

    public void Sell()
    {
        double amountToRescue = buySellCount;

        if (gameManager.savingsCash >= amountToRescue)
        {
            investedCash -= amountToRescue;

            gameManager.PocketCash += amountToRescue;
            gameManager.savingsCash -= amountToRescue;

            updateCashAmount();

            audioSource.Play();
        }
        else
        {
            // @TODO não pode resgatar
        }
    }
}
