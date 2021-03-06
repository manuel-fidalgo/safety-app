﻿using Plugin.Vibrate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Safe
{
    class VibrationManager
    {
        public static void vibrate(int ms)
        {
            //Will vibrate only if true in the settings
            if (SettingsWrap.vibration_status)
            {
                var v = CrossVibrate.Current;
                v.Vibration(ms); // 1 second vibration
            }
        }
    }
    internal  class MessageManager
    {
        public static void sendAlertMessage()
        {
            System.Diagnostics.Debug.WriteLine("Message sucess");
            if (SettingsWrap.use_personal_message)
            {
                Android.Telephony.SmsManager.Default.SendTextMessage(SettingsWrap.contact_number, null, SettingsWrap.contact_Message, null, null);
            }else
            {
                VectorValue c = Gps.lastcor;

                if (c != null) {
                    String s = "Safe app has detected and accident at Lon:{"+c.x+"} Lat:{" +c.y+"}";
                    Android.Telephony.SmsManager.Default.SendTextMessage(SettingsWrap.contact_number, null, s, null, null);
                }else
                {
                    String s = "Safe app has detected and accident.";
                    Android.Telephony.SmsManager.Default.SendTextMessage(SettingsWrap.contact_number, null, s, null, null);
                }
            }
        }
    }
}
