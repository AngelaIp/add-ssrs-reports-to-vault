// This sample code shows how to:
// - Call the SQL report server from Aras
// - Rename SQL Report
// - Load the report result to the Aras Vault
// - Create a new Document with the attached report

// This sample works direct from a Method
// With some simple modifications it can easy be used in an Action or Form button onClick event.

Innovator inn = this.getInnovator();

string itemID = "1234567891234567897B26F33247F90C"; // Id of Part for testing

// Get credentials from InnovatorServerConfig.xml
XmlDocument xml = CCO.Cache.GetCacheInfo("ApplicationXML");
string targetURL = "";
string reportServerURL = xml.SelectSingleNode("//ReportingServices/ReportServer").InnerText;
string reportUserDomain = xml.SelectSingleNode("//ReportingServices/Domain").InnerText;
string reportUser = xml.SelectSingleNode("//ReportingServices/User").InnerText;
//string reportUserPW = xml.SelectSingleNode("//ReportingServices/Password").InnerText; // works only with unencrypted PWs
// Pitfall: A SecureString object should never be constructed from a String, because the sensitive data is already subject to the memory persistence consequences of the immutable String class. 

SecureString securedPW = new SecureString(); 
foreach (char c in xml.SelectSingleNode("//ReportingServices/Password").InnerText) // <-  is this really a good approach??
{
    securedPW.AppendChar(c);
}
securedPW.MakeReadOnly();

/* debug - define credentials manually - Use this block for first tests
string reportServerURL = "http://myreportserver/SSRS/";
string reportUserDomain = "";
string reportUser = "myuser";
string reportUserPW = "mypassword";
*/

// Define Report server
string subfolder = "MYSUBFOLDER";
string report = "PCBA_BOM"; // Name of .rdl
string command = "Render";
string format = "EXCEL"; // for xlsx use "EXCELOPENXML"
string fileExtension = "xls";  // xlsx

// Get properties from request item
Item myItem = inn.newItem("Part","get");
myItem.setAttribute("select", "item_number,major_rev");
myItem.setProperty("id",itemID);
myItem = myItem.apply();
if (myItem.isError())
{
    return inn.newError("An error occured: " + myItem.getErrorDetail() ); 
    
} 
string itemType = myItem.getProperty("itemType");
string itemNum = myItem.getProperty("item_number"); 
string major_rev = myItem.getProperty("major_rev"); 
string fileString = itemNum + "-" + major_rev + "." + fileExtension; // e.g. A123456-A01-3.xls

// Specify the path for storing the file temporary on the server
string path = @"C:\\temp\\SSRS\\" + fileString;

targetURL = reportServerURL +   
            "?%2f" + subfolder + 
            "%2f" + report + 
            "&rs:Command=" + command + 
            "&id=" + itemID + 
            "&rs:Format=" + format  ;
  
try 
{
    System.Net.HttpWebRequest request = (HttpWebRequest)System.Net.WebRequest.Create( targetURL );
    request.Credentials = new System.Net.NetworkCredential(
        reportUser,
        securedPW,
        reportUserDomain );
    request.Method = "GET";
    
    System.Net.HttpWebResponse response = (HttpWebResponse)request.GetResponse();

    if (response.StatusCode == HttpStatusCode.OK)
    {
        System.IO.FileStream fs = new System.IO.FileStream(path, System.IO.FileMode.Create);
        System.IO.Stream stream = response.GetResponseStream();
              
        byte[] buffer = new byte[1024];
        int length = stream.Read(buffer, 0, 1024);
    
        while (length > 0)
        {
            fs.Write(buffer, 0, length);
            length = stream.Read(buffer, 0, 1024);
        }
        stream.Close();
        fs.Close();
    }

} 
catch (WebException e)
{
    using (Stream data = e.Response.GetResponseStream())
    {
        string text = new StreamReader(data).ReadToEnd();
        return inn.newError(e.Status + "\n" + e.Message + "\n" + text);
    }
} 
catch (Exception e)
{
    return inn.newError(e.Message);
}

// Check-in File to Aras Vault
Item fileItem = inn.newItem("File", "add");
fileItem.setAttribute("doGetItem", "0");
fileItem.setProperty("filename", fileString);
fileItem.attachPhysicalFile(path);
Item checkin_result = fileItem.apply();
if (checkin_result.isError())
{
    inn.newError("Fehler beim File Check-In. Bitte Admin melden." + checkin_result.getErrorDetail());
}

// Delete temporary File from Server
try 
{
    FileInfo fileInfo = new FileInfo(path);
    fileInfo.Delete();
} 
catch (Exception e) 
{
    return inn.newError(e.ToString());
}

// Create new Document 
Item docItem = inn.newItem("Document","add");
docItem.setAttribute("doGetItem", "0");
string sequenceVal = inn.getNextSequence("Default Document"); 
docItem.setAttribute("useInputProperties", "1"); 
docItem.setProperty("item_number", sequenceVal);
docItem.setProperty("name", "SSRS Report " + report + " " + fileString );
docItem.setProperty("description", "This document was automatically generated.");

// Create new Relationship and attach File
Item relItem = inn.newItem("Document File","add");
relItem.setAttribute("doGetItem", "0");
relItem.setProperty("related_id",fileItem.getID());
docItem.addRelationship(relItem) ;

// Execute query
Item resultItem = docItem.apply();
if (docItem.isError())
{
    return inn.newError("An error occured: " + docItem.getErrorDetail() ); 
} 

string docId = resultItem.getID();

return inn.newResult(docId); // id of new Document
