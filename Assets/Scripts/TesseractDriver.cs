﻿using System;
using UnityEngine;

public class TesseractDriver
{
    private TesseractWrapper _tesseract;

    public string CheckTessVersion()
    {
        _tesseract = new TesseractWrapper();

        try
        {
            string version = "Tesseract version: "+ _tesseract.Version();
            Debug.Log(version);
            return version;
        }
        catch (Exception e)
        {
            string errorMessage = e.GetType() + " - " + e.Message;
            Debug.LogError(errorMessage);
            return errorMessage;
        }
    }
}