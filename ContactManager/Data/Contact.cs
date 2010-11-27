using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.ComponentModel;

namespace ContactManager.Data
{
    public class Contact : IComparable<Contact>, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        public Contact()
        {
            People = new SortableObservableCollection<Person>();
            m_Name = "";
            Address = "";
            PhoneNumber = "";
            Email = "";
            Notes = "";
            FirstNames = "";
        }

        //Not sure if this is good
        private bool m_IsBusiness;
        [XmlAttribute("IsBusiness")]
        public bool IsBusiness
        {
            get { return m_IsBusiness; }
            set
            {
                m_IsBusiness = value;
                PropertyChanged(this, new PropertyChangedEventArgs("DisplayName"));
            }
        }
        [XmlAttribute("FirstNames")]
        public string FirstNames { get; set; } //auto-generated - not for a business

        string m_Name;
        [XmlAttribute("Name")]
        public string Name
        {
            get { return m_Name; }
            set { SetName(value); }
        }

        [XmlElement("Address")]
        public string Address { get; set; }
        [XmlAttribute("PhoneNumber")]
        public string PhoneNumber { get; set; }
        [XmlAttribute("Email")]
        public string Email { get; set; }
        [XmlElement("Notes")]
        public string Notes { get; set; }

        //Read-only derived properties
        public string OrderByName { get { return IsBusiness ? (Name ?? "") : Name + ", " + FirstNames; } } //what if Names is empty?
        public string DisplayName { get { return IsBusiness ? (Name ?? "") : FirstNames.RightPadIf() + Name; } }
        public string AllText 
        { 
            get 
            {
                var peopleText = String.Join(" ", People.Select(p => p.AllText).ToArray());
                return String.Concat(
                    StringX.RightPadIf(Name), 
                    StringX.RightPadIf(Address), 
                    StringX.RightPadIf(PhoneNumber), 
                    StringX.RightPadIf(Email), 
                    StringX.RightPadIf(Notes),
                    peopleText); 
            } 
        }

        [XmlArray("People"),
        XmlArrayItem("Person")]
        public ObservableCollection<Person> People { get; set; } 

        //Gives us a bit more control (though doesn't prevent people from accessing People collection directly)
        public void AddPerson(Person person)
        {
            People.Add(person);
            //m_People.Sort();
        }

        //TODO: handle case where people don't have the default last name
        //TODO: work out how and when to use this!
        public void UpdateFirstNames()
        {
            FirstNames = IsBusiness ? "" : GenerateFirstNames(People);
            PropertyChanged(this, new PropertyChangedEventArgs("DisplayName"));
        }

        public static Contact QuickEdit(Contact oldcontact, string text)
        {
            var newcontact = Contact.ParseFromText(text);
            return MergeContacts(new Contact[] { newcontact, oldcontact }); //in merge contact, the first one wins...
        }

        public Contact ShallowClone()
        {
            return new Contact 
            {
                Name = this.Name,
                Email = this.Email,
                Address = this.Address,
                IsBusiness = this.IsBusiness,
                Notes = this.Notes,
                PhoneNumber = this.PhoneNumber,
                FirstNames = this.FirstNames,
            };
        }

        public Contact DeepClone()
        {
            var contact = this.ShallowClone();
            People.ToList().ForEach(p => contact.AddPerson(p.Clone()));
            return contact;
        }
        
        //Parse a string into a contact - expected format:
        //FirstName, FirstName & First-Name Last Name
        //Address Line    --> not first line, not email, not phone = address
        //Email@email.com --> contains @ = email
        //012 4032 503    --> all numbers = phone number
        public static Contact ParseFromText(string text)
        {
            if(text == null || text == "") throw new ArgumentNullException("No string, no contact");
            var contact = new Contact();
            var lines = Regex.Split(text, Environment.NewLine);
            ParsePeople(contact, lines[0]);

            //Get email, address, phone number (pretty basic, but if user plays nice...)
            var address = "";
            foreach(var line in lines.Skip(1))
            {
                if(line.Trim() == "") continue;
                if(line.Contains("@")) 
                    contact.Email = line.Trim();
                else if(Regex.IsMatch(line, @"^[\d .+]*$")) //spaces, numbers, plus or dots only
                    contact.PhoneNumber = line.Trim();
                else if(address == "")
                    address = line;
                else
                    address = address + Environment.NewLine + line;
            }
            contact.Address = address;

            return contact;
        }

        internal static List<Contact> SplitContact(Contact contact)
        {
            if(contact.People.Count <= 1) return new List<Contact> { contact };
            var splitList = new List<Contact>();
            foreach(var person in contact.People)
            {
                var newcontact = contact.ShallowClone();
                person.IncludeInDisplayName = true;
                newcontact.AddPerson(person); // don't need to clone, we delete parent after anyway
                newcontact.UpdateFirstNames();
                splitList.Add(newcontact);
            }
            return splitList;
        }

        //Very dumb algorithm, we take the first email/address found, and ignore others (Notes are concatenated)
        //For persons, we do check and concatenate properties if duplicate first names found
        internal static Contact MergeContacts(IEnumerable<Contact> contacts)
        {
            var newContact = new Contact();
            foreach(var contact in contacts)
            {
                newContact.Name = EitherOrBoth(newContact.Name, contact.Name);
                newContact.PhoneNumber = EitherOrBoth(contact.PhoneNumber, newContact.PhoneNumber);
                newContact.Email = EitherOrBoth(contact.Email, newContact.Email);

                //don't bother concatenating for address - first one wins
                if(newContact.Address == "") newContact.Address = contact.Address;

                //For notes, equivalent of EitherOrBoth, but with new line instead of hash
                if(newContact.Notes == "")
                    newContact.Notes = contact.Notes;
                else if (newContact.Notes != contact.Notes)
                    newContact.Notes = newContact.Notes + StringX.NewLineBeforeIf(contact.Notes);
                
                //Copy people
                foreach(var person in contact.People)
                {
                    if(newContact.People.Any(p => p.FirstName == person.FirstName))
                    {
                        //Person already exists, we just keep the most interesting bits
                        var existing = newContact.People.First(p => p.FirstName == person.FirstName);
                        existing.Email = EitherOrBoth(existing.Email, person.Email);
                        existing.LastName = EitherOrBoth(existing.LastName, person.LastName);
                        existing.MobileNumber = EitherOrBoth(existing.MobileNumber, person.MobileNumber);
                        existing.WorkNumber = EitherOrBoth(existing.WorkNumber, person.WorkNumber);
                        existing.DateOfBirth = existing.DateOfBirth == null ? person.DateOfBirth : existing.DateOfBirth;
                    }
                    else //person doensn't already exist
                    {
                        var clone = person.Clone();
                        //Copy contact email to person email if not same as main email
                        if(clone.Email == "" && contact.Email != newContact.Email)
                            clone.Email = contact.Email;
                        newContact.AddPerson(clone);
                    }
                }                
            }
            newContact.UpdateFirstNames();
            return newContact;
        }

        private static string EitherOrBoth(string one, string theother)
        {
            if(one == "") return theother; //one is empty, return the other
            if(theother == "") return one; //the other is empty, return one
            if(one == theother) return one; //both are the same, return one
            return one + "/" + theother; //different, return both, user will have to decide
        }

        //We don't handle the case where people don't have same last name
        internal static void ParsePeople(Contact contact, string displayName)
        {
            //difficult, we need to get people first, then last name, but then add last name to people
            string firstperson = @"^(?<First>[-\w]+)";
            string lastname = @"\s+(?<Last>.*)";
            string others = @"(?:(?:\s*[,|&]\s*)(?<Others>[-\w]+))*";

            var reg = new Regex(firstperson + others + lastname);
            var groups = reg.Match(displayName).Groups;
            contact.Name = groups["Last"].Value;
            contact.AddPerson(new Person { FirstName = groups["First"].Value, LastName = contact.Name, IncludeInDisplayName = true });
            foreach(Capture firstname in groups["Others"].Captures)
                contact.AddPerson(new Person { FirstName = firstname.Value, LastName = contact.Name, IncludeInDisplayName = true });
            contact.UpdateFirstNames();
        }

        //Cascades new name to people if not a business, and if the names are not already different
        private void SetName(string newName)
        {
            if(!IsBusiness)
            {
                foreach(var person in People)
                {
                    if(person.LastName == m_Name)
                        person.LastName = newName;
                }
            }
            m_Name = newName;
            PropertyChanged(this, new PropertyChangedEventArgs("DisplayName"));
        }

        private static string GenerateFirstNames(Collection<Person> people)
        {
            //http://stackoverflow.com/questions/788535/eric-lipperts-challenge-comma-quibbling-best-answer
            var titlePeople = people.Where(p => p.IncludeInDisplayName);
            switch(titlePeople.Count())
            {
                case 0: return "";
                case 1: return titlePeople.First().FirstName;
                default:
                    var allbutlast = titlePeople.Take(titlePeople.Count() - 1).Select(p => p.FirstName).ToArray();
                    return String.Join(", ", allbutlast) + " & " + titlePeople.Last().FirstName;
            }
        }

        public int CompareTo(Contact other)
        {
            return this.OrderByName.CompareTo(other.OrderByName);
        }
    }
}
