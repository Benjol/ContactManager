using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.ComponentModel;

namespace ContactManager.Data
{
    public class Person : IComparable<Person>, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        //constructor to prevent null fields...
        public Person()
        {
            FirstName = "";
            m_LastName = "";
            MobileNumber = "";
            WorkNumber = "";
            Email = "";
        }
        //Can't have auto-property for this one, as we want to be informed of property changes (for detailed edition cascade from contact name)
        string m_LastName;
        [XmlAttribute("LastName")]
        public string LastName 
        {
            get { return m_LastName; }
            set
            {
                m_LastName = value;
                PropertyChanged(this, new PropertyChangedEventArgs("LastName"));
            }
        }

        [XmlAttribute("FirstName")]
        public string FirstName { get; set; }
        [XmlAttribute("IncludeInDisplayName")]
        public bool IncludeInDisplayName { get; set; }
        [XmlAttribute("MobileNumber")]
        public string MobileNumber { get; set; }
        [XmlAttribute("WorkNumber")]
        public string WorkNumber { get; set; }
        [XmlAttribute("Email")]
        public string Email { get; set; }

        public DateTime? DateOfBirth { get; set; }
        
        //Can't put isnullable on an XmlAttribute http://connect.microsoft.com/VisualStudio/feedback/ViewFeedback.aspx?FeedbackID=172645
        [XmlAttribute("DateOfBirth", DataType = "date")]
        [EditorBrowsable(EditorBrowsableState.Never)] //don't need to see this in intellisense
        public DateTime SerializableDateOfBirth
        {
            get
            {
                return DateOfBirth.HasValue ? DateOfBirth.Value : default(DateTime);
            }
            set
            {
                if(value == default(DateTime))
                    DateOfBirth = null;
                else
                    DateOfBirth = value;
            }
        }

        //XmlSerialization uses this to know whether to serialize or not
        [EditorBrowsable(EditorBrowsableState.Never)] //don't need to see this in intellisense
        public bool SerializableDateOfBirthSpecified { get { return DateOfBirth.HasValue; } }

        //Read-only derived properties
        public string OrderByName { get { return LastName + ", " + FirstName; } }
        public string DisplayName { get { return FirstName + " " + LastName; } }
        public string AllText
        {
            get
            {
                return String.Concat(
                    StringX.RightPadIf(FirstName),
                    StringX.RightPadIf(LastName),
                    StringX.RightPadIf(MobileNumber),
                    StringX.RightPadIf(Email),
                    StringX.RightPadIf(WorkNumber),
                    DateOfBirth.HasValue ? DateOfBirth.Value.ToString("dd MMM yyyy") : ""
                    );
            }
        }
        public int? Age
        {
            get
            {
                if(!DateOfBirth.HasValue) return null;
                
                int age = (DateTime.Now.Year - DateOfBirth.Value.Year);
                int adjust = 0;
                if(DateTime.Now.Month > DateOfBirth.Value.Month || (DateTime.Now.Month == DateOfBirth.Value.Month && DateTime.Now.Day > DateOfBirth.Value.Day)) adjust = 1;
                return age + adjust - 1;
            }
        }

        public Person Clone()
        {
            return new Person
            {
                LastName = this.LastName,
                FirstName = this.FirstName,
                MobileNumber = this.MobileNumber,
                WorkNumber = this.WorkNumber,
                DateOfBirth = this.DateOfBirth, 
                Email = this.Email,
                IncludeInDisplayName = this.IncludeInDisplayName,
            };
        }

        #region IComparable<Person> Members

        public int CompareTo(Person other)
        {
            return this.FirstName.CompareTo(other.FirstName);
        }

        #endregion
    }
}
