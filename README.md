
=============================================================================================================================================

The Project Demonstrates:

Req 1 : prepared using C#, the .Net Framework, and Visual Studio 2017

Req 2 : includes a Message-Passing Communication Service built with WCF
	WCF Comm is used between Repo, MotherBuilder and ChildBuilder
	to transfer build requests, ready requests and file request messages.

Req 3 : supports accessing build requests by Pool Processes from the mother Builder process, 
	sending and receiving build requests, and sending and receiving files.

Req 4 : provides a Process Pool component that creates a specified number of processes on command
	The no. of processes to be opened by the MotherBuilder is specified in Command Line Arguments
	
Req 5 : uses Communication prototype to access messages from the mother Builder.
	they continue to access messages from the shared mother's queue

Req 6 : includes a Graphical User Interface, built using WPF

Req 7 : the GUI doesn't trigger the Mother Builder to start or stop processes and Comm

Req 8 : enables building test requests by selecting file names from the Mock Client Directory

Req 9 : integrates these three prototypes into a single functional Visual Studio Solution, with a Visual Studio project for each

=============================================================================================================================================
The Storage:
	- \RemoteBuildServer\Storage\BuilderStorage : 
		Separate folders will be created according to the no. of Child Builders

	- MockClientStorage : 
		- \RemoteBuildServer\Storage\MockClientStorage\CodeFiles : 
			Contains Sample Directory Structure present at Client side to create Build Requests from
			A copy of this folder is in the main RemoteBuildServer folder

		- \RemoteBuildServer\Storage\MockClientStorage\BuildRequests : 
			created by the GUI will be saved here

	- RepositoryStorage :
		- \RemoteBuildServer\Storage\RepositoryStorage\BuildRequests : 
			Build Requests sent by the client will be stored here
		- \RemoteBuildServer\Storage\RepositoryStorage : 
			< .cs files> : Currently these are not uploaded by the Client. A copy of this folder is in the main RemoteBuildServer folder

=============================================================================================================================================