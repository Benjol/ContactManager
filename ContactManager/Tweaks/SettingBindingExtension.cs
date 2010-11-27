using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace ContactManager
{
    //For binding to settings : see http://stackoverflow.com/questions/845030/wpf-bind-to-a-value-defined-in-the-settings/846297#846297
    public class SettingBindingExtension : Binding
    {
        public SettingBindingExtension()
        {
            Initialize();
        }

        public SettingBindingExtension(string path)
            : base(path)
        {
            Initialize();
        }

        private void Initialize()
        {
            this.Source = ContactManager.Properties.Settings.Default;
            this.Mode = BindingMode.TwoWay;
        }
    }

}
