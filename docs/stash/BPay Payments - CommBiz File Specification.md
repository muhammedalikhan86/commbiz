<mark>y</mark> a 



# **Table of contents** 

|**1.**|**File Specification**|**1**|
|---|---|---|
|**1.1**|**File Format Rules**|**1**|
|1.2|Sample File|1|
|_1.2.1_|_Single debit for multiple payments_|_1_|
|_1.2.2_|_Individual debit for each payment_|_1_|
|**1.3**|**BPay File Layout**|**2**|



# **1. File Specification** 

## **1.1 File Format Rules** 

|**#**|**Rule Description**|
|---|---|
|1|File is in CSV format, i.e. comma delimited.|
|2|The last field in each record does not close with a comma.|
|3|Fields are left-justified with no trailing spaces.|
|4|An empty field is signified by a comma immediately following the comma after the<br>previous field, e.g. “,,”.|
|5|Each record (Header, Payment Details) must end in a Carriage Return Line Feed<br>(CRLF) character (ASCII values 13 and 10).|
|6|Field formats are:<br>A = alphabetic – any letter, number of symbol.<br>AN = alphanumeric – numbers (0 to 9), “-” (hyphen/dash), “.” (full stop) and “+” (plus<br>sign).<br>N = numeric – numbers only (0 to 9).|
|7|For a single debit for multiple payments, create a file with one Header record and<br>multiple Payment Details records.|
|8|For an individual debit for each payment, create a file with both a Header record and<br>a Payment Details record for each payment.|
|9|The maximum limit of payments per file is 200.|
|10|Multiple files can be imported into CommBiz in the one action.  The maximum limit<br>of files per import is 20.|
|11|If the “Payment Date” field (field 6) is left blank it will default to today’s date (i.e. the<br>date the file is submitted).|



## **1.2 Sample Files** 

## **1.2.1 Single debit for multiple payments** 

01,20210306,103051,001,06200012345678,20210308,2,182923 

50,,,,,,,,7334,,8923037123,,,130350,,,,,,,,,,, 

50,,,,,,,,6666,,12340001756,,,52573,,,,,,,,,,, 

## **1.2.2 Individual debit for each payment** 

01,20210306,103051,001,06200012345678,20210308,1,130350 50,,,,,,,,7334,,8923037123,,,130350,,,,,,,,,,, 

01,20070306,103051,001,06412378945612,20070308,1,52573 

50,,,,,,,,6666,,12340001756,,,52573,,,,,,,,,,, 

1 

## **1.3 BPay File Layout** 

The highlighted fields are either mandatory or optional.  All other fields are for future use and are required to be empty. 

|**Field**<br>**No.**|**Field Name**|**Length**|**Format**|**Mandatory**|**Description**|
|---|---|---|---|---|---|
||**Header**|||||
|1|Record Type|2|N|Yes|"01"|
|2|File Creation<br>Date|8|N||YYYYMMDD - the date the<br>file was created.|
|3|File Creation<br>Time|6|N||HHMMSS - the time the<br>file was created.|
|4|File Number|3|N||Sequential number of file.<br>Could commence at '001'<br>each day.|
|5|Payment<br>Account|20|N|Yes|The account number of the<br>funding account.|
|6|Payment Date|8|N|Yes|YYYYMMDD - the date on<br>which the payments are to<br>be made.<br>Can be up to 15 months<br>into the future from the<br>lodgement date.<br>If left blank, will default<br>to the date the file is<br>submitted.|
|7|Number of<br>Payment<br>Records|6|N|Yes|The number of payments in<br>the file.|
|8|Total Amount<br>of Payments|12|N|Yes|The total amount of<br>payments in the file, in<br>cents.|



2 

**BPay File Layout** 

|**Field**<br>**No.**|**Field Name**|**Length**|**Format**|**Mandatory**|**Description**|
|---|---|---|---|---|---|
||**Payment**<br>**Details**|||||
|1|Record Type|2|N|Yes|"50"|
|2|File Creation<br>Date|2|N||empty|
|3|File Creation<br>Time|1|N||empty|
|4|File Number|3|A||empty|
|5|Payment<br>Account|20|AN||empty|
|6|Payment Date|3|A||empty|
|7|Number of<br>Payment<br>Records|3|A||empty|
|8|Currency Code<br>of Payment|3|A||empty|
|9|Biller Code|10|N|Yes|The BPay Biller Code of the<br>beneficiary of the payment.|
|10|Service Code|7|N||empty|
|11|Customer<br>Reference<br>Number|20|N|Yes|The account number,<br>reference number, etc. of<br>the account that is being<br>paid.|
|12|Payment<br>Method|3|N||empty|



3 

### **BPay File Layout** 

|13|Entry Method|3|N||empty|
|---|---|---|---|---|---|
|14|Amount|12|N|Yes|The amount of the<br>payment, in cents.|
|15|Transaction<br>Reference<br>Number|21|AN||empty|
|16|Original<br>Reference<br>Number|21|AN||empty|
|17|BPAY<br>Settlement<br>Date|8|N||empty|
|18|Date  Payment<br>Accepted|8|N||empty|
|19|Time  Payment<br>Accepted|6|N||empty|
|20|Payer Name|40|A||empty|
|21|Additional<br>Reference<br>Code|20|A||empty|
|22|Error<br>Correction<br>Reason|3|N||empty|
|23|Discount<br>Method|3|A|||
|24|Discount<br>Reference|20|A||empty|
|25|Discretionary<br>Data|50|A||empty|



4 

This page was intentionally left blank 

5 



This page was intentionally left blank 

6 

