using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Runtime.CompilerServices;
using System.Xml;

namespace ContactManager.Data
{
    public static class ImportExport
    {
        private static char K_SEP = ';'; // hard coded for the moment, to simplify parsing imported csv // CultureInfo.CurrentCulture.TextInfo.ListSeparator[0];
        public static AddressBook LoadFromXml(string filepath)
        {
            var serializer = GetSerializer();
            AddressBook addressbook = null;
            using(var reader = new XmlTextReader(filepath))
            {
                //http://social.msdn.microsoft.com/Forums/en-US/asmxandxml/thread/612d09df-5ea6-4cd3-aa06-dafe55780bdf
                addressbook = serializer.Deserialize(reader) as AddressBook;
            }
            //Temp hack to make up for errors in previous encodings - remove after next version
            foreach(var contact in addressbook.Contacts)
                contact.Address = Regex.Replace(contact.Address, "\r\n|\n|\r", Environment.NewLine);
            //end temp hack
            return addressbook;
        }

        public static void SaveToXml(string filepath, AddressBook book)
        {
            var serializer = GetSerializer();
            using(var writer = new XmlTextWriter(filepath, Encoding.Default))
            {
                serializer.Serialize(writer, book);
            }
            book.ResetDirty();
        }

        //Save just contacts (for filtered export)
        public static void SaveToXml(string filepath, IEnumerable<Contact> contacts)
        {
            var serializer = GetSerializer();
            var book = new AddressBook();
            book.AddContacts(contacts);
            using(var writer = new XmlTextWriter(filepath, Encoding.Default))
            {
                serializer.Serialize(writer, book);
            }
        }

        private static XmlSerializer GetSerializer()
        {
            return new XmlSerializer(typeof(AddressBook));
        }

        /// <summary>
        /// Loads from CSV.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="PropertyToColHeader">Mapping from name of property to csv column header(s).</param>
        public static AddressBook LoadFromOutlookCsv(string filePath)
        {
            //hard coded version
            string text = SafeReadAllText(filePath, Encoding.Default);
            //http://stackoverflow.com/questions/1305401/encoding-default-is-not-the-same-as-no-encoding-in-file-readalltext
            var csv = ImportTools.SelectCSV(text, K_SEP, Keys.EmptyBag, Keys.OutlookMap);
            return ImportTools.GetAddressBookFromLookup(csv);
        }

        internal static AddressBook LoadFromGenericCsv(string filePath)
        {
            //hard coded version
            string text = SafeReadAllText(filePath, Encoding.Default);

            var csv = ImportTools.SelectCSV(text, K_SEP, Keys.EmptyBag, Keys.DefaultMap);
            return ImportTools.GetAddressBookFromLookup(csv);
        }
        
        internal static void ExportPhoneList(string path, IEnumerable<Contact> contacts)
        {
            var list = new List<string[]>();
            
            foreach(var contact in contacts)
            {
                //Contacts, and single people details
                var innerlist = new List<string> { contact.OrderByName };
                AddIf(innerlist, contact.PhoneNumber);
                if(contact.People.Count == 1 && !contact.IsBusiness)
                {
                    AddIf(innerlist, contact.People[0].MobileNumber);
                    AddIf(innerlist, contact.People[0].WorkNumber);
                }
                AddIf(list, innerlist);

                //Multi-people contacts, or businesses
                if(contact.People.Count > 1 || contact.IsBusiness)
                {
                    foreach(var person in contact.People.Where(p => p.MobileNumber != "" || p.WorkNumber != ""))
                    {
                        innerlist = new List<string> { person.OrderByName };
                        AddIf(innerlist, person.MobileNumber);
                        AddIf(innerlist, person.WorkNumber);
                        AddIf(list, innerlist);
                    }
                }
            }
            //Note that list is a jagged 'array', but excel can handle it, apparently
            ExportCSV(path, new string[] { "Name", "Phone1", "Phone2", "Phone3" }, list, CultureInfo.CurrentCulture.TextInfo.ListSeparator[0]);
        }

        internal static void AddIf(List<string> list, string value)
        {
            if(!String.IsNullOrEmpty(value)) list.Add(value);
        }
        internal static void AddIf(List<string[]> list, List<string> innerlist)
        {
            if(innerlist.Count > 1) list.Add(innerlist.ToArray());
        }

        internal static void ExportEmailList(string path, IEnumerable<Contact> contacts)
        {
            //Format = Name, Email
            var contacts2 = from contact in contacts
                           where contact.Email != ""
                           select new { OrderName = contact.OrderByName, Name = contact.OrderByName, Email = contact.Email, IsPerson = 0 };

            var people = from contact in contacts
                         from person in contact.People
                         where person.Email != ""
                         select new { OrderName = contact.OrderByName, Name = person.OrderByName, Email = person.Email, IsPerson = 1 };

            var list = contacts2.Concat(people).OrderBy(a => a.OrderName).ThenBy(a => a.IsPerson).Select(a => new string[] { a.Name, a.Email });

            ExportCSV(path, new string[] { "Name", "Email" }, list, CultureInfo.CurrentCulture.TextInfo.ListSeparator[0]);
        }

        internal static void ExportBirthdayList(string path, IEnumerable<Contact> contacts)
        {
            //Format = Month, Day, Birthday, Name
            var bdays = from contact in contacts
                        from person in contact.People
                        where person.DateOfBirth != null
                        let dob = person.DateOfBirth.Value
                        orderby dob.Month, dob.Day
                        select new string[] { dob.Month.ToString(), dob.Day.ToString(), dob.ToString("dd.MM.yyyy"), person.DisplayName };

            ExportCSV(path, new string[] { "Month", "Day", "Birthday", "Name" }, bdays, CultureInfo.CurrentCulture.TextInfo.ListSeparator[0]);
        }

        //This could be slow
        internal static void ExportAddressList(string path, IEnumerable<Contact> contacts)
        {
            //Start with dumb version:
            string quote = "\"";
            //Format = Name, Address
            var addresses = from contact in contacts
                            where contact.Address != ""
                            orderby contact.OrderByName
                            select new string[] { contact.OrderByName, (quote + contact.Address.Replace("\r", "") + quote) }; //just leave the \n in place.... ugly as hell...

            ExportCSV(path, new string[] { "Name", "Address" }, addresses, CultureInfo.CurrentCulture.TextInfo.ListSeparator[0]);
            //Format = Name, Address Line1, Address Line2, AddressLine3, PostCode, City, State, Country
        }

        internal static void ExportCSV(string path, string[] headers, IEnumerable<string[]> data, char separator)
        {
            string sep = separator.ToString();
            using(var file = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                using(var writer = new StreamWriter(file, System.Text.Encoding.Default)) //File.CreateText doesn't have encoding option
                {
                    writer.WriteLine(String.Join(sep, headers));
                    data.Select(a => String.Join(sep, a)).ToList().ForEach(l => writer.WriteLine(l));
                }
            }
        }

        private static string SafeReadAllText(string path, Encoding encoding)
        {
            string content = "";
            //http://stackoverflow.com/questions/1389155/easiest-way-to-read-text-file-which-is-locked-by-another-application
            using(var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using(var textReader = new StreamReader(fileStream, encoding))
            {
                content = textReader.ReadToEnd();
            }
            return content;
        }
    }
}
