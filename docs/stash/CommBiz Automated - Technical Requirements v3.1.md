f Commonwealth Bank of Australia 



<!-- Start of picture text -->
CommBiz Automated Automated<br><!-- End of picture text -->

# CommBiz Automated Automated Technical Requirements 



<!-- Start of picture text -->
Technical Requirements<br><!-- End of picture text -->

Last updated: April 2024 

Version 3.1 



|Version Control|3|
|---|---|
|Glossary|3|
|1. Introduction|4|
|2. Information you need to send to the Bank|4|
|2.1 CommBiz Setup|4|
|2.2 Automated Connection|4|
|3. Client CommBizAutomated Connection Testing|5|
|3.1 ConnectivityTesting|5|
|3.2 CommBiz File Testing|5|
|4. Bank Support|6|
|4.1 Implementation Managers|6|
|4.2 CommBiz Helpdesk Support|6|
|5. Business Continuity Planning (BCP) Considerations|6|
|6. Technical Details|7|
|6.1 Hardware / Software Requirements|7|
|6.2 Security and Communication Protocols|7|
|6.3 Connection to the Bank|8|
|Appendix 1 — Possible scripting flow charts|10|
|Downloading Receivables Files|10|
|Appendix 2 — File Naming and File Type Conventions|11|
|Acceptable filenames|11|
|AcceptablefileExtensions:|11|





<!-- Start of picture text -->
Acceptable file Extensions:<br><!-- End of picture text -->



<!-- Start of picture text -->
11<br><!-- End of picture text -->







<!-- Start of picture text -->
f<br><!-- End of picture text -->

2 CommBiz Automated Technical Requirements | Version 3.1 

### Version Control 

|~~2.0.12~~|~~October 2010~~||~~Published versionused for migrating clients~~|
|---|---|---|
|2.1|<br>June 2011|<br>Updated specifications including Secure Transfer details|
|3.1<br>Glossary|April 2024|Major updates|
|CommBiz UI||CommBiz User Interface via CommBiz.com.au|
|ERP||Enterprise Resource Planning. This software manages internal and external<br>resources such as financials and HR. Examples include SAP, JDEdwards, SAGE<br>and Microsoft Dynamics amongst many others.|
|LAN||A local areanetwork (LAN) supplies network capability (e.g. internet, email) toa<br>group of computers within close proximity, such as an office building or school|
|PGP||PGP is a data encryption and decryption standard. PGP is used to encrypt or<br>digitallysign files (e.g. email, text files).|
|PKI (Public K|ey Infrastructure)|Asystem that uses digital certificates from certificate authorities thatverify and<br>authenticatethe validity of each party in an internet transaction.|
|SFTP (Secur<br>Protocol)|e File Transfer|SFTP usesSSH to transfer files and encrypts both dataand commandsthus<br>preventing passwords and information from being transmitted in human<br>readable format over the network.|
|SSH||Secure Shell enables datato be exchanged between two networks via a secure<br>channel. SSH operates on port 22 and ensuresthat the communication channel<br>is secure|









<!-- Start of picture text -->
y<br><!-- End of picture text -->

3 CommBiz Automated Technical Requirements | Version 3.1 



CommBiz Automated is CommBank’s client integration and file transfer solution. It allows clients to connect directly with the Bank through their Enterprise Resource Planning (ERP) software to exchange payment instructions and account information and allow straight through processing for a client's business banking needs. 



<!-- Start of picture text -->
payment instructions and account information and allow straight through processing for a client's<br><!-- End of picture text -->



<!-- Start of picture text -->
business<br><!-- End of picture text -->

To assist in the planning and management of your onboarding for CommBiz Automated, the Bank will provide support through your Relationship Team and a dedicated Implementation Manager as well as documentation to assist with your onboarding. 

Below you will find detailed technical requirements, which your information technology team will be able to use in the provisioning of your new CommBiz Automated solution. This service will automate and streamline the transfer of your data files to and from the bank using industry standard secure file transfer software and appliances. 



<!-- Start of picture text -->
the bank using industry standard secure file transfer<br><!-- End of picture text -->



<!-- Start of picture text -->
A glossary has been provided in Appendix 2 to assist with technical terminology that is used in this<br><!-- End of picture text -->

A glossary has been provided in Appendix 2 to assist with technical terminology that is used in this document. 



<!-- Start of picture text -->
process, please contact your<br><!-- End of picture text -->

If you have any questions around these requirements or the engagement process, please contact your Relationship Team or Implementation Manager. 



<!-- Start of picture text -->
Relationship Team or Implementation Manager.<br><!-- End of picture text -->





CommBiz Service: CommBiz is the Bank's online Business Banking channel. As part of this Implementation, we will need to establish your business with CommBiz access. If you already have CommBiz, your existing service can used. If you you do not have CommBiz, please speak to your Relationship Team to help facilitate your CommBiz setup. 



<!-- Start of picture text -->
CommBiz, your existing service can used. If you you do not have CommBiz, please speak to your Relationship<br><!-- End of picture text -->

CommBiz Automated Registration Form: We require you to complete the CommBiz Automated Registration form to authorise the establishment of CommBiz Automated. This form also outlines the CommBiz service, the Automated User, the ERP being connected, and the Payables and Receivables information coming through the CommBiz Automated channel. 



<!-- Start of picture text -->
Please review this form, confirm that the information is correct and that you you would like us to proceed to<br><!-- End of picture text -->

Please review this form, confirm that the information is correct and that you you would like us to proceed to set up your facility. 



The Automated User: The Bank will create an “Automated User” in CommBiz. This “Automated User” will need to be linked to an identified user on your CommBiz Service. This will be completed by the the Bank as part of the implementation process. 



<!-- Start of picture text -->
need to be linked to an identified user on your CommBiz Service. This will be completed by the the Bank as<br><!-- End of picture text -->

Public/Private Keys: Using secure file transfer protocol (SFTP) requires the use of PKI public/private key pairs. To connect to the Bank, you will need to generate and then provide the Bank with your public key. Technical details of this process are included Section 2 of this document and may be used by your your IT department. 



<!-- Start of picture text -->
Technical details of this process are included Section 2 of this document and may be used by your your IT<br><!-- End of picture text -->



<!-- Start of picture text -->
department.<br><!-- End of picture text -->







<!-- Start of picture text -->
f<br><!-- End of picture text -->

4 CommBiz Automated Technical Requirements | Version 3.1 

PGP Key (Optional): For further file security, you may wish to digitally encrypt your files using PGP. Your IT department will have to provide your PGP public key to the Bank so we can encrypt the files you receive from the Bank. The Bank will provide you with its PGP public key so you can encrypt the files sent to the Bank. 



<!-- Start of picture text -->
from the Bank. The Bank will provide you with its<br><!-- End of picture text -->



<!-- Start of picture text -->
Bank.<br><!-- End of picture text -->





Your automated facility will be placed in a “pilot” environment to allow you to initiate connectivity testing. Connectivity testing will be performed by sending files from your SFTP Server to this environment. Your Implementation Manager will help facilitate this transaction by providing your IT department with your “Client Logon” and with further technical details as outlined in Section 2. Once we have confirmed that testing is successful, we will move your facility from the pilot environment to the production environment. 



<!-- Start of picture text -->
testing is successful, we will move your facility from the pilot environment to the production environment.<br><!-- End of picture text -->



You will be setup in Manual Authorisation mode to help you facilitate your file testing. In Manual Authorisation mode, files will need to be manually approved or rejected. Once you are happy with your testing, the Bank can move your setup into Automated Authorisation mode (i.e. Straight Through Processing). 



<!-- Start of picture text -->
low value production file testing. This will be completed with the<br><!-- End of picture text -->

It is recommended that you complete low value production file testing. This will be completed with the assistance of an Implementation Manager. 



<!-- Start of picture text -->
Once your low value testing is completed you are now ready to “go live”.<br><!-- End of picture text -->

Once your low value testing is completed you are now ready to “go live”. 







<!-- Start of picture text -->
f<br><!-- End of picture text -->

5 CommBiz Automated Technical Requirements | Version 3.1 







<!-- Start of picture text -->
with you throughout the setup process. Key tasks tasks that they will be<br><!-- End of picture text -->

Your Implementation Manager will work with you throughout the setup process. Key tasks tasks that they will be involved in will be: 

#### Planning for the Implementation 

CommBiz Service Configuration 



<!-- Start of picture text -->
Providing advice on the steps you will need to follow to connect to the<br><!-- End of picture text -->



<!-- Start of picture text -->
Bank.<br><!-- End of picture text -->

Providing advice on the steps you will need to follow to connect to the Bank. 

Connectivity testing 

CommBiz file acceptance testing 

Low Value Testing 



Your IT Team will be responsible for all scripting requirements and configuring your SFTP Client or Server. 



Once your CommBiz Automated service is established, you can contact our CommBiz Support team via email at ~~diammond@cba.com.au~~ for any technical support going forward. Alternatively, the team team can also be contacted on 13 23 39. 



<!-- Start of picture text -->
the team team can also<br><!-- End of picture text -->



<!-- Start of picture text -->
be contacted on 13 23 39.<br><!-- End of picture text -->





<!-- Start of picture text -->
should revert to manual<br><!-- End of picture text -->

In an event where the connectivity with the bank is unavailable, you should revert to manual processing for file uploading and downloading via CommBiz UI. 

We strongly recommend that you create several users within CommBiz who have the permissions to import files, authorise payments. Please speak with your company CommBiz Administrator. You may want to consider building in an automated email notification to your system 

administrator so they can be made aware that manual processing will need to commence. 



<!-- Start of picture text -->
from the file before you<br><!-- End of picture text -->

If you are using PGP encryption you will need to remove the encryption from the file before you can manually upload into CommBiz UI. 



<!-- Start of picture text -->
 maximum path<br><!-- End of picture text -->

In the event of BCP for manual processing - Batch files via CommBiz Ul has a maximum path 

length of 248 characters and maximum file name length of 182 characters. 



<!-- Start of picture text -->
In the event of BCP for manual<br><!-- End of picture text -->



<!-- Start of picture text -->
processing<br><!-- End of picture text -->



<!-- Start of picture text -->
Maximum<br><!-- End of picture text -->



<!-- Start of picture text -->
sizes for<br><!-- End of picture text -->



<!-- Start of picture text -->
uploading payment<br><!-- End of picture text -->

In the event of BCP for manual processing — Maximum file sizes for manually uploading payment files: 



<!-- Start of picture text -->
Entry<br><!-- End of picture text -->



<!-- Start of picture text -->
9.25 MB.<br><!-- End of picture text -->

#### Direct Entry is 9.25 MB. 

BPAY is 9.216 MB 

FX is 9.215 MB 







<!-- Start of picture text -->
f<br><!-- End of picture text -->

6 CommBiz Automated Technical Requirements | Version 3.1 



<!-- Start of picture text -->
Technical Details<br><!-- End of picture text -->

### 6. Technical Details 



<!-- Start of picture text -->
Hardware / Software Software Requirements<br><!-- End of picture text -->

## 6.1 Hardware / Software Software Requirements 





|SFTP Client or Server|An SFTP client or server is used to send or receive files to/from the<br>Bank.|
|---|---|
||SFTP is an open standard file transfer protocol that use PKI<br>public/private keys for security. The client or server must be able to<br>use Open SSH 2048-bit RSA public keys.|
|WAN and LAN Connectivity|All files will be transferred over the internet. The quickeryour upload<br>and download speeds, the more efficient the file delivery will be.<br>The files saved on your file servers will need to be accessible within<br>your LocalArea Network (LAN) and must have the appropriate file<br>permissions for theSFTP client or server to accessthem<br>unattended.|





<!-- Start of picture text -->
Security and and Communication Protocols<br><!-- End of picture text -->

## 6.2 Security and and Communication Protocols 





|CommBizAutomated Login ID <br>(generated bythe Bank)|| Used as part oftheSFTP client configuration to initiate theSFTP<br>connection between you and the Bank.|
|---|---|
|Generation of an Open SSH<br>public/private key pair (2048<br>bit RSA)<br>(generated byyou)|You will need to generate an Open SSH public/private key pair to<br>secure yourSFTP sessions. Your 2048-bit RSAOpen SSH public<br>key must be provided tothe Bankvia email.|
|Bank to verifythe digital<br>fingerprint of the public key<br>provided byyou|The Bank will confirm to you the fingerprint of the public keywe<br>receive, soyou can verifythat the key pair has originated from within<br>your organisation.|
|Firewall changes|You may be required to add an outbound rule enabling your<br>internal SFTP client or serverthrough your firewall and outside<br>your network to connect to the CommBank infrastructure over<br>the internet.<br>Connection details:<br>IP Subnet: 140.168.0.0/16<br>TCP/IP Port: 22|





<!-- Start of picture text -->
 that the key pair has originated from within<br><!-- End of picture text -->







<!-- Start of picture text -->
y<br><!-- End of picture text -->

7 CommBiz Automated Technical Requirements | Version 3.1 





<!-- Start of picture text -->
To connect to the Bank the following activities will need to be undertaken by your your technical<br><!-- End of picture text -->



<!-- Start of picture text -->
 technical team in the<br><!-- End of picture text -->

To connect to the Bank the following activities will need to be undertaken by your your technical team in the following order. 



<!-- Start of picture text -->
following order.<br><!-- End of picture text -->

|Activity|Reason / Details|
|---|---|
|Create an OpenSSH<br>public/private key pair<br>(2048-bit RSA)|You will need to create an SSH key pair. You will keep the Private key<br>secure on your network and in your SFTP client software and provide<br>the bank with the public key. Please email yourSSH public key to your<br>Implementations Manager for onboarding.<br>Your key needs to be an OpenSSH compatible 2048-bit RSA key. Once<br>we have this key, we will confirm receipt and verify authenticity.<br>Post implementation, if you require replacement keys to be provided to<br>the Bank, you can email the CommBiz Supportteam at<br>~~diammond@cba.com.au~~|
|Establish a connection using <br>yourSFTP Client||You will need to create<br>anew connection/site in yourSFTP client or<br>server software environment:<br>Site: securetransfer.commbank.com.au<br>Login ID: Provided to you bythe Bank. CommBizService I/D<br>precededby 3zeros (e.g., 000100002001)<br>Load SSH Private key: load the Private key into the SFTP client or<br>server (this varies according to the program used)|
|Ensure your file formats are<br>compatible|Existing standard legacy file formats will be supported.<br>Automated channel size limit - LOOMB|
|Ensure your file names and<br>file types meet the<br>requirements|Please refer to Appendix 2 for the detailed file naming and file type<br>requirements.<br>File name length max 173 characters.<br>Alphanumeric allowed.<br>Specific special characters allowed depends on Operating System.<br>WinZip/file compression is supported on the automated channel.<br>PGP also has compression capability.|
|Scripting yourSFTP client<br>or server connection|Scripting will be required to automate the connection to the Bank.<br>Please refer to Appendix 1 for the flow charts that describe possible<br>steps to be taken when scripting your solution.|









<!-- Start of picture text -->
f<br><!-- End of picture text -->

8 CommBiz Automated Technical Requirements | Version 3.1 

|Use of PGP encryption for<br>additional security<br>(optional)|PGP encryption can be used ifyou require increased security for<br>delivering payment filesto and/or retrieving receivables files from the<br>Bank.<br>To use PGP,we need to exchange PGP public keys. Key elements<br>include:<br>You must create a PGP private/ public key pair.<br>You keep the PGP Private Key on your SFTP client.<br>You provide the Bank acopy ofyour public key.<br>The Bank will generate<br>a PGP key pair and send you the PGP public<br>key.<br>To encrypt files sent to you, the Bank will use your public key.<br>To encrypt files sent to the Bank, you will need to use the Bank’s public<br>key.|
|---|---|
|Folder Structures|Once connected you will see the following folder structures:|
|Name<br>Ext<br>outbox<br>inbox:<br>imt_priority_noncba_payments<br>bpay_batch_payments|/CommBiz/inbox this folder is used to collect files from the Bank<br>(e.g. receivables, status files, BAI2/BTRS).<br>It will beyour<br>responsibilityto remove/archive files from thisdirectoryonce they<br>have been downloaded successfully.<br>/CommBiz/outbox thisfolder isused to send files for processing<br>bythe bank. Filesdropped in this folder will be moved to a<br>processing area within The Bank. You will no longer have access to<br>files delivered to this folder.<br>/CommBiz/bpay_batch_payments folder is used to send BPAY<br>Payment files to the bank. Files dropped in this folder will be<br>moved to a processing areawithin the Bank. You will no longer<br>have access to files delivered to this folder.<br>/CommBiz/imt_priority_noncba_payments folder is used to send<br>IMT, Priority Payment and NonCBA payment files to the bank. Files<br>dropped in this folder will be moved to a processing area within the<br>Bank. You will no longer have access to files delivered to this folder.<br>This folder is enabled upon request.<br>Note: The inbox, outbox, imt_priority_noncba_payments &<br>bpay_batch_payments paths are case sensitive and need to be<br>populated in code exactly asshown above.|









<!-- Start of picture text -->
f<br><!-- End of picture text -->

9 CommBiz Automated Technical Requirements | Version 3.1 



<!-- Start of picture text -->
Appendix 1 —<br><!-- End of picture text -->



<!-- Start of picture text -->
Possible scripting flow charts<br><!-- End of picture text -->

Appendix 1 — Possible scripting flow charts 

Downloading Receivables Files 











<!-- Start of picture text -->
content, Transmission response, Time sent<br>- Timestamp, FileType,Checksum of file<br>CommBiz Automated Technical Requirements Requirements | Version 3.1 Version 3.1 ?éd<br><!-- End of picture text -->



10 CommBiz Automated Technical Requirements Requirements | Version 3.1 Version 3.1 









<!-- Start of picture text -->
For data files<br><!-- End of picture text -->



<!-- Start of picture text -->
sent to the Bank, filenames must conform<br><!-- End of picture text -->



<!-- Start of picture text -->
«<br><!-- End of picture text -->

For data files sent to the Bank, filenames must conform to: « 

Only alphanumeric (numbers and letters), or; 

‘* (dot) or; 

‘‘(underscore); 

Filenames should not contain spaces; 



<!-- Start of picture text -->
Maximum client filename size should conform to a maximum of 173 characters”(inclusive of the<br><!-- End of picture text -->

Maximum client filename size should conform to a maximum of 173 characters”(inclusive of the file extension). 



<!-- Start of picture text -->
file extension).<br><!-- End of picture text -->







<!-- Start of picture text -->
If sending files via CommBiz Automated, then the following file extensions will fail and the file will be<br><!-- End of picture text -->

If sending files via CommBiz Automated, then the following file extensions will fail and the file will be rejected: 



<!-- Start of picture text -->
rejected:<br><!-- End of picture text -->

JPEG, BMP, PDF, DOCX, ZIP. 



<!-- Start of picture text -->
If using CommBiz UI (Manual) to upload files and the file ends in “.zip”, “.z” or “.gz” then it must be<br><!-- End of picture text -->

If using CommBiz UI (Manual) to upload files and the file ends in “.zip”, “.z” or “.gz” then it must be a compressed file. 



<!-- Start of picture text -->
Irrespective of the upload method (Manual or Automated) Automated) CommBiz will reject the following file<br><!-- End of picture text -->

Irrespective of the upload method (Manual or Automated) Automated) CommBiz will reject the following file extensions: 



<!-- Start of picture text -->
extensions:<br><!-- End of picture text -->

WMZ, HTM, XLSX 





<!-- Start of picture text -->
f<br><!-- End of picture text -->

11 CommBiz Automated Technical Requirements | Version 3.1 

