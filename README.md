# aph-interview-test-01

## Request

Create a web application using ASP.NET MVC in which a file can be uploaded. 
The application calls a web service (preferably hosted by a Windows service) that will archive this file. 
When the archiving is finished, the service announces the web application that the archiving process 
is finished and the new created archive can be downloaded from the graphical interface. At the same time, 
this operation is saved in an MSSQL database as a record in a table with the following information: file name, 
date of loading in the system, archiving time, operation status (success, error).

## Solution

### 1 - Create web app

This application will be the frontend - the UI. This is where file upload logic is implemented.

Steps in VS 2019 Community:

1. File -> New project -> filter by _mvc_ -> select _ASP.NET Web Application (.NET Framework)_
1. First page -> you can leave default (just remember path)
1. Select MVC, and and finish

Now, a solution named the same as the project is created, e.g. _WebApplication1_.

### 2 - Create api app

This application handle request to archive file - this will be the API. This is where API logic is implemented.

First, follow previous steps.

Steps in VS 2019 Community:

1. In solution explorer, right click the e.g. _WebApplication1_ solution (above the _WebApplication1_ project).
1. Add -> New Project -> filter by _mvc_ again -> select _ASP.NET Web Application (.NET Framework)_
1. First page -> you can leave default
1. Select _Web API_
1. Create

### 3 - Verify solution creation

First, check solution files which should look like [here](src/WebApplication1).
