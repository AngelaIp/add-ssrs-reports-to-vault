# How to get files from the SSRS Report Server

## Description

Aras offers the ability to create reports by using the SQL Server Reporting Services (SSRS).
The connection to the report server is done via the rsgateway.aspx.

If reports have to be generated and stored automatically, the following issue arise:
* Every generated report has the name rsgateway.aspx by default. 
* Users have to rename and save the Report by themself.

This sample code shows how to:
* Access the SQL report server directly from Aras
* Rename a SQL Report automatically
* Load the report result to the Aras Vault
* Create a new Document with the attached report

This sample works direct from a Method.
With some simple modifications it can easy be used in an Action or Form button onClick event.

## Project Details

#### Built Using:
Aras 11.0 SP9

SQL Server 2012

#### Versions Tested:
Aras 11.0 SP9, 11.0 SP12

SQL Server 2012

#### Browsers Tested:
Internet Explorer 11, Chrome 61.0, Firefox ESR 52.4.0

## Usage

1. Backup your database and store the BAK file in a safe place.
2. Navigate to **Administration > Methods** in the table of contents (TOC).
3. Create a new Method (Server-side / C#)
4. Insert sample code 
5. Adjust the report settings inside the code
6. Click **Run the Method**
7. Method will create a new Document with the Report attached

## Contributing

1. Fork it!
2. Create your feature branch: `git checkout -b my-new-feature`
3. Commit your changes: `git commit -am 'Add some feature'`
4. Push to the branch: `git push origin my-new-feature`
5. Submit a pull request :D


## License

This project is published to Github under the Microsoft Public License (MS-PL). See the [LICENSE file](./LICENSE.md) for license rights and limitations.
