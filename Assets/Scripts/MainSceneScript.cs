using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;
using System.Net;
using System;
using System.IO;
using SimpleJSON;

public class MainSceneScript : MonoBehaviour
{

    public Text infoText;
    public Button mButtonConvert;
    public InputField amountInputField, amountOutputField;
    public Dropdown dayPicker, monthPicker, yearPicker, currencyInPicker, currencyOutPicker;
    private int day, month, year;
    private double amount;
    private string currencyIn, currencyOut;
    private CultureInfo ci;
    private bool isSafe;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Started");
        mButtonConvert.onClick.AddListener(ConvertOnClick);
        CultureInfo ci = (CultureInfo)CultureInfo.CurrentCulture.Clone();
        ci.NumberFormat.CurrencyDecimalSeparator = ".";

        //Set the Pickers to current date
        DateTime dt = DateTime.Today;
        dayPicker.value = int.Parse(dt.ToString("dd"))-1;
        monthPicker.value = int.Parse(dt.ToString("MM"))-1;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void ConvertOnClick() 
    {
        int day = int.Parse(dayPicker.options[dayPicker.value].text);
        int month = int.Parse(monthPicker.options[monthPicker.value].text);
        int year = int.Parse(yearPicker.options[yearPicker.value].text);
        string currencyIn = currencyInPicker.options[currencyInPicker.value].text;
        string currencyOut = currencyOutPicker.options[currencyOutPicker.value].text;
        Debug.Log("currencies : " + currencyIn + currencyOut);

        bool isSafe = SafeCheck(day, month, year, currencyIn, currencyOut);
        
        if (isSafe == false) 
        {
            infoText.text = "Please check the date, and that the currencies are different";
            return;
        }
        else 
        {
            infoText.text = "";
        }

        double rate = GetRate(day, month, year, currencyIn, currencyOut);
        double amount = double.Parse(amountInputField.text, CultureInfo.InvariantCulture);
        double finalAmount = ConvertMoney(amount, rate);
        amountOutputField.text = finalAmount.ToString();
    }

    double GetRate(int day, int month, int year, string currencyIn, string currencyOut)
    {
        string url = "https://api.exchangeratesapi.io/" + year + "-" + month + "-" + day + "?base=" + currencyIn;   
        try {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            StreamReader reader = new StreamReader(response.GetResponseStream());
            string jsonResponse = reader.ReadToEnd();
            Debug.Log(response);
            JSONNode data = JSON.Parse(jsonResponse);
            string rateResponse = data["rates"][currencyOut];
            Debug.Log(rateResponse.GetType());
            Debug.Log(currencyOut + " rate : " + data["rates"][currencyOut]);
            return double.Parse(rateResponse, CultureInfo.InvariantCulture);
        } catch (WebException ex) {
            if (day == 29 && month == 2) 
            {
                infoText.text = "There was no 29/02 during this year";
            } else
            {
                infoText.text = "An error occured, check your connection and try again.";
            }
            return 0;
        }
        
    }

    //Func to return the conversion, currencyIn x rate = amountOut
    double ConvertMoney(double amount, double rate)
    {
         return amount*rate;
    }

    //Check if the date is correct, and if the currencies are different, to avoid errors.
    bool SafeCheck(int day, int month, int year, string currencyIn, string currencyOut)
    {
        //February check
        if (month == 2) 
        {
            if (day > 29) 
            {
                Debug.Log("Day over 29 : " + day);
                return false;
            }
        }

        //30-days months
        if (month == 4 || month == 6 || month == 9 || month == 11) 
        {
            if (day > 30) 
            { 
                 Debug.Log("Day over 30 : " + day);
                return false;
            }
        }

        //Currency check 
        if (currencyIn == currencyOut) 
        {
            Debug.Log("same currency : " + currencyIn + currencyOut);
            return false;
        }

        return true;
    }
}