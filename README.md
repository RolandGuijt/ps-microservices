# GloboTicket ASP.NET Core Microservices Sample Application

GloboTicket is a sample ASP.NET Core Microservices application that you can learn about in the Pluralsight course: "Microservice Archictecture in ASP.NET Core".
### Prerequisites
In order to build and run the sample GloboTicket application, it is recommended that you have the following installed.

- [.NET 10 SDK](https://dotnet.microsoft.com/download). You can test that you have it installed by entering the command `dotnet --list-sdks`
- [Entity Framework Command Line Tools](https://docs.microsoft.com/en-us/ef/core/miscellaneous/cli/dotnet). You can install these as a global tool with the command `dotnet tool install --global dotnet-ef`
- An IDE like Visual Studio or JetBrains Rider
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)

### Building the Code

You can either load `GloboTicket\GloboTicket.sln` and build with your IDE, or from the command line, in the same folder as `GloboTicket.sln`, enter the `dotnet build` command.

### Running the Migrations
The entity framework migrations needed to run the microservices are automatically executed when the applications start. There is code for that in program.cs.
### Running the Application
You can run or debug the complete GloboTicket application directly from within your IDE. All you have to do is start the AppHost project. That will bring all applications up and shows a dashboard with the status of each component. 

### Running the Application from the Command Line
Alternatively, you can run the GloboTicket application from the command line. Navigate to the directory where the apphost resides in and type `dotnet run`. 

**Note:** You may be asked to trust the .NET Core developer certificates. Make sure you do so, in order to use HTTPS to access the services.





