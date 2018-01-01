//////////////////////////////////////////////////////////////////////////////
// RepoMock.cs -    Demonstrate a Mock Repo Operations                      //
//                  ver 2.0                                                 //
//--------------------------------------------------------------------------//
//  Source:         Prof. Jim Fawcett, CST 4-187, jfawcett@twcny.rr.com     //
//	Author:			Amritbani Sondhi,										//
//					Graduate Student, Syracuse University					//
//					asondhi@syr.edu											//
//	Application:	CSE 681 Project #3, Fall 2017							//
//	Platform:		HP Envy x360, Core i7, Windows 10 Home					//
//  Environment:    C#, Visual Studio 2017 RC                               //
//////////////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * ===================
 *      Simulates basic Repository operations. The Repository is supposed to
 *      get BuildRequests from Client. Then it should send the code files and
 *      build requests to the Build Server for Compilation.
 *      
 *      Here, we are generating a no. of Build Requests and sending it to the MotherBuilder
 *      for compilation. When the assigned ChildBuilder will need files from the repo to process
 *      a build request, the Child builder will send a message asking for the specific files.
 *      The Repo, should then send the specific files to the Child Builder.
 * 
 * Public Methods:
 * ==============
 *      Class RepoMock -
 *      - RepoMock()        : initializes RepoMock Storage and Comm objects 
 *      - requestHandler()  : overridden from ChannelComm Abstract class, shows how comm messages will
 *                            be handled
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
 *      ver 3.0 : Nov 01, 2017
 *      ver 2.0 : Oct 05, 2017
 *      ver 1.0 : Sep 07, 2017
 *          - first release
 *      
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using MessagePassingComm;
using System.Threading;

namespace Federation
{
    ///////////////////////////////////////////////////////////////////
    // RepoMock class
    // - begins to simulate basic Repo operations
    // - and Demonstrates how the communication will happen with the Repo

    public class RepoMock : ChannelComm
    {
        // Storages
        private string repoStorage { get; set; } = "../../../Storage/RepositoryStorage";
        private string childBuilderStorage { get; set; } = null;
        // Endpoints
        private static string repositoryEndpoint { get; set; } = "http://localhost:8070/IPluggableComm";
        private static string motherBuilderEndpoint { get; set; } = "http://localhost:8080/IPluggableComm";
        private List<string> fileList { get; set; } = new List<string>();

        /*----< initialize RepoMock Storage and Comm objects >---------------------------*/
        public RepoMock(string baseAddress, int port)
        {
            // Shows the demonstration of the project
            demoProject();

            // checks if Directories are present or not
            if (!Directory.Exists(repoStorage))
                Directory.CreateDirectory(repoStorage);

            // Comm functions
            repositoryEndpoint = baseAddress + ":" + port.ToString() + "/IPluggableComm";
            base.initializeComm(baseAddress, port);

            Console.Title = "Repository with EndPoint: " + repositoryEndpoint;

            // Build Request Generator
            CommMessage buildRequest = null;
            string author = null;
            for (int c = 1; c <= 10; c++)
            {
                author = "Amrit\'s BuildRequest from Repo No: " + c;
                buildRequest = new CommMessage(CommMessage.MessageType.request, motherBuilderEndpoint, repositoryEndpoint, author, "buildRequest");

                for (int d = 1; d <= 3; d++)
                {
                    string fileName = "Test" + c + "_File" + d + ".cs";

                    // For Every Build Request, we have 3 separate files
                    buildRequest.arguments.Add(fileName);
                }
                // Send the Build Request Message to the MotherBuilder
                base.commChannel.postMessage(buildRequest);
            }
        }

        // overriding from abstract base class 'ChannelComm' present in IMPCommService
        // demonstrates how the Repo will handled the messages received by it
        public override void requestHandler(CommMessage commMsg)
        {
            switch (commMsg.type)
            {
                case CommMessage.MessageType.connect:
                    {
                        Console.WriteLine("\n Connected to Repository!");
                        break;
                    }
                case CommMessage.MessageType.request:
                    {
                        if (commMsg.command == "fileRequest")
                        {
                            Console.WriteLine("\n Repository received a {0} from EndPoint {1}! ", commMsg.command, commMsg.from);
                            commMsg.show();

                            string childBuilder = commMsg.from;

                            // The message will go as a reply to the ChildBuilder
                            commMsg.type = CommMessage.MessageType.reply;
                            commMsg.to = commMsg.from;
                            commMsg.from = repositoryEndpoint;

                            // populate the list of files
                            fileList.Clear();
                            foreach (string file in commMsg.arguments)
                            {
                                fileList.Add(file);
                            }

                            string toEndPoint = commMsg.to;
                            // retrieve port no. of ChildBuilder
                            int index = toEndPoint.IndexOf("t:") + 2;
                            string port = toEndPoint.Substring(index, 4);
                            int portNo = Convert.ToInt32(port);

                            // set up Storage Directory for the Child
                            childBuilderStorage = "../../../Storage/BuilderStorage/Child_" + portNo + "/";

                            if (!Directory.Exists(childBuilderStorage))
                                Directory.CreateDirectory(childBuilderStorage);

                            //Send File to Specific ChildBuilder
                            triggerRepo(commMsg.to);

                            //Send Reply to Specific ChildBuilder
                            Console.WriteLine("\n Repository Sends a Reply to ChildBuilder {0}", childBuilder);
                            commChannel.postMessage(commMsg);
                        }
                        else
                        {
                            Console.WriteLine("\n Repository received a {0} from {1}! ", commMsg.command, commMsg.from);
                            Console.WriteLine("\n Inside else case for Request Message");
                            Console.WriteLine("\n You are missing some message case!");
                        }
                        break;
                    }
                case CommMessage.MessageType.reply:
                    {
                        Console.WriteLine("\n Repository received a {0} message from {1}! ", commMsg.type, commMsg.from);
                        break;
                    }
                default:
                    {
                        Console.WriteLine("\n Inside default in Switch-Case of Repository");
                        Console.WriteLine("\n You are missing some message case");
                        break;
                    }
            }
        }

        // sends files to the ChildBuilder
        private void triggerRepo(string toEndPoint)
        {
            Console.WriteLine("\n =========================================================================================");
            Console.ForegroundColor = ConsoleColor.Black;
            Console.BackgroundColor = ConsoleColor.White;
            Console.WriteLine(" Triggering Mock Repository for sending files to the Child Builder!");
            Console.ResetColor();
            Console.WriteLine(" =========================================================================================");

            Console.WriteLine("\n Sending the following files to {0} \n", childBuilderStorage);

            // specify the list of files which are being sent
            foreach (string file in fileList)
            {
                Console.WriteLine("\n\t {0}", file.ToString());

                // Send files to the ChildBuilder
                commChannel.postFile(file, repoStorage, childBuilderStorage, toEndPoint);
            }

            Console.WriteLine("\n =========================================================================================");
            Console.ForegroundColor = ConsoleColor.Black;
            Console.BackgroundColor = ConsoleColor.White;
            Console.WriteLine(" Mock Repo functionality Completed!");
            Console.WriteLine(" The Mock Repository sent the code files to the Child Build Server");
            Console.ResetColor();
            Console.WriteLine(" =========================================================================================");
        }

        private void demoProject()
        {
            Console.WriteLine(" =============================================================================================================================================");
            Console.WriteLine("\n The Project Demonstrates: \n Req 1 : prepared using C#, the .Net Frameowrk, and Visual Studio 2017");
            Console.WriteLine("\n Req 2 : includes a Message - Passing Communication Service built with WCF. \n       WCF Comm is used between Repo, MotherBuilder and ChildBuilder \n       to transfer build requests, ready requests and file request messages.");
            Console.WriteLine("\n Req 3 : supports accessing build requests by Pool Processes from the mother Builder process, \n     sending and receiving build requests, and sending and receiving files.");
            Console.WriteLine("\n Req 4 : provides a Process Pool component that creates a specified number of processes on command \n     The no. of processes to be opened by the MotherBuilder is specified in Command Line Arguments");
            Console.WriteLine("\n Req 5 : uses Communication prototype to access messages from the mother Builder. \n     they continue to access messages from the shared mother's queue");
            Console.WriteLine("\n Req 6 : includes a Graphical User Interface, built using WPF");
            Console.WriteLine("\n Req 7 : My GUI doesn't trigger the Mother Builder to start or stop processes and Comm");
            Console.WriteLine("\n Req 8 : enables building test requests by selecting file names from the Mock Client Directory");
            Console.WriteLine("\n Req 9 : integrates these three prototypes into a single functional Visual Studio Solution, with a Visual Studio project for each");
            Console.WriteLine("\n =============================================================================================================================================");
            Console.WriteLine("\n The Storage:");
            Console.WriteLine("\n     - \\RemoteBuildServer\\Storage\\BuilderStorage:");
            Console.WriteLine("\n         Separate folders will be created according to the no.of Child Builders \n");
            Console.WriteLine("\n     - MockClientStorage :  \n");
            Console.WriteLine("\n         - \\RemoteBuildServer\\Storage\\MockClientStorage\\CodeFiles:");
            Console.WriteLine("\n             Contains Sample Directory Structure present at Client side to create Build Requests from");
            Console.WriteLine("\n             A copy of this folder is in the main RemoteBuildServer folder \n");
            Console.WriteLine("\n         - \\RemoteBuildServer\\Storage\\MockClientStorage\\BuildRequests:");
            Console.WriteLine("\n             created by the GUI will be saved here \n");
            Console.WriteLine("\n     - RepositoryStorage : \n");
            Console.WriteLine("\n         - \\RemoteBuildServer\\Storage\\RepositoryStorage\\BuildRequests:");
            Console.WriteLine("\n             Build Requests sent by the client will be stored here");
            Console.WriteLine("\n         - \\RemoteBuildServer\\Storage\\RepositoryStorage: \n");
            Console.WriteLine("\n             < .cs files > : Currently these are not uploaded by the Client.A copy of this folder is in the main RemoteBuildServer folder");
            Console.WriteLine("\n =============================================================================================================================================");


        }

        static void Main(string[] args)
        {
            RepoMock repo = new RepoMock("http://localhost", 8070);
            Console.ReadKey();
        }
    }
}
