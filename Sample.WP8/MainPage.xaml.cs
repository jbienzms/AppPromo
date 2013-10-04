using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Sample.WP8.Resources;
using AppPromo;

namespace Sample.WP8
{
    public partial class MainPage : PhoneApplicationPage
    {
        private const string ShownText = "Shown";
        private const string NotShownText = "Not Shown";

        // Constructor
        public MainPage()
        {
            InitializeComponent();
        }

        private void RateReminder_TryReminderCompleted(object sender, RateReminderResult e)
        {
            // Update on UI thread
            Dispatcher.BeginInvoke(() =>
                {
                    DaysLabel.Text = e.Days.ToString();
                    RunsLabel.Text = e.Runs.ToString();
                    ReminderLabel.Text = (e.ReminderShown ? ShownText : NotShownText);
                    RatingLabel.Text = (e.RatingShown ? ShownText : NotShownText);
                });
        }

        private void ResetCountersButton_Click(object sender, RoutedEventArgs e)
        {
            RateReminder.ResetCounters();
            DaysLabel.Text = "0";
            RunsLabel.Text = "0";
            ReminderLabel.Text = NotShownText;
            RatingLabel.Text = NotShownText;
            ResetBlock.Text = "Counters Reset";
        }
    }
}