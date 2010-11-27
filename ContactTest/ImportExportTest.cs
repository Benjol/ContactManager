using ContactManager.Data;
using System.Collections.Generic;
using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Diagnostics;
using Xunit;

namespace ContactTest
{
    public class ImportExportTest
    {
        /// <summary>
        /// A test for GetFields
        /// </summary>
        [Fact]
        public void GetFieldsTest()
        {
            string line = "worse case ;\"I can think\nof right now \";hello";
            char sep = ';';
            string[] expected = { "worse case", "I can think\nof right now", "hello" };
            string[] actual;
            actual = ImportTools.GetFields(line, sep);
            Assert.True(TestUtilities.ArraysEqual(expected, actual), "espected = " + String.Join("|", expected) + ", actual= " + String.Join("|", actual));
        }

        ///// <summary>
        /////A test for LeftPadIf
        /////</summary>
        //[TestMethod()]
        //public void LeftPadIfTest()
        //{
        //    string actual;
        //    actual = ImportExport.LeftPadIf("");
        //    Assert.Equal("", actual);
        //    actual = ImportExport.LeftPadIf("content");
        //    Assert.Equal(" content", actual);
        //}

        /// <summary>
        ///A test for GetAddress
        ///</summary>
        [Fact]
        public void GetAddressTest()
        {
            //
            var row = new Dictionary<string, string> 
            { 
                {"Street1", "Via Colostomia"},
                {"Street2", ""},
                {"City", "My Town" },
                {"State", "" },
                {"PostCode", "1234" },
                {"Country", "Italie" },
            };
            //France/Switzerland/italy
            string expected = "Via Colostomia\r\n1234 My Town\r\nItalie";
            string actual = ImportTools.GetAddress(row);
            Assert.Equal(expected, actual);

            row["State"] = "(RG)";
            expected = "Via Colostomia\r\n1234 My Town (RG)\r\nItalie";
            actual = ImportTools.GetAddress(row);
            Assert.Equal(expected, actual);

            //UK/Canada
            row["PostCode"] = "RG6 1LP";
            row["State"] = "";
            expected = "Via Colostomia\r\nMy Town\r\nRG6 1LP\r\nItalie";
            actual = ImportTools.GetAddress(row);
            Assert.Equal(expected, actual);
            
            //US
            row["Country"] = "USA";
            row["State"] = "CA";
            row["PostCode"] = "12343";
            expected = "Via Colostomia\r\nMy Town CA 12343\r\nUSA";
            actual = ImportTools.GetAddress(row);
            Assert.Equal(expected, actual);
        }

        /// <summary>
        ///A test for LoadFromOutlookCsv
        ///</summary>
        [Fact]
        public void LoadFromCsvTest()
        {
            string filePath =  @"TestData.csv";
            AddressBook actual = ImportExport.LoadFromOutlookCsv(filePath);
            Assert.Equal(8, actual.Contacts.Count);
            Assert.Equal("Françoise", actual.Contacts[0].People[1].FirstName);
            Assert.Equal("+33 42 32 81 59", actual.Contacts[4].PhoneNumber);
            Assert.Equal("Pierre & Françoise Borcard", actual.Contacts[0].DisplayName);
        }

        /// <summary>
        ///A test for GetCSVNew
        ///</summary>
        [Fact]
        public void GetCSVNewTest()
        {
            string text = "Column 1;Column 2;Column 3\r\nValue 1;Value 2;Value 3\r\nValue 4;Value 5;Value 6";
            char sep = ';'; // TODO: Initialize to an appropriate value
            var bag = new Dictionary<string, string> { {"Address1",""}, {"Address2",""}, {"Address3",""}, {"Address4",""} };
            var mapping = new Dictionary<string, string> { {"Column 1", "Address1"}, {"Column 2", "Address3" }};
            IEnumerable<Dictionary<string, string>> actual;
            actual = ImportTools.SelectCSV(text, sep, bag, mapping);
            Assert.Equal(2, actual.Count());
            Assert.Equal("Value 2", actual.First()["Address3"]);
            Assert.Equal("", actual.Last()["Address2"]);
            Assert.Equal("", actual.Last()["Address4"]);
            Assert.Equal("Value 1", actual.First()["Address1"]);
        }
    }
}
