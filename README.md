# aph-interview-test-01

## Request

Create a web application using ASP.NET MVC in which a file can be uploaded.
The application calls a web service (preferably hosted by a Windows service) that will archive this file.
When the archiving is finished, the service announces the web application that the archiving process
is finished and the new created archive can be downloaded from the graphical interface. At the same time,
this operation is saved in an MSSQL database as a record in a table with the following information: file name,
date of loading in the system, archiving time, operation status (success, error).

## Solution

### Pre-requisites

Need VS2019 and IIS installed. [See this article](https://www.howtogeek.com/112455/how-to-install-iis-8-on-windows-8/)
for IIS install.

**Use VS2019 in administrator mode for IIS integration to work!!!**

### 1 - Create web app - aka frontend

This application will be the frontend - the UI. This is where file upload logic is implemented.

Steps in VS 2019 Community:

1. File -> New project -> filter by _mvc_ -> select _ASP.NET Web Application (.NET Framework)_
1. First page -> you can leave default (just remember path)
1. Select MVC, and and finish

Now, a solution named the same as the project is created, e.g. _WebApplication1_.

### 2 - Create api app - aka backend

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

### 4.1 - Test projects in IIS Express (optional)

Configure apps to start on _F5_:

1. Right click on solution
1. Select _multiple startup projects_
1. Set action for projects to _start_

Hit _F5_ or select menu _debug_ -> _start debugging_, say _Yes_ to _IIS Express_ certificates,
_yes_ to certifcate install -> browser window should be opened.

You should have both apps running under _IIS Express_, check in system tray for _IIS Express_ icon.

### 4.2 - Test projects in IIS (this is the way)

For this, you need to have IIS installed and configured
([see this article](https://www.howtogeek.com/112455/how-to-install-iis-8-on-windows-8/)).

We prefer IIS because both apps will be started automatically when navigated.
**Also, start VS2019 in administrator mode!!!**

Configure IIS:

1. Go to project properties (right click) from solution explorer
1. In _web_ tab, in _servers_ section select _local IIS_
1. Hit _CTRL+S_ and confirm save

Repeat for the second project.

Open _IIS_ (just type _IIS_ in _Start_) and under _Default Web Site_ you should see both apps.
They are navigatable using:

* [http://localhost/WebApplication1](http://localhost/WebApplication1)
* [http://localhost/WebApplication2](http://localhost/WebApplication2)

Use [http://localhost/WebApplication2/api/values](http://localhost/WebApplication2/api/values) to test
the mock API that VS template created.

Now we are setup and can move on to changing the source code.

## Source code solution

### API aka backend - first part - setup

We need to create an file upload folder on the local disk (easiest option).
The easiest way is to do it on app startup:

This is implemented in *Application_Start* event
([see this on other app events](https://www.c-sharpcorner.com/uploadfile/aa04e6/major-events-in-global-asax-file/)).

```csharp
            var uploadDir = Server.MapPath("~/App_Data/Upload");
            System.Diagnostics.Trace.TraceInformation($"Probing '{uploadDir}' for uploads...");
            if (!Directory.Exists(uploadDir))
            {
                Directory.CreateDirectory(uploadDir);
                System.Diagnostics.Trace.TraceInformation($"Created '{uploadDir}' for uploads!");
            }
```

Also, we are going to hook an error handler into *Application_Error* and configure it to
trace unhandled exceptions.

```csharp
        protected void Application_Error()
        {
            var ex = Server.GetLastError();
            System.Diagnostics.Trace.TraceError($"Unhandled exception: {ex}");
        }
```

For the trace to work, we modify _web.config_ file to reflect that by adding
a _system.diagnostics_ section like this (the log file name will be *__.log*):

```xml
<system.diagnostics>
    <trace autoflush="false" indentsize="4">
      <listeners>
        <add name="myListener" type="System.Diagnostics.TextWriterTraceListener" initializeData="__.log" />
        <remove name="Default" />
      </listeners>
    </trace>
  </system.diagnostics>
```

Logging is setup the same for the frontend.
[See this article on logging and tracing to file.](https://docs.microsoft.com/en-us/dotnet/framework/debug-trace-profile/how-to-create-and-initialize-trace-listeners)

### API aka backend - second part - implementing API

The default VS template for ASP.NET WebApi project already created a controller for us
in _Controllers_ folder called _ValuesController_.

We will start by creating an identical copy of that controller which we will call
_FilesController_. This is where we will implement the backend for our frontend.

So, hit _CTRL+C_ then _CTRL+V_ in same project folder (_Controllers_). Rename _ValuesController - Copy.cs_ to
_FilesController.cs_. Rename class in file to _FilesController_ and
[this](src/WebApplication1/WebApplication2/Controllers/FilesController.cs) is how it should look like.

#### Implementing files controller

Let's first use a convention for the upload dir value by implementing a static property
on the _FilesController_ like this:

```csharp
        /// <summary>
        /// Gets upload dir.
        /// </summary>
        public static string UploadDir
        {
            get { return HttpContext.Current.Server.MapPath("~/App_Data/Upload"); }
        }
```

Now let's implement the _Get()_ method. As a convention, get methods with no parameters
retrieve lists. In our case the method will return the list of _zip_ files from upload
folder. Also, we will not create a separate class for _DTO (Data Transfer Object)_
so we will return a serializable list of anonymous type objects (these are
resolved at compile time) using LINQ like so:

```csharp
        // GET api/files
        public IHttpActionResult Get()
        {
            var files = Directory.GetFiles(UploadDir, "*.zip");
            // Get file system info for each file.
            var res = from _ in files
                      select new FileInfo(_);
            // Return file info as serializable DTO.
            return this.Ok(from _ in res
                    select new 
                    {
                        _.Name,
                        _.FullName,
                        _.Extension,
                        _.Length,
                        _.CreationTime,
                        _.LastWriteTime,
                    });
        }
```

You can test with _Postman_ by creating a simple _GET_ request to
[http://localhost/WebApplication2/api/files](http://localhost/WebApplication2/api/files). Don't forget to place
some dummy files in there [App_Data/Uploads](src/WebApplication1/WebApplication2/App_Data/Uploads).

Response should look like this:

```json
[
    {
        "Name": "see.zip",
        "FullName": "D:\\src\\github\\apphousero\\aph-interview-test-01\\src\\WebApplication1\\WebApplication2\\App_Data\\Upload\\see.zip",
        "Extension": ".zip",
        "Length": 14,
        "CreationTime": "2020-11-10T17:53:21.1065436+02:00",
        "LastWriteTime": "2020-11-10T18:12:13.876586+02:00"
    }
]
```

Now let's implement _Get(string id)_ method.

```csharp
        // GET api/files/see.zip
        public HttpResponseMessage Get(string id)
        {
            // Create HTTP Response.
            var res = Request.CreateResponse(HttpStatusCode.OK);
            // Check file id parameter.
            if (string.IsNullOrWhiteSpace(id))
            {
                res.StatusCode = HttpStatusCode.BadRequest;
                res.ReasonPhrase = "No file ID provided.";
                throw new HttpResponseException(res);
            }
            var fp = Path.Combine(UploadDir, id);
            if (!File.Exists(fp))
            {
                res.StatusCode = HttpStatusCode.NotFound;
                res.ReasonPhrase = $"File id not found!";
                throw new HttpResponseException(res);
            }
            // Read the File into a Byte Array.
            var bytes = File.ReadAllBytes(fp);
            // Set the Response Content.
            res.Content = new ByteArrayContent(bytes);
            // Set the Response Content Length.
            res.Content.Headers.ContentLength = bytes.LongLength;
            // Set the Content Disposition Header Value and FileName.
            res.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
            res.Content.Headers.ContentDisposition.FileName = id;
            // Set the File Content Type as byte stream (browsers will download instead of display).
            res.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            return res;
        }
```

After rebuilding you should be able to download file using this link
[http://localhost/WebApplication2/api/files?id=see.zip](http://localhost/WebApplication2/api/files?id=see.zip).

#### Implementing processing task

In _WebApplication2_, in _references_, right click and add a reference to _System.IO.Compression_.

Create a folder _Tasks_ and add _ZipProcessingTask.cs_. Check
[this link for implementation](src/WebApplication1/WebApplication2/Tasks/ZipProcessingTask.cs).

In order to activate this task at the startup of the application, we go back to _Global.asax_ file 
(the _cs_ part) and add this property ```public ZipProcessingTask ZipProcessingTask { get; private set; } = null;```
then we call it in *Application_Start* event like this
```ZipProcessingTask = new ZipProcessingTask();```.

#### Implementing file upload

For this we change POST method in _FilesController_.

Check implementation here
[this link for implementation](src/WebApplication1/WebApplication2/Controllers/FilesController.cs).

### Frontend - upload file

First, let's delete all boiler plate code from _WebApplication1/Views/Home/Index.cshtml_.

We will use [this link](https://getbootstrap.com/docs/4.0/components/forms/#file-browser) as a
reference on file input with _Bootstrap CSS_.

Most of the code is in the main _HomeController_ in _WebApplication1_. Most important is both
implementations of the _Index_ action (the GET and the POST). GET returns the list of files from API
and POST uploads a file to API.

All view changes are done in _WebApplication1/Views/Home/Index.cshtml_.

One more thing added to UI is a _web.config_ key for API URL as it is best practice called _api-url_.
