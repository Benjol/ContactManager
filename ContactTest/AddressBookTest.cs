using ContactManager.Data;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Collections.ObjectModel;
using Xunit;
using System.IO;

namespace ContactTest
{
    public class AddressBookTest
    {
        [Fact]
        public void AddressesTest()
        {
            string address1 = "1 road name\r\npostcode\r\ntown";
            string displayname1 = "Contact display name";
            string email1 = "testmail@mail.com";
            bool isbusiness1 = false;
            string lastname1 = "Lastname";
            string notes1 = "some notes here";
            string phonenumber1 = "021 9551 684 6";
            string workphone = "044 56 89 79";
            string contact1firstname = "Horrace";
            string contact2email = "testing@test2.com";

            //Create an address book in memory
            AddressBook target = new AddressBook(); // TODO: Initialize to an appropriate value
            target.Contacts.Add(new Contact()
            {
                Address = address1,
                FirstNames = displayname1,
                Email = email1,
                IsBusiness = isbusiness1,
                Name = lastname1,
                Notes = notes1,
                PhoneNumber = phonenumber1
            });

            target.Contacts[0].AddPerson(new Person { DateOfBirth = new DateTime(1968, 5, 3), FirstName = contact1firstname, LastName = "Lastname", MobileNumber= "079 813 83 53", Email = "contact1@mail.com", IncludeInDisplayName = true });
            target.Contacts[0].AddPerson(new Person { FirstName = "Contact1", LastName = "Lastname", MobileNumber = "079 887 35 00", Email = contact2email, IncludeInDisplayName = false, WorkNumber = workphone });

            string address2 = "3 rue d'aubervilliers\r\ncode postale\r\nville\r\npays";
            string displayname2 = "Company display name";
            string email2 = "company@mail.com";
            bool isbusiness2 = true;
            string lastname2 = "";
            string notes2 = "";
            string phonenumber2 = "08800 9551 684 6";

            string contact3lastname = "Dimbleby";
            string contact4email = "testing@company.com";
            
            target.Contacts.Add(new Contact()
            {
                Address = address2,
                FirstNames = displayname2,
                Email = email2,
                IsBusiness = isbusiness2,
                Name = lastname2,
                Notes = notes2,
                PhoneNumber = phonenumber2
            });
            
            target.Contacts[1].AddPerson(new Person { DateOfBirth = new DateTime(1968, 5, 3), FirstName = "Contact2", LastName = contact3lastname, MobileNumber= "079 813 83 53", Email = "contact1@mail.com", IncludeInDisplayName = true });
            target.Contacts[1].AddPerson(new Person { DateOfBirth = new DateTime(1984, 7, 13), FirstName = "Contact4", LastName = "Critchely", MobileNumber= "079 887 35 00", Email = contact4email, IncludeInDisplayName = false });

            //Serialize to file
            string filepath = Path.GetTempFileName();
            ImportExport.SaveToXml(filepath, target);

            //Deserialize
            var test = ImportExport.LoadFromXml(filepath);

            //Check same
            Assert.Equal(target.Contacts.Count, test.Contacts.Count);
            Assert.Equal(target.Contacts[0].Address, test.Contacts[0].Address);
            Assert.Equal(target.Contacts[1].People.Count, test.Contacts[1].People.Count);
            Assert.Equal(target.Contacts[1].People.Count, test.Contacts[1].People.Count);
            Assert.Equal(target.Contacts[0].FirstNames, test.Contacts[0].FirstNames);
            Assert.Equal(workphone, test.Contacts[0].People[1].WorkNumber);
            Assert.Equal(contact1firstname, test.Contacts[0].People[0].FirstName);
            Assert.Equal(contact2email, test.Contacts[0].People[1].Email);
            Assert.Equal(contact3lastname, test.Contacts[1].People[0].LastName);
            Assert.Equal(contact4email, test.Contacts[1].People[1].Email);
            Assert.Equal(null, test.Contacts[0].People[1].DateOfBirth);
            Assert.Equal(13, test.Contacts[1].People[1].DateOfBirth.Value.Day);

            //Cleanup
            File.Delete(filepath);
        }

        [Fact]
        public void GetBirthdaysFromTest()
        {
            AddressBook target = new AddressBook(); // TODO: Initialize to an appropriate value
            var contact = new Contact();
            contact.AddPerson(new Person { FirstName = "Bob", LastName = "Smith", DateOfBirth = new DateTime(1972, 12, 05) });
            contact.AddPerson(new Person { FirstName = "Jim", LastName = "Smith" });
            target.Contacts.Add(contact);
            contact = new Contact();
            contact.AddPerson(new Person { FirstName = "Dave", LastName = "Jones", DateOfBirth = new DateTime(1938, 05, 12) });
            contact.AddPerson(new Person { FirstName = "Kate", LastName = "Jones", DateOfBirth = new DateTime(1965, 08, 09) });
            contact.AddPerson(new Person { FirstName = "Sarah", LastName = "Jones", DateOfBirth = new DateTime(2001, 01, 15) });
            target.Contacts.Add(contact);
            

            //int daysahead = 60;
            //IEnumerable<string> expected = new List<string> { "Bob Smith - 05 déc. 1972", "Sarah Jones - 15 janv. 2001" };
            //IEnumerable<string> actual = target.GetBirthdaysFrom(new DateTime(2009, 11, 25), daysahead);
            //Assert.IsTrue(expected.SequenceEqual(actual));

            //expected = new List<string> { "Dave Jones - 12 mai 1938" };
            //actual = target.GetBirthdaysFrom(new DateTime(2010, 04, 15), 30);
            //Assert.IsTrue(expected.SequenceEqual(actual));
        }
    }
}
