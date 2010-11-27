using ContactManager.Data;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using Xunit;

namespace ContactTest
{   
    public class PersonTest
    {
        [Fact]
        public void PersonMainTest()
        {
            DateTime dob = new DateTime(1970, 11, 3);
            string email = "mail@mail.com";
            string firstname = "firstname";
            bool includeindisplayname = true;
            string lastname = "lastname";
            string mobilenumber = "079 846 23 83";
            string worknumber = "056 897 85 68";

            Person target = new Person()
            {
                DateOfBirth = dob,
                Email = email,
                FirstName = firstname,
                IncludeInDisplayName = includeindisplayname,
                LastName = lastname,
                MobileNumber = mobilenumber,
                WorkNumber = worknumber
            };
            Assert.Equal(target.DateOfBirth, dob);
            Assert.Equal(target.Email, email);
            Assert.Equal(target.FirstName, firstname);
            Assert.Equal(target.IncludeInDisplayName, includeindisplayname);
            Assert.Equal(target.LastName, lastname);
            Assert.Equal(target.MobileNumber, mobilenumber);
            Assert.Equal(target.WorkNumber, worknumber);
        }
    }
}
