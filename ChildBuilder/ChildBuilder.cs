//////////////////////////////////////////////////////////////////////////////////////
// ChildBuilder.cs :        builds projects using .cs files                         //
//                          ver 2.0                                                 //
//----------------------------------------------------------------------------------//
//  Source:                 Ammar Salman, EECS Department, Syracuse University      //
//                          (313)-788-4694, hoplite.90@hotmail.com                  //
//	Author:			        Amritbani Sondhi,										//
//					        Graduate Student, Syracuse University					//
//					        asondhi@syr.edu											//
//	Application:	        CSE 681 Project #3, Fall 2017							//
//	Platform:		        HP Envy x360, Core i7, Windows 10 Home					//
//  Environment:            C#, Visual Studio 2017 RC                               //
//////////////////////////////////////////////////////////////////////////////////////
 /*
 * Package Operations:
 * ===================
 * This package demonstrates receiving CommMessages from the Mother Builder for handling build requests for
 * the Remote Build Server.
 * The Child Builders will be run on separate processes. Each child builder will receive a Build Request Message from
 * the Mother Builder. It will then as the Mock Repository for the files it needs to Compile a Request.
 * When the Repository sends appropriate files to it, it will then proceed with processing the Build Request.
 * 
 * Methods:
 * ==============
 *      Class ChildBuilder -
 *      - ChildBuilder()        - initializes communication objects, endpoints and storage directories
 *      - requestHandler()      - overridden from ChannelComm Abstract class, shows how comm messages will
 *                                be handled
 *      - triggerBuild()        - builds the .cs files sent by the repository
 *      
 * Build Process:
 * ==============
 *	- Required Files:
 *          ProjectBuildServer.cs
 * 	- Build commands:
 *		    devenv RemoteBuildServer.sln
 *		    
 * Maintenance History:
 * ===================
 *      ver 3.0 : Nov 01, 2017
 *      ver 2.0 : Oct 05, 2017
 */

using System;
using System.Collections.Generic;
using Microsoft.Build.BuildEngine;
using Microsoft.Build.Framework;
using Microsoft.Build.Execution;
using System.IO;
using System.Security;
using MessagePassingComm;
using System.Threading;
using System.Diagnostics;

namespace Federation
{
    // Demonstrates how the communication will happen with Child Builder Processes
    public class ChildBuilder : ChannelComm
    {
        private string childBuilderStorage { get; set; } = null;
        private static string motherBuilderEndpoint { get; set; } = "http://localhost:8080/IPluggableComm";
        private static string repositoryEndpoint { get; set; } = "http://localhost:8070/IPluggableComm";
        private static string childBuilderEndpoint { get; set; } = null;

        public ChildBuilder(string baseAddress, int port)
        {
            // creates separate directories for demonstrating separate Child Building Processes
            childBuilderStorage = "../../../Storage/BuilderStorage/Child_" + port;

            // checks if Directories are present or not
            if (!Directory.Exists(childBuilderStorage))
                Directory.CreateDirectory(childBuilderStorage);

            // Comm functions
            childBuilderEndpoint = baseAddress + ":" + port.ToString() + "/IPluggableComm";
            initializeComm(baseAddress, port);

            Console.Title = "Child Builder with EndPoint: " + childBuilderEndpoint;

            // Sends Acknowledgement to the Mother Builder that it is Open for receiving Build Requests
            CommMessage readyAck = new CommMessage(CommMessage.MessageType.request, motherBuilderEndpoint, childBuilderEndpoint, "Amrit", "readyRequest");
            base.commChannel.postMessage(readyAck);

        }

        // overriding from abstract base class 'ChannelComm' present in IMPCommService
        public override void requestHandler(CommMessage commMsg)
        {   
            // demonstrates how the Child Builder will handle different message types and commands
            switch (commMsg.type)
            {
                case CommMessage.MessageType.connect:
                    {
                        Console.WriteLine("\n Connected to a ChildBuilder!");
                        break;
                    }
                case CommMessage.MessageType.request:
                    {
                        if (commMsg.command == "buildRequest")
                        {
                            Console.WriteLine("\n ChildBuilder {0} received a {1} command from {2}!", childBuilderEndpoint, commMsg.command, commMsg.from);
                            commMsg.show();

                            //change the command to fileRequest and send it to Repo
                            commMsg.to = repositoryEndpoint;
                            commMsg.from = childBuilderEndpoint;
                            commMsg.command = "fileRequest";

                            Console.WriteLine("\n Sending a fileRequest from ChildBuilder {0} to Repository", childBuilderEndpoint);

                            base.commChannel.postMessage(commMsg);
                        }
                        else if (commMsg.command == "fileRequest")
                        {
                            Console.WriteLine("\n Your Repository is not converting the file reply's message type to be a Reply Message");
                        }
                        else if (commMsg.command == "quit")
                        {
                            Console.WriteLine("\n ChildBuilder {0} received a {1} command from {2}! ", childBuilderEndpoint, commMsg.command, commMsg.from);
                            commMsg.show();
                            Console.WriteLine("\n Killing {0} Process!", childBuilderEndpoint);
                            Process.GetCurrentProcess().Kill();
                        }
                        else
                        {
                            Console.WriteLine("ChildBuilder {0} received a {1} command from {2}! ", childBuilderEndpoint, commMsg.command, commMsg.from);
                            Console.WriteLine("Inside else case for Request Message");
                            Console.WriteLine("You are missing some message case!");
                        }
                        break;
                    }
                case CommMessage.MessageType.reply:
                    {
                        Console.WriteLine("ChildBuilder {0} received a {1} message from {2}! ", childBuilderEndpoint, commMsg.type, commMsg.from);
                        commMsg.show();

                        if (commMsg.command == "fileRequest")
                        {
                            triggerBuild(commMsg.arguments);
                        }
                        else
                        {
                            Console.WriteLine("\n The reply received was not a fileRequest command. Hence it didn't build");
                        }
                        break;
                    }
                case CommMessage.MessageType.closeSender:
                    {
                        Console.WriteLine("ChildBuilder {0} received a {1} type message from {2}! ", childBuilderEndpoint, commMsg.type, commMsg.from);
                        commChannel.sndr.close();
                        break;
                    }
                case CommMessage.MessageType.closeReceiver:
                    {
                        Console.WriteLine("ChildBuilder {0} received a {1} type message from {2}! ", childBuilderEndpoint, commMsg.type, commMsg.from);
                        commChannel.rcvr.close();
                        Process.GetCurrentProcess().Kill();

                        break;
                    }
                default:
                    {
                        Console.WriteLine("Inside default in Switch-Case of ChildBuilder {0}", childBuilderEndpoint);
                        Console.WriteLine("You are missing some message case");
                        break;
                    }
            }
        }

        // processes Build Requests
        public void triggerBuild(List<string> arguments)
        {
            Console.WriteLine("\n =========================================================================================");
            Console.ForegroundColor = ConsoleColor.Black;
            Console.BackgroundColor = ConsoleColor.White;
            Console.WriteLine(" Triggering ProjectBuildServer!");
            Console.ResetColor();
            Console.WriteLine(" =========================================================================================");

            //Check if files are present in the folder
            foreach (string file in arguments)
            {
                string absFilePath = childBuilderStorage + "\\" + file;
                absFilePath = Path.GetFullPath(absFilePath);

                if (File.Exists(absFilePath))
                {
                    Console.WriteLine("\n ChildBuilder {0} received {1} file from Repository!", childBuilderEndpoint, file);
                }
                else
                {
                    Console.WriteLine("\n {0} file didn't come from the repository!", file);
                }
            }

            Console.WriteLine("\n =========================================================================================");
            Console.ForegroundColor = ConsoleColor.Black;
            Console.BackgroundColor = ConsoleColor.White;
            Console.WriteLine(" Child Build Completed!");
            Console.WriteLine(" Here Project 2's functionality of compiling the files will be done");
            Console.ResetColor();
            Console.WriteLine(" =========================================================================================");

            // Send an acknowledgement to the Mother Builder that this Child Builder is ready to receive the next build request
            CommMessage readyAck = new CommMessage(CommMessage.MessageType.request, motherBuilderEndpoint, childBuilderEndpoint, "Amrit", "readyRequest");
            base.commChannel.postMessage(readyAck);
            Console.WriteLine("\n Sending Ready Message from {0} triggerBuild", childBuilderEndpoint);
        }

        // command line will be in the form of:
        // http://localhost 8081
        static void Main(string[] args)
        {
            string baseAddress = args[0].ToString();
            int port = Convert.ToInt32(args[1]);

            Console.WriteLine(" Building project Build Server");
            Console.WriteLine(" =========================================================================================");

            // Calls Build Server and gets it running
            ChildBuilder childBuildObj = new ChildBuilder(baseAddress, port);

            Thread.Sleep(1000000000);
            Thread.Sleep(1000000000);

            Console.WriteLine(" =========================================================================================");
            Console.WriteLine(" Done Building project Build Server");

            Console.ReadLine();
        }
    }
}
