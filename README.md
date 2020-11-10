# aph-interview-test-01

## Request

Create a web application using ASP.NET MVC in which a file can be uploaded. 
The application calls a web service (preferably hosted by a Windows service) that will archive this file. 
When the archiving is finished, the service announces the web application that the archiving process 
is finished and the new created archive can be downloaded from the graphical interface. At the same time, 
this operation is saved in an MSSQL database as a record in a table with the following information: file name, 
date of loading in the system, archiving time, operation status (success, error).

## Solution

### Create web app

This application will be the frontend - the UI. This is where file upload logic is implemented.

### Create api app

This application handle request to archive file - this will be the API. This is where API logic is implemented.
