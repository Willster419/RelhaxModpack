﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RelhaxModpack.UIComponents
{
    /// <summary>
    /// Interaction logic for RelhaxWPFComboBox.xaml
    /// </summary>
    public partial class RelhaxWPFComboBox : ComboBox, INotifyPropertyChanged
    {
        /// <summary>
        /// Create an instance of the RelhaxWPFComboBox class
        /// </summary>
        public RelhaxWPFComboBox()
        {
            InitializeComponent();
        }

        /// <summary>
        /// The event to subscribe to when the selection is changed
        /// </summary>
        public SelectionChangedEventHandler Handler;

        /// <summary>
        /// Called from the database object to update the UI on a combobox selection change
        /// </summary>
        /// <param name="spc">The SelectablePakage that caused the update</param>
        /// <param name="value">The checked value</param>
        public void OnDropDownSelectionChanged(SelectablePackage spc, bool value)
        {
            for (int i = 0; i < Items.Count; i++)
            {
                ComboBoxItem cbi = (ComboBoxItem)Items[i];
                if (cbi.Package.Equals(spc) && value && cbi.Package.Enabled)
                {
                    if (Handler != null)
                        SelectionChanged -= Handler;
                    SelectedItem = cbi;
                    if (Handler != null)
                        SelectionChanged += Handler;
                    continue;
                }
                //if value is false it will uncheck all the packages
                if (cbi.Package.Enabled && cbi.Package.Checked)
                    cbi.Package.Checked = false;
            }
            if (!value)
            {
                SelectionChanged -= Handler;
                SelectedIndex = 0;
                SelectionChanged += Handler;
            }
        }

        #region Data UI Binding
        private Color _DisabledColor = Colors.DarkGray;

        /// <summary>
        /// Set the value of the disabled component color
        /// </summary>
        public Color DisabledColor
        {
            get
            {
                return _DisabledColor;
            }
            set
            {
                _DisabledColor = value;
                OnPropertyChanged(nameof(DisabledColor));
            }
        }

        //https://stackoverflow.com/questions/34651123/wpf-binding-a-background-color-initializes-but-not-updating
        /// <summary>
        /// Event to trigger when an internal property is changed. It forces a UI update
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Method to invoke the PropertyChanged event to update the UI
        /// </summary>
        /// <param name="propertyName">The name of the property that changed, to update it's UI binding</param>
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
