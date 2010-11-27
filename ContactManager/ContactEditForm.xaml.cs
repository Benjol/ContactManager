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
using ContactManager.Data;
using System.Diagnostics;
using ContactManager.Properties;
using System.Collections.ObjectModel;

namespace ContactManager
{
    /// <summary>
    /// Interaction logic for ContactEditForm.xaml
    /// </summary>
    public partial class ContactEditForm : Window
    {
        public event EventHandler RequestSave = delegate { };
        public event EventHandler RequestCancel = delegate { };

        public ContactEditForm()
        {
            InitializeComponent();
        }
        
        public Contact Contact
        {
            get { return DataContext as Contact; }
            set { DataContext = value; }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            RequestCancel(this, EventArgs.Empty);
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            Contact.UpdateFirstNames();
            RequestSave(this, EventArgs.Empty);
        }

        private void txtName_KeyUp(object sender, KeyEventArgs e)
        {
            Contact.Name = txtName.Text;
        }

        private void PersonDetailedEdit_NameChange(object sender, EventArgs e)
        {
            Contact.UpdateFirstNames();
        }

        private void PersonDetailedEdit_MoveUp(object sender, EventArgs e)
        {
            var person = (sender as Person);
            var curpos = Contact.People.IndexOf(person);
            if(curpos > 0)
            {
                Contact.People.Move(curpos, curpos - 1);
                Contact.UpdateFirstNames();
            }
        }

        private void PersonDetailedEdit_MoveDown(object sender, EventArgs e)
        {
            var person = (sender as Person);
            var curpos = Contact.People.IndexOf(person);
            if(curpos < Contact.People.Count - 1)
            {
                Contact.People.Move(curpos, curpos + 1);
                Contact.UpdateFirstNames();
            }
        }

        private void PersonDetailedEdit_Delete(object sender, EventArgs e)
        {
            var person = (sender as Person);
            var curpos = Contact.People.Remove(person);
            Contact.UpdateFirstNames();
        }

        private void btnNewPerson_Click(object sender, RoutedEventArgs e)
        {
            Contact.AddPerson(new Person { LastName = Contact.Name, IncludeInDisplayName = false });
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            RequestCancel(this, EventArgs.Empty);
        }
    }
}
