// AIP / b+ @ 2017-10-16
// Method: AIP_Get_SSRS_Report

// This sample code shows how to:
// - Call the SQL report server from Aras
// - Rename SQL Report
// - Load the report result to the Aras Vault
// - Create a new Document with the attached report

// This sample works direct from a Method.
// With some simple modifications it can easy be used in an Action or Form button onClick event.

Innovator inn = this.getInnovator();

string itemID = "1234567891234567897B26F33247F90C"; // Id of Part

// Define Report server
string reportUser = "myuser";
string reportUserPW = "mypassword";
string reportUserDomain = "";
string targetURL = "http://myreportserver/SSRS/";
string subfolder = "MYSUBFOLDER";
string report = "MYBOMREPORT"; // Name of .rdl
string command = "Render";
string format = "EXCEL";
string fileExtension = "xlsx";

// Get properties from request item
Item myItem = inn.newItem("Part","get"); // select Email template
myItem.setProperty("id",itemID);
myItem=myItem.apply();
string itemType = myItem.getProperty("itemType");
string itemNum = myItem.getProperty("item_number"); 
string major_rev = myItem.getProperty("major_rev"); 
string fileString = itemNum + "-" + major_rev + "." + fileExtension; // e.g. A123456-A.xls

// Specify the path for storing the file temporary on the server
string path = @"C:\\temp\\SSRS\\" + fileString;

targetURL = targetURL + "?%2f" + subfolder + 
                        "%2f" + report + 
                        "&rs:Command=" + command + 
                        "&id=" + itemID + 
                        "&rs:Format=" + format  ;
  
System.Net.HttpWebRequest req = (HttpWebRequest)System.Net.WebRequest.Create( targetURL );

req.Credentials = new System.Net.NetworkCredential(
    reportUser,
    reportUserPW,
    reportUserDomain );
req.Method = "GET";

System.Net.WebResponse objResponse = (HttpWebResponse)req.GetResponse();
System.IO.FileStream fs = new System.IO.FileStream(path, System.IO.FileMode.Create);
System.IO.Stream stream = objResponse.GetResponseStream();
          
byte[] buffer = new byte[1024];
int length = stream.Read(buffer, 0, 1024);

while (length > 0)
{
    fs.Write(buffer, 0, length);
    length = stream.Read(buffer, 0, 1024);
}
stream.Close();
fs.Close();

// Check-in File to Aras Vault
Item fileItem = inn.newItem("File", "add");

fileItem.setProperty("filename", fileString);
fileItem.attachPhysicalFile(path);
Item checkin_result = fileItem.apply();

if (checkin_result.isError()){
    inn.newError("Fehler beim File Check-In. Bitte Admin melden.");
}
// Delete temporary File from Server
FileInfo fileInfo = new FileInfo(path);
        fileInfo.Delete();

// Create new Document 
var docItem = inn.newItem("Document","add");
docItem.setProperty("item_number", "Report-12345"); // todo: Change to use sequence instead of fixed value
docItem.setProperty("name", "SSRS Report " + report + " " + fileString );
docItem.setProperty("description", "This document was automatically generated.");

// Create new Relationship and attach File
var relItem = inn.newItem("Document File","add");
relItem.setProperty("related_id",fileItem.getID());
docItem.addRelationship(relItem) ;
var resultItem = docItem.apply();
var docId = resultItem.getID();

return inn.newResult(docId); // id of new Document