using Android.Content;
using Android.Database;
using Android.Provider;
using System;
using System.Collections.Generic;

namespace OrdersExtractor.Models
{
    internal static class SMSData
    {
        private const string PACKAGE_RECEIVED_PATTERN = @"ממתין ב";
        internal static List<Order> ExtractOrders(ContentResolver c, string phoneNumber)
        {
            List<Order> orders = new List<Order>();

            ContentResolver contentResolver = c;
            Android.Net.Uri uri = Telephony.Sms.Inbox.ContentUri;

            string[] projection = { "body", "date" };

            string selection = "address = ?";
            string[] selectionArgs = { phoneNumber };

            ICursor cursor = contentResolver.Query(uri, projection, selection, selectionArgs, null);

            if (cursor != null)
            {
                while (cursor.MoveToNext())
                {
                    string message = cursor.GetString(cursor.GetColumnIndex(projection[0]));
                    long unixDate = cursor.GetLong(cursor.GetColumnIndex(projection[1]));

                    if (message.Contains(PACKAGE_RECEIVED_PATTERN))
                        orders.Add(new Order(message, UnixToSTR(unixDate)));
                }

                cursor.Close();
            }

            return orders;
        }

        private static DateTime UnixToSTR(long unixDate)
        {
            DateTime start = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return start.AddMilliseconds(unixDate).ToLocalTime();

        }
    }
}
