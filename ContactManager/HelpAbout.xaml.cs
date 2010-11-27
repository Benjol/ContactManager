using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Reflection;
using System.Diagnostics;

namespace ContactManager
{
    /// <summary>
    /// Interaction logic for HelpAbout.xaml
    /// </summary>
    public partial class HelpAbout : Window
    {
        public HelpAbout()
        {
            //needs doing first
            var asm = Assembly.GetExecutingAssembly();
            var fvi = FileVersionInfo.GetVersionInfo(asm.Location);
            this.Build = fvi.FileVersion;
            this.Version = asm.GetName().Version.ToString();

            InitializeComponent();
        }

        public string Version { get; private set; }
        public string Build { get; private set; }
    }
}
