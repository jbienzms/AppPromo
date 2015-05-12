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

namespace AppPromo.Controls
{
    /// <summary>
    /// A dialog that prompts a user to perform an action.
    /// </summary>
    public sealed partial class ActionDialog : ContentDialog
    {
        #region Static Version

        #region Dependency Property Defninitions
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
        #endregion // Dependency Property Defninitions

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
        /// Initialzies a new <see cref="ActionDialog"/> instance.
        /// </summary>
        public ActionDialog()
        {
            this.InitializeComponent();
            this.Loaded += ActionDialog_Loaded;
            this.Unloaded += ActionDialog_Unloaded;
        }

        #endregion // Constructors

        private void HandleSizeChange(Size newSize)
        {
            // HACK for ContentDialog not showing at right size and not handling rotation properly
            LayoutRoot.Width = Math.Max(newSize.Width - 50, 0);
            LayoutRoot.Height = Math.Max(newSize.Height - 50, 0);
        }

        #region Overrides / Event Handlers
        private void ActionDialog_Loaded(object sender, RoutedEventArgs e)
        {
            Window.Current.SizeChanged += Window_SizeChanged;
            var bounds = Window.Current.Bounds;
            HandleSizeChange(new Size(bounds.Width, bounds.Height));
        }

        private void ActionDialog_Unloaded(object sender, RoutedEventArgs e)
        {
            Window.Current.SizeChanged -= Window_SizeChanged;
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
        private void Window_SizeChanged(object sender, Windows.UI.Core.WindowSizeChangedEventArgs e)
        {
            HandleSizeChange(e.Size);
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
