using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ContactManager.Data
{
    internal static class ImportTools
    {
        /// <summary>
        /// Gets the address book from CSV dictionary.
        /// </summary>
        //(This would be private, but test generation wouldn't work...)
        internal static AddressBook GetAddressBookFromLookup(IEnumerable<Dictionary<string, string>> csv)
        {
            AddressBook book = new AddressBook();
            foreach(var row in csv)
            {
                bool isbusiness = (row[Keys.HomePhone] == "" && row[Keys.WorkPhone] != ""); //is business if no home phone AND has business phone
                bool multiperson = row[Keys.FirstName].Contains('&');
                var contact = new Contact
                {
                    Name = row[Keys.LastName],
                    Email = ContactMailLogic(row, multiperson),
                    IsBusiness = isbusiness,
                    PhoneNumber = isbusiness ? row[Keys.WorkPhone] : row[Keys.HomePhone],
                    Address = GetAddress(row),
                    Notes = row[Keys.Notes]
                };
                //Add one or two people, based on first name containing &
                if(multiperson)
                {
                    contact.AddPerson(FirstPerson(row, false));
                    contact.AddPerson(SecondPerson(row));
                }
                else
                {
                    contact.AddPerson(FirstPerson(row, true));
                }

                //Update first names with new people
                contact.UpdateFirstNames();
                book.AddContact(contact);
            }

            return book;
        }

        internal static string ContactMailLogic(Dictionary<string, string> row, bool ismultiperson)
        {
            if(ismultiperson) 
            {
                //if two mails exist, we consider that they are for the two people, so contact mail is empty, else it is shared mail
                if(row[Keys.Email2] != "")
                    return "";
                else
                    return row[Keys.Email1];
            }
            else
            {   
                //if one user, main email goes in contact, secondary in person
                return row[Keys.Email1];
            }
        }

        //(This would be private, but test generation wouldn't work...)
        internal static string GetAddress(Dictionary<string, string> row)
        {
            //Need to differentiate address types - simplify by detecting non-numeric characters in postal code
            int dummy;

            if(row[Keys.Country].ToUpper().StartsWith("US"))
            {
                //US - different format
                return StringX.NewLineAfterIf(row[Keys.Street1])
                    + StringX.NewLineAfterIf(row[Keys.Street2])
                    + StringX.NewLineAfterIf(row[Keys.City] + StringX.LeftPadIf(row[Keys.State]) + StringX.LeftPadIf(row[Keys.PostCode]))
                    + row[Keys.Country];
            }
            else if(Int32.TryParse(row[Keys.PostCode], out dummy)) //DOES EMPTY POSTAL CODE (ireland) COME HERE?
            {
                //FRANCE/SWITZERLAND/ITALY
                    return StringX.NewLineAfterIf(row[Keys.Street1])
                    + StringX.NewLineAfterIf(row[Keys.Street2])
                    + StringX.NewLineAfterIf(row[Keys.PostCode] + StringX.LeftPadIf(row[Keys.City]) + StringX.LeftPadIf(row[Keys.State]))
                    + row[Keys.Country];
            }
            else //UK/CANANDA
            {
                return StringX.NewLineAfterIf(row[Keys.Street1])
                    + StringX.NewLineAfterIf(row[Keys.Street2])
                    + StringX.NewLineAfterIf(row[Keys.City])
                    + StringX.NewLineAfterIf(row[Keys.PostCode])
                    + StringX.NewLineAfterIf(row[Keys.State])
                    + row[Keys.Country];
            }
        }

        //(This would be private, but test generation wouldn't work...)
        internal static Person FirstPerson(Dictionary<string, string> row, bool isOnly)
        {
            var firstname = isOnly ? row[Keys.FirstName] : row[Keys.FirstName].Split(new char[] { '&' }).First().TrimEnd();
            var email = isOnly ? row[Keys.Email2] : (row[Keys.Email2] == "" ? "" : row[Keys.Email1]);
            return new Person { FirstName = firstname, LastName = row[Keys.LastName], IncludeInDisplayName = true, Email = email, 
                MobileNumber = row[Keys.MobilePhone], WorkNumber = row[Keys.WorkPhone], DateOfBirth = GetDateOfBirth(row) };
        }

        internal static Person SecondPerson(Dictionary<string, string> row)
        {
            var firstname = row[Keys.FirstName].Split(new char[] { '&' }).Last().TrimStart();
            return new Person { FirstName = firstname, LastName = row[Keys.LastName], IncludeInDisplayName = true, Email = row[Keys.Email2],
                MobileNumber = row[Keys.OtherPhone], DateOfBirth = GetDateOfBirth(row) };
        }

        internal static DateTime? GetDateOfBirth(Dictionary<string, string> row)
        {
            DateTime? ans = null;
            DateTime tmp;
            //If birthdate is interpretable as a date
            if(row[Keys.BirthDate] != "") 
            {
                if(DateTime.TryParse(row[Keys.BirthDate], out tmp))
                    ans = tmp;
            }
            //if day, month and year are ints, and combined can be interpreted as a date
            else if(IsInt(row[Keys.BirthDay]) && IsInt(row[Keys.BirthMonth]) && IsInt(row[Keys.BirthYear]))
            {
                if(DateTime.TryParse(String.Format("{0}.{1}.{2}", row[Keys.BirthYear], row[Keys.BirthMonth], row[Keys.BirthDay]), out tmp))
                    ans = tmp;
            }
            return ans;
        }

        internal static bool IsInt(string s)
        {
            return Regex.IsMatch(s, @"\d+");
        }

        //text is csv text, separated by separator, bag is dictionary in which values will be returned, mapping provides correspondance from csv headers to internal headers
        internal static IEnumerable<Dictionary<string, string>> SelectCSV(string text, char sep, Dictionary<string, string> bag, Dictionary<string, string> mapping)
        {
            var lines = SelectLines(text, sep);
            var headers = GetFields(lines.First(), sep);
            var lookup = headers.Select((header, pos) => new { header, pos })  //gets tuple of header text + column number
                .Join(mapping, h => h.header, map => map.Key, (h, m) => new { m.Value, h.pos }) //Join with mapping to get tuple of internal header + pos (and eliminate unhandled csv headers)
                .ToDictionary(tup => tup.Value, tup => tup.pos); //this provides correspondance from internal header to pos in row

            foreach(var line in lines.Skip(1))
            {
                if(line == "") continue;
                var fields = GetFields(line, sep);
                foreach(var kvp in lookup)
                {
                    bag[kvp.Key] = fields[kvp.Value];
                }
                yield return bag;
            }
        }

        //IEnumerable returning one 'line' at a time - can manage lines that spread over several lines (with double quotes)
        internal static IEnumerable<string> SelectLines(string text, char sep)
        {
            var lines = Regex.Split(text, Environment.NewLine); //Warning, this could go pear-shaped on a mac or linux...
            bool multi = false;
            string cumul = "";
            for(int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                int quoteCount = line.ToCharArray().Count(c => c == '"');
                if(quoteCount % 2 > 0)
                {
                    //Odd number of quotes                 
                    if(multi)
                    {
                        yield return cumul + Environment.NewLine + line;
                        multi = false;
                        cumul = "";
                    }
                    else
                    {
                        multi = true;
                        cumul = line;
                    }
                }
                else
                {
                    //Even number of quotes
                    if(multi)
                        cumul = cumul + Environment.NewLine + line;
                    else
                        yield return line;
                }
            }
            if(multi) throw new Exception("Unfinished lines in csv file");
        }

        //Specific to our use - we don't expect separators in fields, nor escaped quotes
        //(This would be private, but test generation wouldn't work...)
        internal static string[] GetFields(string line, char sep)
        {
            char[] totrim = { '"', ' ' };
            return line.Split(sep).Select(col => col.Trim(totrim)).ToArray();
        }
    }

    static class Keys
    {
        public const string BirthDate = "BirthDate";
        public const string BirthDay = "BirthDay";
        public const string BirthMonth = "BirthMonth";
        public const string BirthYear = "BirthYear";
        public const string FirstName = "FirstName";
        public const string LastName = "LastName";
        public const string Street1 = "Street1";
        public const string Street2 = "Street2";
        public const string City = "City";
        public const string PostCode = "PostCode";
        public const string State = "State";
        public const string Country = "Country";
        public const string HomePhone = "HomePhone";
        public const string MobilePhone = "MobilePhone";
        public const string WorkPhone = "WorkPhone";
        public const string OtherPhone = "OtherPhone";
        public const string Email1 = "Email1";
        public const string Email2 = "Email2";
        public const string Notes = "Notes";

        private static List<string> m_InternalKeys =
            new List<string> { BirthDate, BirthDay, BirthMonth, BirthYear, 
                                FirstName, LastName,
                                Street1, Street2, City, PostCode, State, Country,
                                HomePhone, MobilePhone, WorkPhone, OtherPhone,
                                Email1, Email2,
                                Notes };

        public static Dictionary<string, string> EmptyBag
        {
            get { return m_InternalKeys.ToDictionary(k => k, k => ""); }
        }

        public static Dictionary<string, string> DefaultMap
        {
            get { return m_InternalKeys.ToDictionary(k => k, k => k); }
        }

        public static Dictionary<string, string> OutlookMap
        {
            get 
            { 
                return new Dictionary<string,string>
                {
                    { "First Name" , FirstName },
                    { "Last Name" , LastName },
                    { "Street" , Street1 },
                    { "City" , City },
                    { "State" , State },
                    { "Postal Code" , PostCode },
                    { "Country/Region" , Country },
                    { "Business Phone" , WorkPhone },
                    { "Home Phone" , HomePhone },
                    { "Mobile Phone" , MobilePhone },
                    { "Other Phone" , OtherPhone },
                    { "E-mail Address" , Email1 },
                    { "E-mail 2 Address" , Email2 },
                };
            }
        }
    }
}
