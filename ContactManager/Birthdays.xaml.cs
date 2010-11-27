using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ContactManager.Properties;
using ContactManager.Data;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace ContactManager
{
    /// <summary>
    /// Interaction logic for Birthdays.xaml
    /// </summary>
    public partial class Birthdays : Window
    {
        AddressBook m_AddressBook;

        public Birthdays()
        {
            InitializeComponent();
        }

        //Dependency property
        public bool FilterOn
        {
            get { return (bool)GetValue(FilterOnProperty); }
            set { SetValue(FilterOnProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FilterOn.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FilterOnProperty =
            DependencyProperty.Register("FilterOn", typeof(bool), typeof(Birthdays));

        public Birthdays(AddressBook addressBook) : this()
        {
            m_AddressBook = addressBook;
            ResetContext();
        }

        private void slider1_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            ResetContext();
        }

        private void ResetContext()
        {
            if(m_AddressBook != null)
                DataContext = new ObservableCollection<Person>(m_AddressBook.GetUpcomingBirthdays((int)slider1.Value, chkNotable.IsChecked ?? false));
        }

        private void chkNotable_Click(object sender, RoutedEventArgs e)
        {
            ResetContext();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        //for later: http://joshsmithonwpf.wordpress.com/2007/06/12/searching-for-items-in-a-listbox/
        private void txtFilter_KeyUp(object sender, KeyEventArgs e)
        {
            if(txtFilter.Text.Length > 2)
            {
                FilterOn = true;
                listView.Items.Filter = new Predicate<object>(GetCanShow);
            }
            else
            {
                FilterOn = false;
                listView.Items.Filter = null;
            }
            e.Handled = true; //don't bubble this! http://msdn.microsoft.com/en-us/library/ms742806.aspx
        }

        private bool GetCanShow(object item)
        {
            var person = item as Person;
            bool show = false;
            if(person != null)
                show = (person.AllText.IndexOf(txtFilter.Text, StringComparison.InvariantCultureIgnoreCase) >= 0);
            return show;
        }

        private void btnClearFilter_Click(object sender, RoutedEventArgs e)
        {
            txtFilter.Text = "";
            listView.Items.Filter = null;
            FilterOn = false;
        }
    }
}
