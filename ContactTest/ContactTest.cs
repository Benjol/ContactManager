using ContactManager.Data;
using System.Collections.Generic;
using System;
using System.Collections.ObjectModel;
using Xunit;

namespace ContactTest
{
    public class ContactTest
    {
        [Fact]
        public void ContactMainTest()
        {
            string address = "1 road name\npostcode\ntown";
            string email = "testmail@mail.com";
            bool isbusiness = false;
            string lastname = "Lastname";
            string notes = "some notes here";
            string phonenumber = "021 9551 684 6";

            string contact1firstname = "Horrace";
            string worknumber = "032 568 79 85";
            string contact2email = "testing@test2.com";

            Contact target = new Contact()
            {
                Address = address,
                Email = email,
                IsBusiness = isbusiness,
                Name = lastname,
                Notes = notes,
                PhoneNumber = phonenumber,
            };
            target.AddPerson(new Person { DateOfBirth = new DateTime(1968, 5, 3), FirstName = contact1firstname, LastName = "Lastname", MobileNumber= "079 813 83 53", WorkNumber = worknumber, Email = "contact1@mail.com", IncludeInDisplayName = true });
            target.AddPerson(new Person { DateOfBirth = new DateTime(1984, 7, 13), FirstName = "Contact1", LastName = "Lastname", MobileNumber = "079 887 35 00", Email = contact2email, IncludeInDisplayName = false });

            Assert.Equal(target.Address, address);
            Assert.Equal(target.Email, email);
            Assert.Equal(target.IsBusiness, isbusiness);
            Assert.Equal(target.Name, lastname);
            Assert.Equal(target.Notes, notes);
            Assert.Equal(target.PhoneNumber, phonenumber);
            Assert.Equal(target.People[0].WorkNumber, worknumber);

            Assert.Equal(target.People.Count, 2);
            Assert.Equal(target.People[0].FirstName, contact1firstname);
            Assert.Equal(target.People[1].Email, contact2email);
        }

        /// <summary>
        ///A test for GenerateFirstName
        ///</summary>
        [Fact]
        public void GenerateFirstNameTest()
        {
            Contact contact = new Contact { Name = "Todd", FirstNames = "Test business", };
            contact.AddPerson(new Person { FirstName="John", IncludeInDisplayName=true});
            contact.AddPerson(new Person { FirstName="Anne", IncludeInDisplayName=true});
            contact.AddPerson(new Person { FirstName="David", IncludeInDisplayName=true});

            //Test - 3 people included
            string expected = "John, Anne & David";
            contact.UpdateFirstNames();
            Assert.Equal(expected, contact.FirstNames);
            //Test - 2 people included
            contact.People[2].IncludeInDisplayName = false;
            expected = "John & Anne";
            contact.UpdateFirstNames();
            Assert.Equal(expected, contact.FirstNames);
            //Test - just one person included
            contact.People[1].IncludeInDisplayName = false;
            expected = "John";
            contact.UpdateFirstNames();
            Assert.Equal(expected, contact.FirstNames);
            //Test - error if no people included
            contact.People[0].IncludeInDisplayName = false;
            //ExceptionAssert.Throws<ArgumentException>(() => contact.UpdateFirstNames());
            //Test - if business, return Display name directly
            contact.IsBusiness = true;
            expected = "";
            contact.UpdateFirstNames();
            Assert.Equal(expected, contact.FirstNames);
        }

        /// <summary>
        ///A test for OrderByName
        ///</summary>
        [Fact]
        public void OrderByNameDisplayNameTest()
        {
            Contact contact = new Contact { Name = "Todd", FirstNames = "Test business", };
            contact.AddPerson(new Person { FirstName = "John", IncludeInDisplayName = true });
            contact.AddPerson(new Person { FirstName = "Anne", IncludeInDisplayName = true });
            contact.AddPerson(new Person { FirstName = "David", IncludeInDisplayName = true });

            //Test - 3 people included
            string expected = "John, Anne & David Todd";
            contact.UpdateFirstNames();
            Assert.Equal(expected, contact.DisplayName);
            expected = "Todd, John, Anne & David";
            Assert.Equal(expected, contact.OrderByName);
            //Test - 2 people included
            contact.People[2].IncludeInDisplayName = false;
            expected = "John & Anne Todd";
            contact.UpdateFirstNames();
            Assert.Equal(expected, contact.DisplayName);
            expected = "Todd, John & Anne";
            Assert.Equal(expected, contact.OrderByName);
            //Test - just one person included
            contact.People[1].IncludeInDisplayName = false;
            expected = "John Todd";
            contact.UpdateFirstNames();
            Assert.Equal(expected, contact.DisplayName);
            expected = "Todd, John";
            Assert.Equal(expected, contact.OrderByName);
            //Test - if business, return Display name directly
            contact.IsBusiness = true;
            expected = contact.Name;
            contact.UpdateFirstNames();
            Assert.Equal(expected, contact.DisplayName);
            Assert.Equal(expected, contact.OrderByName);
        }


        /// <summary>
        ///A test for ParseFromText
        ///</summary>
        [Fact]
        public void ParseFromTextTest()
        {
            string text = "David, Karen & Gary da Costa\r\nOn the Mead 4\r\nTrimbley Down\r\nNT4 3FK\r\nAngleterre\r\n+44 45.68.81.56\r\nmail@test.com";
            Contact actual = Contact.ParseFromText(text);
            Assert.Equal("mail@test.com", actual.Email);
            Assert.Equal("+44 45.68.81.56", actual.PhoneNumber);
            Assert.Equal("da Costa", actual.Name);
            Assert.Equal("David, Karen & Gary da Costa", actual.DisplayName);
            Assert.Equal("da Costa", actual.People[1].LastName);
            Assert.Equal("Gary", actual.People[2].FirstName);
            Assert.Equal("On the Mead 4\r\nTrimbley Down\r\nNT4 3FK\r\nAngleterre", actual.Address);
        }

        /// <summary>
        ///A test for ParsePeople
        ///</summary>
        [Fact]
        public void ParsePeopleTest()
        {
            Contact contact = new Contact();
            string displayName = "Peter-Paul, Mary & Joël Van der Winkel"; 
            Contact.ParsePeople(contact, displayName);
            Assert.Equal("Peter-Paul", contact.People[0].FirstName);
            Assert.Equal("Van der Winkel", contact.Name);
            Assert.Equal(displayName, contact.DisplayName);
        }


        /// <summary>
        ///A test for ParsePeople
        ///</summary>
        [Fact]
        public void ParsePeopleTest2()
        {
            Contact contact = new Contact();
            string displayName = "Peter-Paul Van der Winkel";
            Contact.ParsePeople(contact, displayName);
            Assert.Equal("Peter-Paul", contact.People[0].FirstName);
            Assert.Equal("Van der Winkel", contact.Name);
            Assert.Equal(displayName, contact.DisplayName);
        }

        /// <summary>
        ///A test for ParsePeople
        ///</summary>
        [Fact]
        public void ParsePeopleTest3()
        {
            Contact contact = new Contact();
            string displayName = "Peter-Paul&Joseph Van der Winkel";
            Contact.ParsePeople(contact, displayName);
            Assert.Equal("Peter-Paul", contact.People[0].FirstName);
            Assert.Equal("Van der Winkel", contact.Name);
            Assert.Equal("Peter-Paul & Joseph Van der Winkel", contact.DisplayName);
        }

        /// <summary>
        ///A test for ParsePeople
        ///</summary>
        [Fact]
        public void ParsePeopleTest4()
        {
            Contact contact = new Contact();
            string displayName = "Peter-Paul  &  Joseph  Van der Winkel";
            Contact.ParsePeople(contact, displayName);
            Assert.Equal("Peter-Paul", contact.People[0].FirstName);
            Assert.Equal("Van der Winkel", contact.Name);
            Assert.Equal("Peter-Paul & Joseph Van der Winkel", contact.DisplayName);
        }

        /// <summary>
        ///A test for MergeContacts
        ///</summary>
        [Fact]
        public void MergeContactsTest()
        {
            var contact1 = new Contact
            {
                Address = "Address",
                Email = "email@meail.com",
                IsBusiness = false,
                Name = "Name",
                PhoneNumber = "012 3209 2 93",
                Notes = "Notes 1"
            };
            contact1.AddPerson(new Person { FirstName = "Duplicate", Email="", LastName="person1name", MobileNumber = "mobile1a", WorkNumber="work1a", DateOfBirth=new DateTime(1964, 12, 1) });
            contact1.AddPerson(new Person { FirstName = "Marjory", Email="person2mail", LastName="person2name", MobileNumber = "mobile2", WorkNumber="work2", DateOfBirth=new DateTime(1968, 12, 1) });

            var contact2 = new Contact
            {
                Address = "Address2",
                Email = "email2@meail.com",
                IsBusiness = false,
                Name = "Name",
                PhoneNumber = "012 3209 2 93",
                Notes = "Notes 2"
            };
            contact2.AddPerson(new Person { FirstName = "Duplicate", Email = "person1email", LastName = "", MobileNumber = "mobile1b", WorkNumber = "work1b" });

            var contact3 = new Contact
            {
                Address = "Address3",
                Email = "email3@meail.com",
                IsBusiness = false,
                Name = "Name",
                PhoneNumber = "012 3209 2 93",
                Notes = "Notes 3"
            };
            contact3.AddPerson(new Person { FirstName = "Herny", Email = "person3mail", LastName = "person3name", MobileNumber = "mobile3", WorkNumber = "work3", DateOfBirth = new DateTime(1966, 12, 1) });

            var contacts = new List<Contact> { contact1, contact2, contact3 };

            var actual = Contact.MergeContacts(contacts);
            Assert.Equal(contact1.Address, actual.Address);
            Assert.Equal(contact1.Name, actual.Name);
            Assert.Equal("Notes 1\r\nNotes 2\r\nNotes 3", actual.Notes);
            Assert.Equal(3, actual.People.Count);
            Assert.Equal(contact1.People[1].FirstName, actual.People[1].FirstName);
            Assert.Equal(contact3.People[0].FirstName, actual.People[2].FirstName);
            Assert.Equal(contact2.People[0].Email, actual.People[0].Email);
            Assert.Equal(contact1.People[0].LastName, actual.People[0].LastName);
            Assert.Equal("mobile1a/mobile1b", actual.People[0].MobileNumber);
            Assert.Equal(new DateTime(1964, 12, 1), actual.People[0].DateOfBirth);
        }
    }
}
