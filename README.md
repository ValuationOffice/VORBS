VORBS 
[![Build status](https://ci.appveyor.com/api/projects/status/7njtwuy10v6ac93f?svg=true)](https://ci.appveyor.com/project/reecebedding/vorbs)
=======

Room booking mangement system.

Getting Started:
------------
### Currently there is a hard dependency on having an Active Directory service available on your network which we will hopefully move away from. ###

### You will need to have SQL server installed and configured in the Web.config ###
You can find a lightweight express version here: https://www.microsoft.com/en-us/download/details.aspx?id=42299

### Front-End Dependencies ###
To download the dependencies you will need [NodeJS]("https://nodejs.org/en/") installed.

Navigate inside the VORBS project directory and run the command: `npm install`.

To list all the available gulp tasks, navigate inside the VORBS project directory and run the command: `gulp`

### Testing ###

To run the tests for the front-end project, navigate inside the VORBS project directory and run the command: `gulp test`

### Seeding data ###
There is the choice to seed the database with data when it is created. You can specify the values in the DAL/VORBSInitializer.cs file.

## License ##

This code is open source software licensed under the [Apache 2.0 License]("http://www.apache.org/licenses/LICENSE-2.0.html").
