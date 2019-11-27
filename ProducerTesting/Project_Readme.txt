This project references several NuGet packages, which need to be downloaded before it will compile.

1) If you have "Automatically check for missing packages during Build" selected in the Visual Studio 
Options dialog (under the NuGet Package Manager node), you can simply build the project to have them 
downloaded.

2) Otherwise, right-click on the project in Solution Explorer, and choose Manage NuGet Packages.
Click on the "Restore" button in the yellow bar at the top to download the packages automatically.
Then build the project.