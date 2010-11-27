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
using System.Windows.Shapes;
using System.Reflection;
using System.IO;

namespace ContactManager
{
    /// <summary>
    /// Interaction logic for HelpDescription.xaml
    /// </summary>
    public partial class HelpDescription : Window
    {
        private string m_FilePath;

        public HelpDescription()
        {
            InitializeComponent();
            var temppath = System.IO.Path.GetTempPath();
            m_FilePath = System.IO.Path.Combine(temppath, "HelpDescription.htm");
            DumpHtmlToDisk(m_FilePath); //Note, we recreate and delete everytime - not good to leave cruft on computer.
            webBrowser1.Source = new Uri("file:" + m_FilePath);
        }

        private void DumpHtmlToDisk(string path)
        {
            //http://www.cs.nyu.edu/~vs667/articles/embed_executable_tutorial/
            var streamToResourceFile = Assembly.GetExecutingAssembly()
                .GetManifestResourceStream("ContactManager.Resources.HelpDescription.htm");
            var fileInfoOutputFile = new FileInfo(path);
            var streamToOutputFile = fileInfoOutputFile.OpenWrite();

            const int size = 4096;
            byte[] bytes = new byte[4096];
            int numBytes;
            while((numBytes = streamToResourceFile.Read(bytes, 0, size)) > 0)
            {
                streamToOutputFile.Write(bytes, 0, numBytes);
            }

            streamToOutputFile.Close();
            streamToResourceFile.Close();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            File.Delete(m_FilePath);
        }
    }
}
