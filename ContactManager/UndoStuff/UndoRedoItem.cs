using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ContactManager.Data;

namespace ContactManager.UndoStuff
{
    internal class UndoRedoItem
    {
        public UndoRedoItem(string action, IEnumerable<Contact> before, IEnumerable<Contact> after)
        {
            ActionName = action;
            Before = before == null ? new List<Contact>() : new List<Contact>(before); 
            After = after == null ? new List<Contact>() : new List<Contact>(after);
        }
        public string ActionName { get; private set; }
        public List<Contact> Before { get; private set; }
        public List<Contact> After { get; private set; }
    }
}
