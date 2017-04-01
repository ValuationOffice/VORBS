VORBS
=======

Room booking mangement system.

Getting Started:
------------
### Currently there is a hard dependency on having an Active Directory service available on your network which we will hopefully move away from. ###

### You will need to have SQL server installed and configured in the Web.config ###
You can find a lightweight express version here: https://www.microsoft.com/en-us/download/details.aspx?id=42299

### Front-End Dependencies ###
To download the dependencies you will need [NodeJS]("https://nodejs.org/en/") installed.

Once installed, download the [bower]("https://bower.io/") package globally using the following command: `npm install bower -g`

Finally, navigate inside the VORBS project and run the command: `bower install`

### Seeding data ###
There is the choice to seed the database with data when it is created. You can specify the values in the DAL/VORBSInitializer.cs file.

## License ##

This code is open source software licensed under the [Apache 2.0 License]("http://www.apache.org/licenses/LICENSE-2.0.html").
