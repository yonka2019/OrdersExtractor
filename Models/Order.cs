using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace OrdersExtractor.Models
{
    internal class Order
    {
        private static readonly Regex PACKAGE_NUMBER_REGEX = new Regex(@"^(.\s*\d+)$", RegexOptions.Multiline);
        private static readonly Regex TRACK_NUMBER_REGEX = new Regex(@"משלוח\s*(.+)");
        private static readonly Regex LOCATION_REGEX = new Regex(@"ממתין ב(.+).$", RegexOptions.Multiline);
        private static readonly Regex URL_REGEX = new Regex(@"(https?:\/\/(www\.)?[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,6}\b([-a-zA-Z0-9()@:%_\+.~#?&\/\/=]*))");

        internal string PackageNumber { get; private set; }
        internal string TrackNumber { get; private set; }
        internal string Location { get; private set; }
        internal string URL { get; private set; }
        internal DateTime SMSArrivedOn { get; private set; }

        internal Order(string packageNumber, string trackNumber, string location, string url, DateTime arrivedOn)
        {
            PackageNumber = packageNumber;
            TrackNumber = trackNumber;
            Location = location;
            URL = url;
            SMSArrivedOn = arrivedOn;
        }

        internal Order(string message, DateTime arrivedOn)
        {
            PackageNumber = PACKAGE_NUMBER_REGEX.Match(message).Groups[1].Value;
            TrackNumber = TRACK_NUMBER_REGEX.Match(message).Groups[1].Value;
            Location = LOCATION_REGEX.Match(message).Groups[1].Value;
            URL = URL_REGEX.Match(message).Groups[1].Value;
            SMSArrivedOn = arrivedOn;
        }

        public override string ToString()
        {
            return $"**• Tracking Number:** {TrackNumber}\n\n" +
                $"**• Package Location:** {Location}\n\n" +
                $"**• Message Arrived At:** {SMSArrivedOn.ToString(CultureInfo.GetCultureInfo("he-IL"))}\n\n" +
                $"**• More Info:** {URL}";
        }
    }
}