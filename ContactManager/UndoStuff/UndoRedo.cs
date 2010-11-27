using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ContactManager.Data;
using System.ComponentModel;

namespace ContactManager.UndoStuff
{
    public class UndoRedo : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        private Stack<UndoRedoItem> m_Undoable;
        private Stack<UndoRedoItem> m_Redoable;
        private AddressBook m_AddressBook;

        //only undo/redo can do stuff to addressbook?
        public UndoRedo(AddressBook addressbook)
        {
            m_AddressBook = addressbook;
            m_Undoable = new Stack<UndoRedoItem>();
            m_Redoable = new Stack<UndoRedoItem>();
        }

        public void PerformAction(string actionName, IEnumerable<Contact> before, IEnumerable<Contact> after)
        {
            m_Undoable.Push(new UndoRedoItem(actionName, before, after)); //important, to keep references!
            m_AddressBook.DeleteContacts(before);
            m_AddressBook.AddContacts(after);
            m_Redoable.Clear();
            UpdateProperties();
        }

        public Contact UndoAction()
        {
            if(m_Undoable.Count == 0) return null;
            var item = m_Undoable.Pop();
            m_AddressBook.DeleteContacts(item.After);
            m_AddressBook.AddContacts(item.Before);
            m_Redoable.Push(item);
            UpdateProperties();
            return item.Before.FirstOrDefault();
        }

        public Contact RedoAction()
        {
            if(m_Redoable.Count == 0) return null;
            var item = m_Redoable.Pop();
            m_AddressBook.AddContacts(item.After);
            m_AddressBook.DeleteContacts(item.Before);
            m_Undoable.Push(item);
            UpdateProperties();
            return item.After.FirstOrDefault();
        }

        private void UpdateProperties()
        {
            PropertyChanged(this, new PropertyChangedEventArgs("CanUndo"));
            PropertyChanged(this, new PropertyChangedEventArgs("CanRedo"));
            PropertyChanged(this, new PropertyChangedEventArgs("NextUndoAction"));
            PropertyChanged(this, new PropertyChangedEventArgs("NextRedoAction"));
        }

        public bool CanUndo { get { return m_Undoable.Count > 0; } }
        public bool CanRedo { get { return m_Redoable.Count > 0; } }
        public string NextUndoAction { get { return m_Undoable.Peek().ActionName; } }
        public string NextRedoAction { get { return m_Redoable.Peek().ActionName; } }
    }
}
