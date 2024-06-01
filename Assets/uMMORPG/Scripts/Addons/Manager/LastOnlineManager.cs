using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LastOnlineManager : MonoBehaviour
{
    public static LastOnlineManager singleton;
    
    void Start()
    {
        if (!singleton) singleton = this;    
    }

    public string CalculateTime (DateTime inputDate)
    {
        DateTime currentDate = DateTime.Now;
        TimeSpan timeDifference = currentDate - inputDate;

        if (timeDifference.TotalDays >= 365)
        {
            int years = (int)(timeDifference.TotalDays / 365);
            return $"{years}y";
        }
        else if (timeDifference.TotalDays >= 30)
        {
            int months = (int)(timeDifference.TotalDays / 30);
            return $"{months}m";
        }
        else if (timeDifference.TotalDays >= 1)
        {
            int days = (int)timeDifference.TotalDays;
            return $"{days}d";
        }
        else if (timeDifference.TotalHours >= 1)
        {
            int hours = (int)timeDifference.TotalHours;
            return $"{hours}h";
        }
        else
        {
            int minutes = (int)timeDifference.TotalMinutes;
            return $"{minutes}h";
        }
    }
}
