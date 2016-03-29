using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace AppPromo.UWP.Controls
{
    /// <summary>
    /// A dialog that prompts a user to perform an action.
    /// </summary>
    public sealed partial class ActionDialog : ContentDialog
    {
        #region Static Version

        #region Dependency Property Definitions
        /// <summary>
        /// Identifies the <see cref="ConfirmText"/> dependency property.
        /// </summary>
        static internal readonly DependencyProperty ConfirmTextProperty = DependencyProperty.Register("ConfirmText", typeof(string), typeof(ActionDialog), new PropertyMetadata("Yes", OnConfirmTextChanged));

        /// <summary>
        /// Identifies the <see cref="DeclineText"/> dependency property.
        /// </summary>
        static internal readonly DependencyProperty DeclineTextProperty = DependencyProperty.Register("DeclineText", typeof(string), typeof(ActionDialog), new PropertyMetadata("Never", OnDeclineTextChanged));

        /// <summary>
        /// Identifies the <see cref="DelayText"/> dependency property.
        /// </summary>
        static internal readonly DependencyProperty DelayTextProperty = DependencyProperty.Register("DelayText", typeof(string), typeof(ActionDialog), new PropertyMetadata("Later", OnDelayTextChanged));

        /// <summary>
        /// Identifies the <see cref="DontRemindAgain"/> dependency property.
        /// </summary>
        static internal readonly DependencyProperty DontRemindAgainProperty = DependencyProperty.Register("DontRemindAgain", typeof(bool), typeof(ActionDialog), new PropertyMetadata(false));

        /// <summary>
        /// Identifies the <see cref="DontRemindAgainText"/> dependency property.
        /// </summary>
        static internal readonly DependencyProperty DontRemindAgainTextProperty = DependencyProperty.Register("DontRemindAgainText", typeof(string), typeof(ActionDialog), new PropertyMetadata("Don't remind me again"));

        /// <summary>
        /// Identifies the <see cref="PromptText"/> dependency property.
        /// </summary>
        static internal readonly DependencyProperty PromptTextProperty = DependencyProperty.Register("PromptText", typeof(string), typeof(ActionDialog), new PropertyMetadata("Would you like to perform this action?"));
        #endregion // Dependency Property Definitions

        #region Overrides / Event Handlers
        private static void OnConfirmTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // Get ActionDialog instance
            var dlg = (ActionDialog)d;

            // Update button text
            dlg.PrimaryButtonText = (string)e.NewValue;
        }

        private static void OnDeclineTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // Get ActionDialog instance
            var dlg = (ActionDialog)d;

            // If "Don't remind me again" box is checked, update the button text.
            if (dlg.ChkDontRemind.IsChecked == true)
            {
                dlg.SecondaryButtonText = (string)e.NewValue;
            }
        }

        private static void OnDelayTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // Get ActionDialog instance
            var dlg = (ActionDialog)d;

            // If "Don't remind me again" box isn't checked, update the button text.
            if (dlg.ChkDontRemind.IsChecked != true)
            {
                dlg.SecondaryButtonText = (string)e.NewValue;
            }
        }
        #endregion // Overrides / Event Handlers
        #endregion // Static Version

        #region Instance Version
        #region Constructors
        /// <summary>
        /// Initializes a new <see cref="ActionDialog"/> instance.
        /// </summary>
        public ActionDialog()
        {
            this.InitializeComponent();
            this.Opened += ActionDialog_Opened;
        }
        #endregion // Constructors

        #region Overrides / Event Handlers
        private void ActionDialog_Opened(ContentDialog sender, ContentDialogOpenedEventArgs args)
        {
            // HACK: Tweak the ContentDialog to show expanded content.
            // 
            // I'm really not a fan of hacks like this. They're brittle and if done wrong
            // they can actually crash apps. But unfortunately the inner template for 
            // ContentDialog is not easily changed and having the "don't remind me again" 
            // check box floating up near the content looks really bad.
            //
            // Though the implementation below is brittle, at least it should not crash 
            // if the default template changes and is not likely to break existing content.
            // JB - 2016-03-29


            // Try to find a parent Grid control
            FrameworkElement parent = VisualTreeHelper.GetParent(LayoutRoot) as FrameworkElement;
            var parentGrid = parent as Grid;
            while ((parent != null) && (parentGrid == null))
            {
                parent = VisualTreeHelper.GetParent(parent) as FrameworkElement;
                parentGrid = parent as Grid;
            }

            // If found
            if (parentGrid != null)
            {
                // And it has exactly two rows
                if (parentGrid.RowDefinitions.Count == 2)
                {
                    // Assume it's the right one and expand the content area (second row)
                    parentGrid.RowDefinitions[1].Height = new GridLength(1, GridUnitType.Star);
                }
            }
        }

        private void ChkDontRemind_Checked(object sender, RoutedEventArgs e)
        {
            if (ChkDontRemind.IsChecked == true)
            {
                SecondaryButtonText = DeclineText;
            }
            else
            {
                SecondaryButtonText = DelayText;
            }

        }
        #endregion // Overrides / Event Handlers

        #region Public Properties
        /// <summary>
        /// Gets or sets the ConfirmText of the <see cref="ActionDialog"/>. This is a dependency property.
        /// </summary>
        /// <value>
        /// The ConfirmText of the <see cref="ActionDialog"/>.
        /// </value>
        public string ConfirmText
        {
            get
            {
                return (string)GetValue(ConfirmTextProperty);
            }
            set
            {
                SetValue(ConfirmTextProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the DeclineText of the <see cref="ActionDialog"/>. This is a dependency property.
        /// </summary>
        /// <value>
        /// The DeclineText of the <see cref="ActionDialog"/>.
        /// </value>
        public string DeclineText
        {
            get
            {
                return (string)GetValue(DeclineTextProperty);
            }
            set
            {
                SetValue(DeclineTextProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the DelayText of the <see cref="ActionDialog"/>. This is a dependency property.
        /// </summary>
        /// <value>
        /// The DelayText of the <see cref="ActionDialog"/>.
        /// </value>
        public string DelayText
        {
            get
            {
                return (string)GetValue(DelayTextProperty);
            }
            set
            {
                SetValue(DelayTextProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the DontRemindAgain of the <see cref="ActionDialog"/>. This is a dependency property.
        /// </summary>
        /// <value>
        /// The DontRemindAgain of the <see cref="ActionDialog"/>.
        /// </value>
        public bool DontRemindAgain
        {
            get
            {
                return (bool)GetValue(DontRemindAgainProperty);
            }
            set
            {
                SetValue(DontRemindAgainProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the DontRemindAgainText of the <see cref="ActionDialog"/>. This is a dependency property.
        /// </summary>
        /// <value>
        /// The DontRemindAgainText of the <see cref="ActionDialog"/>.
        /// </value>
        public string DontRemindAgainText
        {
            get
            {
                return (string)GetValue(DontRemindAgainTextProperty);
            }
            set
            {
                SetValue(DontRemindAgainTextProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the PromptText of the <see cref="ActionDialog"/>. This is a dependency property.
        /// </summary>
        /// <value>
        /// The PromptText of the <see cref="ActionDialog"/>.
        /// </value>
        public string PromptText
        {
            get
            {
                return (string)GetValue(PromptTextProperty);
            }
            set
            {
                SetValue(PromptTextProperty, value);
            }
        }
        #endregion // Public Properties
        #endregion // Instance Version
    }
}
