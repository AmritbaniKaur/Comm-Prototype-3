//////////////////////////////////////////////////////////////////////
// FileMgr - provides file and directory handling for navigation    //
//           ver 1.1                                                //
// Jim Fawcett, CSE681 - Software Modeling and Analysis, Fall 2017  //
//////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * ===================
 * Creates and parses TestRequest XML messages using XDocument
 *
 * Public Methods:
 * ==============
 *      Class FileMgrFactory-
 *      - create()          - creates an objec of LocalFileMgr
 *      
 *      Class LocalFileMgr-
 *      - getFiles()        - get names of all files in current directory
 *      - getDirs()         - get names of all subdirectories in current directory
 *      - setDir()          - sets value of current directory
 *      
 * Build Process:
 * ==============
 *	- Required Files:
 *          TestRequest.cs
 * 	- Build commands:
 *		    devenv RemoteBuildServer.sln
 *		    
 * Maintenance History:
 * ===================
 *      ver 1.1 - Nov 01, 2017
 *              added Main()
 *      ver 1.0
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace NavigatorClient
{
    public struct ClientEnvironment
    {
        public static string localRoot { get; set; } = "../../../Storage/MockClientStorage/CodeFiles/";
    }

    public struct ServerEnvironment
    {
        public static string remoteRoot { get; set; } = "../../../ServerFiles/";
    }

    public enum FileMgrType { Local, Remote }

    ///////////////////////////////////////////////////////////////////
    // NavigatorClient uses only this interface and factory
    public interface IFileMgr
    {
        IEnumerable<string> getFiles();
        IEnumerable<string> getDirs();
        bool setDir(string dir);
        Stack<string> pathStack { get; set; }
        string currentPath { get; set; }
    }

    public class FileMgrFactory
    {
        static public IFileMgr create(FileMgrType type)
        {
            if (type == FileMgrType.Local)
                return new LocalFileMgr();
            else
                return null;  // eventually will have remote file Mgr
        }
    }

    ///////////////////////////////////////////////////////////////////
    // Concrete class for managing local files
    public class LocalFileMgr : IFileMgr
    {
        public string currentPath { get; set; } = "";
        public Stack<string> pathStack { get; set; } = new Stack<string>();

        public LocalFileMgr()
        {
            pathStack.Push(currentPath);  // stack is used to move to parent directory
        }

        //----< get names of all files in current directory >------------
        public IEnumerable<string> getFiles()
        {
            List<string> files = new List<string>();
            string path = Path.Combine(ClientEnvironment.localRoot, currentPath);
            string absPath = Path.GetFullPath(path);
            files = Directory.GetFiles(path).ToList<string>();
            for (int i = 0; i < files.Count(); ++i)
            {
                files[i] = Path.Combine(currentPath, Path.GetFileName(files[i]));
            }
            return files;
        }

        //----< get names of all subdirectories in current directory >---
        public IEnumerable<string> getDirs()
        {
            List<string> dirs = new List<string>();
            string path = Path.Combine(ClientEnvironment.localRoot, currentPath);
            dirs = Directory.GetDirectories(path).ToList<string>();
            for (int i = 0; i < dirs.Count(); ++i)
            {
                string dirName = new DirectoryInfo(dirs[i]).Name;
                dirs[i] = Path.Combine(currentPath, dirName);
            }
            return dirs;
        }

        //----< sets value of current directory - not used >-------------
        public bool setDir(string dir)
        {
            if (!Directory.Exists(dir))
                return false;
            currentPath = dir;
            return true;
        }
    }

    //#if (TEST_FILEMGR)
    class TestFileMgr
    {
        static void Main(string[] args)
        {
            LocalFileMgr locObj = new LocalFileMgr();
            Console.WriteLine("\n Path Stack value: {0}", locObj.pathStack.Pop().ToString());

            locObj.setDir("../../../Storage/");
            Console.WriteLine("\n Current Directory (newly Set): {0}", System.IO.Directory.GetCurrentDirectory().ToString());

            IEnumerable<string> dirList = locObj.getDirs();
            Console.WriteLine("\n Sub Directories in Current Directory: ");
            foreach (string dir in dirList)
            {
                Console.WriteLine(" {0}", dir.ToString());
            }
            Console.WriteLine(" ");
        }
    }
    //#endif

}
