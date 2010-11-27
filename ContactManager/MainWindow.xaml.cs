using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
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
using ContactManager.Properties;
using ContactManager.UndoStuff;

namespace ContactManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        AddressBook m_AddressBook;

        //Dependency property
        public UndoRedo UndoRedo
        {
            get { return (UndoRedo)GetValue(UndoRedoProperty); }
            set { SetValue(UndoRedoProperty, value); }
        }

        //Dependency property
        public bool FilterOn
        {
            get { return (bool)GetValue(FilterOnProperty); }
            set { SetValue(FilterOnProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FilterOn.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FilterOnProperty =
            DependencyProperty.Register("FilterOn", typeof(bool), typeof(MainWindow));

        // Using a DependencyProperty as the backing store for FilterOn.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty UndoRedoProperty =
            DependencyProperty.Register("UndoRedo", typeof(UndoRedo), typeof(MainWindow));


        enum EMode { Edit, Add, None }
        EMode m_EditMode = EMode.None;
        ContactEditForm m_ContactEdit;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //http://blogs.interknowlogy.com/joelrumerman/archive/2007/04/03/12497.aspx
            //http://www.thejoyofcode.com/Sortable_ListView_in_WPF.aspx

            LoadDefaultAddressBook();
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Hyperlink thisLink = (Hyperlink)sender;
            string navigateUri = thisLink.NavigateUri.ToString();
            Process.Start(new ProcessStartInfo("mailto:" + navigateUri));
            e.Handled = true;
        }

        private void listView_KeyUp(object sender, KeyEventArgs e)
        {
            //if(e.Key == Key.LeftCtrl)
            //    this.Opacity = 1.0;

            var first = m_AddressBook.Contacts.FirstOrDefault(c => c.OrderByName.StartsWith(e.Key.ToString()));
            if(first != null)
            {
                SelectContact(first);
            }
        }


        private void SelectContact(Contact first)
        {
            ScrollViewer scrollViewer = GetScrollViewer(listView) as ScrollViewer;
            listView.SelectedItem = first;
            scrollViewer.ScrollToBottom();
            listView.ScrollIntoView(first);
            listView.Focus();
        }

        //http://stackoverflow.com/questions/1077397/scroll-listviewitem-to-be-at-the-top-of-a-listview
        public static DependencyObject GetScrollViewer(DependencyObject o)
        {
            if(o is ScrollViewer)
            { return o; }

            for(int i = 0; i < VisualTreeHelper.GetChildrenCount(o); i++)
            {
                var child = VisualTreeHelper.GetChild(o, i);

                var result = GetScrollViewer(child);
                if(result == null)
                {
                    continue;
                }
                else
                {
                    return result;
                }
            }

            return null;
        }

        private void btnSplit_Click(object sender, RoutedEventArgs e)
        {
            if(listView.SelectedItem is Contact)
            {
                var contact = listView.SelectedItem as Contact;
                var newlist = Contact.SplitContact(contact);
                UndoRedo.PerformAction("split " + contact.Name, new[] { contact }, newlist);
                SelectContact(newlist.First());
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if(listView.SelectedItem is Contact)
            {
                var contact = listView.SelectedItem as Contact;
                UndoRedo.PerformAction("delete " + contact.DisplayName, new[] {contact}, null);
            }
        }

        private void btnCopy_Click(object sender, RoutedEventArgs e)
        {
            var contact = listView.SelectedItem as Contact;
            Clipboard.SetText(contact.DisplayName + Environment.NewLine + contact.Address);
        }

        private void btnNew_Click(object sender, RoutedEventArgs e)
        {
            EnterEdition(EMode.Add, "");
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            if(listView.SelectedItem is Contact)
            {
                var contact = listView.SelectedItem as Contact;
                string text = StringX.NewLineAfterIf(contact.DisplayName) 
                    + StringX.NewLineAfterIf(contact.Address) 
                    + StringX.NewLineAfterIf(contact.PhoneNumber) 
                    + StringX.NewLineAfterIf(contact.Email);
                EnterEdition(EMode.Edit, text);
            }
        }

        private void btnEditCancel_Click(object sender, RoutedEventArgs e)
        {
            ExitEdition();
        }

        private void btnEditOK_Click(object sender, RoutedEventArgs e)
        {
            Contact newcontact = null;
            switch(m_EditMode)
            {
                case EMode.Add:
                    newcontact = Contact.ParseFromText(txtEdit.Text);
                    UndoRedo.PerformAction("add " + newcontact.Name, Utilities.EnumerableUnit<Contact>(null), Utilities.EnumerableUnit(newcontact));
                    break;
                case EMode.Edit:
                    var oldcontact = listView.SelectedItem as Contact;
                    newcontact = Contact.QuickEdit(oldcontact, txtEdit.Text);
                    UndoRedo.PerformAction("edit " + newcontact.Name, Utilities.EnumerableUnit(oldcontact), Utilities.EnumerableUnit(newcontact));
                    break;
                default: break;
            }

            if(newcontact != null) SelectContact(newcontact);
            ExitEdition();
        }

        private void ExitEdition()
        {
            cnvEdit.Visibility = Visibility.Hidden;
            txtEdit.Text = "";
            m_EditMode = EMode.None;
        }

        private void EnterEdition(EMode mode, string text)
        {
            txtEdit.Text = text;
            cnvEdit.Visibility = Visibility.Visible;
            m_EditMode = mode;
        }

        private void btnMerge_Click(object sender, RoutedEventArgs e)
        {
            var contacts = listView.SelectedItems.Cast<Contact>().ToList();

            if(contacts != null && contacts.Count() > 1)
            {
                var newContact = Contact.MergeContacts(contacts);
                UndoRedo.PerformAction("merge contacts", contacts, new[] { newContact });
                SelectContact(newContact);
            }
        }

        private void btnDetail_Click(object sender, RoutedEventArgs e)
        {
            if(m_ContactEdit == null) 
            {
                m_ContactEdit = new ContactEditForm();
                m_ContactEdit.Owner = this;
                m_ContactEdit.RequestSave += DetailedEdit_RequestSave;
                m_ContactEdit.RequestCancel += DetailedEdit_RequestCancel;
            }
            m_ContactEdit.Contact = (listView.SelectedItem as Contact).DeepClone();
            m_ContactEdit.ShowDialog();
        }


        private void listView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if(m_ContactEdit == null)
            {
                m_ContactEdit = new ContactEditForm();
                m_ContactEdit.Owner = this;
                m_ContactEdit.RequestSave += DetailedEdit_RequestSave;
                m_ContactEdit.RequestCancel += DetailedEdit_RequestCancel;
            }
            m_ContactEdit.Contact = (listView.SelectedItem as Contact).DeepClone();
            m_ContactEdit.ShowDialog();
        }

        private void DetailedEdit_RequestSave(object sender, EventArgs e)
        {
            var contact = listView.SelectedItem as Contact;
            UndoRedo.PerformAction("edit " + m_ContactEdit.Contact.Name, new[] { contact }, new[] { m_ContactEdit.Contact });
            SelectContact(m_ContactEdit.Contact);
            m_ContactEdit.Close();
            m_ContactEdit = null;
            listView.Focus();
        }

        private void DetailedEdit_RequestCancel(object sender, EventArgs e)
        {
            m_ContactEdit.Close();
            m_ContactEdit = null;
            listView.Focus();
        }
        
        private void New_Click(object sender, RoutedEventArgs e)
        {
            if(CanCloseFile()) LoadNewAddressBook("");
        }

        private void FileOpen_Click(object sender, RoutedEventArgs e)
        {
            DoOpenFileAction("AddressBook.xml", "xml", LoadNewAddressBook);
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            Save();
        }

        private void SaveAs_Click(object sender, RoutedEventArgs e)
        {
            SaveAs(Settings.Default.FilePath);
        }

        private void Save()
        {
            if(Settings.Default.FilePath == "")
                SaveAs(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "AddressBook.xml"));
            else
                SaveAddressBook(Settings.Default.FilePath, this.m_AddressBook);
        }

        private void SaveAs(string file)
        {
            DoSaveFileAction(file, "xml", path => SaveAddressBook(path, m_AddressBook));
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Options_Click(object sender, RoutedEventArgs e)
        {
        }

        private void Birthdays_Click(object sender, RoutedEventArgs e)
        {
            BirthdaysAlert();
        }

        private void LoadNewAddressBook(string filePath)
        {
            SetFilePath(filePath);
            LoadDefaultAddressBook();
        }

        private void LoadDefaultAddressBook()
        {
            if(Settings.Default.FilePath == "" || ! File.Exists(Settings.Default.FilePath))
                m_AddressBook = new AddressBook();
            else
                m_AddressBook = ImportExport.LoadFromXml(Settings.Default.FilePath);
            DataContext = m_AddressBook;
            UndoRedo = new UndoRedo(m_AddressBook);
            listView.Focus();

            Title = GetTitle(Settings.Default.FilePath);
            if(Settings.Default.BirthdaysOnStartup) BirthdaysAlert();
        }

        private static string GetTitle(string path)
        {
            if(path == "") return "Contact Manager: [Unnamed]";
            return "Contact Manager: " + System.IO.Path.GetFileNameWithoutExtension(path);
        }

        private void SetFilePath(string path)
        {
            Settings.Default.FilePath = path;
            Title = GetTitle(path);
        }

        private void BirthdaysAlert()
        {
            if(this.m_AddressBook.Contacts.Count == 0) return; //don't bother if new!
            var bdays = new Birthdays(this.m_AddressBook);
            bdays.Owner = this;
            bdays.ShowDialog();
        }

        private void SaveAddressBook(string path, AddressBook addressbook)
        {
            if(File.Exists(path) && addressbook.IsDirty())
            {
                //Only backup file if it isn't already a backup file!
                if(!Utilities.IsBackupFile(path))
                {
                    var bakFileName = Utilities.GetBackupFileName(path);
                    File.Move(path, bakFileName);
                    File.SetAttributes(bakFileName, File.GetAttributes(bakFileName) | FileAttributes.ReadOnly);
                }
            }
            ImportExport.SaveToXml(path, addressbook);
            SetFilePath(path);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if(CanCloseFile())
                Settings.Default.Save();
            else
                e.Cancel = true;
        }

        private bool CanCloseFile()
        {
            if(this.m_AddressBook.IsDirty())
            {
                var result = MessageBox.Show("Do you want to save changes to your address book?", "Unsaved changes", MessageBoxButton.YesNoCancel);
                if(result == MessageBoxResult.Cancel) return false;
                if(result == MessageBoxResult.Yes) Save();
            }
            return true;
        }

        private void ImportGenericCsv_Click(object sender, RoutedEventArgs e)
        {
            ImportCSV(ImportExport.LoadFromGenericCsv);
        }

        private void ImportOutlookCsv_Click(object sender, RoutedEventArgs e)
        {
            ImportCSV(ImportExport.LoadFromOutlookCsv);
        }

        private void ImportAddressBook_Click(object sender, RoutedEventArgs e)
        {
            Action<string> ImportAction = (path) =>
            {
                Mouse.OverrideCursor = Cursors.Wait;
                var newdata = ImportExport.LoadFromXml(path);
                UndoRedo.PerformAction("import AddressBook xml", null, newdata.Contacts);
                listView.SelectedIndex = 0;
                listView.Focus();
                Mouse.OverrideCursor = null;
            };
            DoOpenFileAction("AddressBook.xml", "xml", ImportAction);
        }

        private void ImportCSV(Func<string, AddressBook> importer)
        {
            Action<string> ImportAction = (path) =>
                {
                    Mouse.OverrideCursor = Cursors.Wait;
                    var newdata = importer(path);
                    UndoRedo.PerformAction("import CSV", null, newdata.Contacts);
                    listView.SelectedIndex = 0;
                    listView.Focus();
                    Mouse.OverrideCursor = null;
                };
            DoOpenFileAction("", "csv", ImportAction);
        }


        private void ExportAddressBook_Click(object sender, RoutedEventArgs e)
        {
            DoSaveFileAction("", "xml", path => ImportExport.SaveToXml(path, GetFilteredContacts()));
        }

        private void ExportPhoneCsv_Click(object sender, RoutedEventArgs e)
        {
            DoSaveFileAction("", "csv", path => ImportExport.ExportPhoneList(path, GetFilteredContacts()));
        }

        private void ExportEmailCsv_Click(object sender, RoutedEventArgs e)
        {
            DoSaveFileAction("", "csv", path => ImportExport.ExportEmailList(path, GetFilteredContacts()));
        }

        private void ExportAddressCsv_Click(object sender, RoutedEventArgs e)
        {
            DoSaveFileAction("", "csv", path => ImportExport.ExportAddressList(path, GetFilteredContacts()));
        }

        private void ExportBirthdayCsv_Click(object sender, RoutedEventArgs e)
        {
            DoSaveFileAction("", "csv", path => ImportExport.ExportBirthdayList(path, GetFilteredContacts()));
        }

        private IEnumerable<Contact> GetFilteredContacts()
        {
            if(FilterOn)
                return m_AddressBook.Contacts.Where(c => GetCanShow(c));
            else
                return m_AddressBook.Contacts;
        }

        /// <summary>
        /// Get save file location for requested file type, if successful, perform SaveFile action
        /// </summary>
        private void DoSaveFileAction(string defaultname, string exttype, Action<string> SaveFile)
        {
            var filter = "";
            if(exttype == "csv") filter = "Semi-colon separated files (.csv)|*.csv";
            if(exttype == "xml") filter = "Xml addressbook file (.xml)|*.xml";

            var dialog = new System.Windows.Forms.SaveFileDialog { DefaultExt = "." + exttype, Filter = filter, FileName = defaultname, AutoUpgradeEnabled=true };
            var result = dialog.ShowDialog();
            if(result == System.Windows.Forms.DialogResult.OK) SaveFile(dialog.FileName);
        }

        private void DoOpenFileAction(string defaultname, string exttype, Action<string> OpenFile)
        {
            var filter = "";
            if(exttype == "csv") filter = "Semi-colon separated files (.csv)|*.csv";
            if(exttype == "xml") filter = "Xml addressbook file (.xml)|*.xml";
            
            var dialog = new System.Windows.Forms.OpenFileDialog { DefaultExt = "." + exttype, Filter = filter, FileName = defaultname, CheckFileExists = true, AutoUpgradeEnabled = true };
            var result = dialog.ShowDialog();
            if(result == System.Windows.Forms.DialogResult.OK) OpenFile(dialog.FileName);
        }
        //for later: http://joshsmithonwpf.wordpress.com/2007/06/12/searching-for-items-in-a-listbox/
        private void txtFilter_KeyUp(object sender, KeyEventArgs e)
        {
            if(txtFilter.Text.Length > 2)
            {
                FilterOn = true;
                listView.Items.Filter = new Predicate<object>(GetCanShow);
                if(listView.Items.PassesFilter(listView.SelectedItem))
                    listView.ScrollIntoView(listView.SelectedItem);
                else if(listView.Items.Count > 0) //false if nothing passes filter...
                    listView.ScrollIntoView(listView.Items[0]);
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
            Contact contact = item as Contact;
            bool show = false;
            if(contact != null)
                show = (contact.AllText.IndexOf(txtFilter.Text, StringComparison.InvariantCultureIgnoreCase) >= 0);
            return show;
        }

        private void btnClearFilter_Click(object sender, RoutedEventArgs e)
        {
            txtFilter.Text = "";
            listView.Items.Filter = null;
            FilterOn = false;
        }

        private void txtEdit_KeyUp(object sender, KeyEventArgs e)
        {
            e.Handled = true; //stop bubble to form handler http://msdn.microsoft.com/en-us/library/ms742806.aspx
        }

        private void Undo_Click(object sender, RoutedEventArgs e)
        {
            var contact = UndoRedo.UndoAction();
            if(contact != null) SelectContact(contact);
        }

        private void Redo_Click(object sender, RoutedEventArgs e)
        {
            var contact = UndoRedo.RedoAction();
            if(contact != null) SelectContact(contact);
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            var about = new HelpAbout();
            about.ShowDialog();
        }

        private void HelpDescription_Click(object sender, RoutedEventArgs e)
        {
            var help = new HelpDescription();
            help.ShowDialog();
        }

        private void this_KeyDown(object sender, KeyEventArgs e)
        {
            //if(e.Key == Key.LeftCtrl)
            //    this.Opacity = 0.3;
        }
    }
}
