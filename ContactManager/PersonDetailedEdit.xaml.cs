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

namespace ContactManager
{
    /// <summary>
    /// Interaction logic for PersonDetailedEdit.xaml
    /// </summary>
    public partial class PersonDetailedEdit : UserControl
    {
        public event EventHandler NameChange;
        public event EventHandler MoveUp;
        public event EventHandler MoveDown;
        public event EventHandler Delete;

        public PersonDetailedEdit()
        {
            InitializeComponent();
        }

        private void txtFirstName_KeyUp(object sender, KeyEventArgs e)
        {
            (DataContext as Person).FirstName = txtFirstName.Text;
            NameChange(this, EventArgs.Empty);
        }

        private void chkInclude_Click(object sender, RoutedEventArgs e)
        {
            NameChange(this, EventArgs.Empty);
        }

        private void btnUp_Click(object sender, RoutedEventArgs e)
        {
            MoveUp(DataContext, EventArgs.Empty);
        }

        private void btnDown_Click(object sender, RoutedEventArgs e)
        {
            MoveDown(DataContext, EventArgs.Empty);
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            Delete(DataContext, EventArgs.Empty);
        }
    }
}
