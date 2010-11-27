using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.Collections.ObjectModel;

namespace ContactManager.Data
{
    [XmlRoot("AddressBook")]
    public class AddressBook
    {
        public AddressBook()
        {
            Contacts = new SortableObservableCollection<Contact>();
        }

        [XmlElement("Contacts")]
        public SortableObservableCollection<Contact> Contacts { get; set; }

        private bool m_IsDirty;
        public bool IsDirty()
        {
            return m_IsDirty;
        }
        public void ResetDirty()
        {
            m_IsDirty = false;
        }

        public void AddContact(Contact contact)
        {
            Contacts.Add(contact);
            Contacts.Sort();
            m_IsDirty = true;
        }

        //Note, this doesn't clone, so both lists will share items!
        public void AddContacts(IEnumerable<Contact> contacts)
        {
            if(contacts == null) return;
            contacts.ToList().ForEach(c => Contacts.Add(c));
            Contacts.Sort();
            m_IsDirty = true;
        }

        internal void DeleteContact(Contact contact)
        {
            Contacts.Remove(contact);
            m_IsDirty = true;
        }

        internal void DeleteContacts(IEnumerable<Contact> contacts)
        {
            if(contacts == null) return;
            contacts.ToList().ForEach(c => Contacts.Remove(c));
            m_IsDirty = true;
        }

        internal IEnumerable<Person> GetUpcomingBirthdays(int daysahead, bool notable)
        {
            if(notable)
                return GetBirthdaysFrom(DateTime.Now, daysahead).Where(p => (p.Age.Value + 1) % 10 == 0);
            else
                return GetBirthdaysFrom(DateTime.Now, daysahead);
        }

        //This overload to allow tests!
        internal IEnumerable<Person> GetBirthdaysFrom(DateTime startdate, int numberOfDays)
        {
            return from contact in Contacts
                   from person in contact.People
                   where person.DateOfBirth.HasValue
                   let dob = person.DateOfBirth.Value
                   let normdate = NormalizeDate(startdate, dob)
                   where (normdate - startdate).Days >= 0 && (normdate - startdate).Days <= numberOfDays
                   orderby normdate
                   select person;
        }

        //aim is to normalize the year of target date so that it is in the 12 months following the reference date
        private DateTime NormalizeDate(DateTime reference, DateTime target)
        {
            int adjust = 0;
            if(reference.Month > target.Month || (reference.Month == target.Month && reference.Day > target.Day)) adjust = 1;
            return target.AddYears((reference.Year - target.Year) + adjust);
        }
    }
}
