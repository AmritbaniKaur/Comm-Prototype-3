//////////////////////////////////////////////////////////////////////////////////////
// NavigatorClient.xaml.cs - Demonstrates Directory Navigation in WPF App           //
//                           ver 1.0                                                //
//----------------------------------------------------------------------------------//
//  Source:         Prof. Jim Fawcett, CST 4-187, jfawcett@twcny.rr.com             //
//	Author:			Amritbani Sondhi,										        //
//					Graduate Student, Syracuse University					        //
//					asondhi@syr.edu											        //
//	Application:	CSE 681 Project #3, Fall 2017							        //
//	Platform:		HP Envy x360, Core i7, Windows 10 Home					        //
//  Environment:    C#, Visual Studio 2017 RC                                       //
//////////////////////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * ===================
 * This package, creates a GUI to be used by the Clients to generate Build Requests and Send it to the repository
 * 
 * Public Methods:
 * ==============
 *      Class MainWindow -
 *      - getTopFiles()     - show files and dirs in root path
 *      
 * Build Process:
 * ==============
 *	- Required Files:
 *          RepoMock.cs
 * 	- Build commands:
 *		    devenv RemoteBuildServer.sln
 *		    
 * Maintenance History:
 * ===================
 *      ver 1.0 : Nov 01, 2017
 *          - first release
 *      
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using HelpSession;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace NavigatorClient
{
    // creates main GUI window and provides UI and functionality to it
    public partial class MainWindow : Window
    {
        private string buildRequestPath { get; set; } = "../../../Storage/MockClientStorage/BuildRequests/";
        private string repoBuildRequestStorage { get; set; } = "../../../Storage/RepositoryStorage/BuildRequests/";

        private IFileMgr fileMgr = null;  // note: Navigator just uses interface declarations

        public MainWindow()
        {
            InitializeComponent();
            fileMgr = FileMgrFactory.create(FileMgrType.Local);
            getTopFiles(directoryListBox, filesListBox);

            if (this.filesListBox.Items.Count > 0)
                this.filesListBox.SelectedIndex = 0;
        }

        //----< show files and dirs in root path >-----------------------
        private void getTopFiles(ListBox fromListBoxName, ListBox toListBoxName)
        {
            List<string> files = fileMgr.getFiles().ToList<string>();
            toListBoxName.Items.Clear();

            string fileName = null;
            foreach (string file in files)
            {
                fileName = System.IO.Path.GetFileName(file);
                toListBoxName.Items.Add(fileName);
            }
            List<string> dirs = fileMgr.getDirs().ToList<string>();

            if (fromListBoxName != null)
            { fromListBoxName.Items.Clear(); }

            foreach (string dir in dirs)
            {
                fromListBoxName.Items.Add(dir);
            }
        }

        //----< move to directory root and display files and subdirs >---
        private void rootDirectory_Click(object sender, RoutedEventArgs e)
        {
            fileMgr.currentPath = "";
            getTopFiles(directoryListBox, filesListBox);
        }

        //----< show selected file in code popup window >----------------
        private void filesListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            string fileName = filesListBox.SelectedValue as string;
            try
            {
                string path = System.IO.Path.Combine(ClientEnvironment.localRoot, fileName);
                string contents = File.ReadAllText(path);
                CodePopUp popup = new CodePopUp();
                popup.codeView.Text = contents;
                popup.Show();
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
            }
        }

        //----< move to parent directory and show files and subdirs >----
        private void prevDirectory_Click(object sender, RoutedEventArgs e)
        {
            if (fileMgr.currentPath == "")
                return;
            fileMgr.currentPath = fileMgr.pathStack.Peek();
            fileMgr.pathStack.Pop();
            getTopFiles(directoryListBox, filesListBox);
        }

        //----< move into subdir and show files and subdirs >------------
        private void showFiles_Click(object sender, RoutedEventArgs e)
        {
            string dirName = directoryListBox.SelectedValue as string;
            fileMgr.pathStack.Push(fileMgr.currentPath);
            fileMgr.currentPath = dirName;
            getTopFiles(directoryListBox, filesListBox);

            if (this.filesListBox.Items.Count > 0)
                this.filesListBox.SelectedIndex = 0;
        }

        //----< display source in popup window (Same as FilesListBox) >-------
        private void selectedFileListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            string fileName = selectedFileListBox.SelectedValue as string;
            try
            {
                string path = System.IO.Path.Combine(ClientEnvironment.localRoot, fileName);
                string contents = File.ReadAllText(path);
                CodePopUp popup = new CodePopUp();
                popup.codeView.Text = contents;
                popup.Show();
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
            }
        }

        //----< display Build request file contents (Same as FilesListBox)>--
        private void buildRequestListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            string fileName = buildRequestListBox.SelectedValue as string;
            try
            {
                string path = System.IO.Path.Combine(ClientEnvironment.localRoot, fileName);
                string contents = File.ReadAllText(path);
                CodePopUp popup = new CodePopUp();
                popup.codeView.Text = contents;
                popup.Show();
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
            }

        }

        // Creates a Build Request according to the files selected
        private void createBR_Click(object sender, RoutedEventArgs e)
        {
            createBuildRequests();

            // display in buildRequestListBox
            fileMgr.currentPath = buildRequestPath;
            fileMgr.pathStack.Push(fileMgr.currentPath);

            getTopFiles(null, buildRequestListBox);

            // Save Build requests to Repository
            // Saves all the files contained in the MockClientStorage Folder

            string from = System.IO.Path.GetFullPath(buildRequestPath);
            string to = System.IO.Path.GetFullPath(repoBuildRequestStorage);
            sendBuildRequest(from, to);
        }

        // sends generated Build Requets files to the Mock Repo
        private bool sendBuildRequest(string from, string to)
        {
            bool status = false;

            // checks if the Directory exists
            if (!Directory.Exists(from))
                Console.WriteLine("Invalid 'from' Directory. Please provide a valid Directory Name.");
            if (!Directory.Exists(to))
                Directory.CreateDirectory(to);

            List<string> files = new List<string>();

            // get all files into a List
            string[] tempFiles = Directory.GetFiles(from, "*.*");
            for (int i = 0; i < tempFiles.Length; ++i)
            {
                tempFiles[i] = System.IO.Path.GetFullPath(tempFiles[i]);
            }
            files.AddRange(tempFiles);

            Console.WriteLine(" ----------------------------------------------------------------------------------------------");

            // print all the files in the list
            foreach (string f in files)
            {
                string fileSpec = f;
                Console.WriteLine("\n Sending \"{0}\" \n to \n \"{1}\"", fileSpec, to);
                string fileName = "";

                try
                {
                    // configure Destination path
                    fileName = System.IO.Path.GetFileName(fileSpec);
                    string destSpec = System.IO.Path.Combine(to, fileName);

                    File.Copy(fileSpec, destSpec, true);
                    status = true;
                }

                catch (Exception ex)
                {
                    Console.WriteLine(" --{0}--", ex.Message);
                    status = false;
                }

            }
            Console.WriteLine(" All Build Requests saved successfully to Repository");
            files.Clear();
            return status;
        }

        // Removes the selected file from the selected File List
        private void removeFile_Click(object sender, RoutedEventArgs e)
        {
            string itemValue = selectedFileListBox.SelectedValue.ToString();
            int itemIndex = selectedFileListBox.SelectedIndex;

            selectedFileListBox.Items.RemoveAt(itemIndex);
        }

        private void filesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // do nothing for now
        }

        private void buildRequestListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // do nothing for now
        }

        private void addFile_Click(object sender, RoutedEventArgs e)
        {
            string itemValue = filesListBox.SelectedValue.ToString();

            selectedFileListBox.Items.Add(itemValue);
        }

        private void buildBR_Click(object sender, RoutedEventArgs e)
        {
            // do nothing for now
        }

        // creates Build requests to be sent to the MockRepo
        private void createBuildRequests()
        {
            // checks if the Directory exists at Client, if not then create
            if (!System.IO.Directory.Exists(buildRequestPath))
                System.IO.Directory.CreateDirectory(buildRequestPath);

            // Create a File Path
            buildRequestPath = System.IO.Path.GetFullPath(buildRequestPath);
            string dtTime = DateTime.Now.ToString();
            dtTime = dtTime.Replace(":", "-");
            dtTime = dtTime.Replace("/", "-");

            string fileName1 = "BuildRequest_";
            fileName1 = fileName1 + dtTime + ".txt";

            string fileSpec1 = System.IO.Path.Combine(buildRequestPath, fileName1);

            // List of code files
            List<string> codeFiles = new List<string>();
            string fileName = null;

            foreach (string file in selectedFileListBox.Items)
            {
                fileName = System.IO.Path.GetFileName(file);
                codeFiles.Add(fileName);
            }

            // initialize your parameters for all your Build Requests
            TestRequest tr1 = initializeRequestParameters("ProjectBuilder", "Client", "BuildRequest", "TestDriver1.cs", codeFiles);

            tr1.makeRequest();

            // Saves an xml file on the ClientStorage
            Console.WriteLine("\n Saving Test Request to \"{0}\" \n", fileSpec1);
            tr1.saveXml(fileSpec1);

            selectedFileListBox.Items.Clear();
        }

        // initializes all the Request Parameters for the Build Request
        private TestRequest initializeRequestParameters(string to, string from, string type, string testDriver, List<string> testFiles)
        {
            TestRequest tr = new TestRequest();

            // The Client passes the parameters and it gets initialized for the TestRequest class
            tr.toRequest = to;
            tr.fromRequest = from;
            tr.typeOfRequest = type;
            tr.testDriver = testDriver;

            foreach (string codeFile in testFiles)
                tr.testedFiles.Add(codeFile);

            return tr;
        }

        private void selectedFileListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
