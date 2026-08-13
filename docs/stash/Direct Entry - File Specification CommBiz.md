# **1. Header Record Format** 

Direct Entry payment files submitted to the Commonwealth Bank for processing require the following file structure: 

- A header record (one per user) 

- Detail records (variable number) 

- A File trailer record (one per user) 

Payment files consist of a single header record, followed by at least 2 detail records, followed by a single trailer record. Each record should be 120 characters in length. Only the final fields in the header and trailer records can be dropped; the length in this case must be 80 characters. 

Direct Entry files must include a contra entry to the user’s nominated account(s) to ensure the files are self-balancing. The last field in each record must be followed by a carriage return line feed (CRLF) character (ASCII values 13 & 10). Direct Entry files should also be self-balancing. 

## **Header Record Format** 

|**Field**<br>**#**|**Character**<br>**Position**|**Field**<br>**Length**|**Description**|**Mandatory**<br>**/ Optional**|**Comments**|
|---|---|---|---|---|---|
|1|1|1|Record Type<br>0|M|Must be “0”|
|2|2 -  18|17|Blank|M|Unused, space filled|
|3|19 - 20|2|Reel<br>Sequence<br>Number|M|“Must be numeric<br>commencing at “01”|
|4|21 - 23|3|Name Of<br>User Financial<br>Institution|M|Must be “CBA”|
|5|24 - 30|7|Blank|M|Unused, space filled|
|6|31 - 56|26|Name of User<br>Supplying File|M|Should be User preferred<br>name<br>• Must not be left blank<br>• Left justified, space<br>filled|
|7|57 - 62|6|Number<br>of User<br>Supplying File|M|User Identification Number<br>(APCA ID)<br>• Must be 6 digits<br>• Right justified, zero<br>filled|
|8|63 - 74|12|Description<br>of Entries<br>on File (e.g.<br>salaries)|M|• Must not be left blank<br>• Left justified, space<br>filled|
|9|75 - 80|6|Date to be<br>Processed|M|Must be numeric and in the<br>format of DDMMYY.<br>Must be a valid date. Zero<br>filled.|
|10|81 - 120|40|Blank|M|Unused, space filled|

# **2. Detail Record Format** 

|**Field**<br>**#**|**Character**<br>**Position**|**Field**<br>**Length**|**Description**|**Mandatory**<br>**/ Optional**|**Comments**|
|---|---|---|---|---|---|
|1|1|1|Record Type<br>1|M|Must be “1”|
|2|2-8|7|BSB Number|M|Must be in format nnn-nnn<br>where n is numeric|
|3|9-17|9|Account<br>Num-ber to<br>be Credit-ed/<br>Debited|M|• Alpha-numeric, hyphens<br>and spaces allowed only<br>• Must not contain all<br>blanks or all zeros<br>• Leading zeros, which<br>are part of an account<br>number must be shown<br>• Right justified, space<br>filled|
|4|18|1|Indicator|M|See Indicator section below|
|5|19-20|2|Transaction<br>Code|M|See Transaction Code<br>sec-tion below|
|6|21-30|10|Amount|M|• Must be 10 numeric<br>characters<br>• In cents, without<br>decimal point<br>• Right justified, zero<br>filled|
|7|31-62|32|Title of<br>Account to<br>be Credit-ed/<br>Debited|M|• Must not be left blank<br>• Left justified, space<br>filled|
|8|63-80|18|Lodgement<br>Reference<br>(e.g. Payroll<br>Number)|M|• Left justified, space<br>filled|
|9|81-87|7|Trace BSB<br>Number (BSB<br>Number<br>and account<br>number<br>of User,<br>to enable<br>retracing of<br>the entry to<br>its source if<br>necessary).|M|• Must be in format<br>nnn-nnn where n is<br>numeric|
|10|88-96|9||M|• Alpha-numeric, hyphens<br>and spaces allowed only<br>• Must not contain all<br>blanks or all zeros<br>• Leading zeros, which<br>are part of an account<br>number must be shown<br>• Right justified, space<br>filled|
|11|97-112|16|Name of<br>Remit-ter|M|Name of originator of the<br>entry.  This may vary from<br>Name of User<br>• Left justified, space<br>filled|
|12|113-120|8|Amount of<br>withholding<br>tax|M|• In cents, without<br>decimal point<br>• Right justified, zero<br>filled|

## **(1) INDICATOR** 

Must be one of “N”, “W”, “X”, “Y” or blank. Care should be exercised to ensure inclusion of “N” symbol. Failure to do so may render the user liable in the event that incorrect processing occurs as a result. 

However, for Withholding Tax, valid indicators are: 

Care should be exercised to ensure inclusion of: 

|**Code**|**Description**|
|---|---|
|"W"|Dividend paid to a resident of a country where a double tax agreement is in force|
|"X"|Dividend paid to a resident of any other country|
|"Y"|Interest paid to all non-residents|

## **(2) TRANSACTIONS CODES** 

|**Code**|**Description**|
|---|---|
|"13"|Externally initiated debit item|
|"50"|Externally initiated credit items with the exception of those items bearing<br>transaction|
|**Codes**|**“51” – “57” inclusive**|
|"51"|Australian Government Security Interest|
|"52"|Family Allowance|
|"53"|Pay|
|"54"|Pension|
|"55"|Allotment|
|"56"|Dividend|
|"57"|Debenture/Note Interest|

# **3. Trailer Record** 

|**Field**<br>**#**|**Character**<br>**Position**|**Field**<br>**Length**|**Description**|**Mandatory**<br>**/ Optional**|**Comments**|
|---|---|---|---|---|---|
|1|1|Record<br>Type 7|M|Must be<br>“7”|Must be “0”|
|2|2-8|7|BSB Number|M|Placeholder value and must<br>be 999-999|
|3|9-20|12|Blank|M|Unused, space filled|
|4|21-30|10|File (User)<br>Net Total<br>Amount|M|Must be the diference<br>between the File Credit and<br>File Debit total amounts<br>• In cents, without<br>decimal point<br>• Right justified, zero<br>filled<br>• Unsigned|
|5|31-40|10|File (User)<br>Credit Total<br>Amount|M|Must equal the<br>accumulated total of credit<br>Detail Record amounts<br>• In cents, without<br>decimal point<br>• Right justified, zero<br>filled<br>• Unsigned|
|6|41-50|10|File (User)<br>Debit Total<br>Amount|M|Must equal the<br>accumulated total of credit<br>Detail Record amounts<br>• In cents, without<br>decimal point<br>• Right justified, zero<br>filled<br>• Unsigned|
|7|51-74|24|Blank|M|• Unused, space filled|
|8|75-80|6|File (User)<br>Count of<br>Record Type<br>1|M|Must equal accumulated<br>number of Record Type 1<br>(Detail records) on the File<br>• Numeric only<br>• Right justified, zero<br>filled|
|10|81-120|40|Blank|M|Unused, space filled|

**4. Allowable Character Set Codes** 

|**Field**<br>**#**|**Character**<br>**Position**|**Field**<br>**Length**|**Comments**|
|---|---|---|---|
|Space||32|20|
|Exclamation|!|33|21|
|Hash|#|35|23|
|Dollar|$|36|24|
|Percent|%|37|25|
|Ampersand|&|38|26|
|Apostrophe|‘|39|27|
|Dash|-|45|2D|
|Dot|.|46|2E|
|Fwd Slash|/|47|2F|
|All Numbers|0-9|48-57|30-39|
|Colon|:|58|3A|
|Semicolon|;|59|3B|
|Equal|=|61|3D|
|Question|?|63|3F|
|At|@|64|40|
|All alpha|A-Z|65-90|41-49, 4A-F, 50-59, 5A|
|All alpha|a-z|97-122|61-69, 6A-F, 70-79, 7A|
|Underscore|_|95|5F|
|Asterisk|*|42|2A|
|Parenthesis|()|40,41|28, 29|
|Plus|+|43|2B|
|Comma|,|44|2C|
|Tide|~|126|7E|

# **5. Sample File** 

0 01CBA COMPANY ABCD PTY LTD 301500EFT-PAYMENT 051206 

1062-000 10001000 530000010050CLIENT COMPANY XYZ INVOICE 123456 063-000100000000COMPANY ABCD P/ L00000000 

1063-000 10000000 130000010050COMMPANY ABCD PTY LTD PAYMENT    063-000100000000COMPANY ABCD P/ L00000000 

7999-999 000000000000000100500000010050 000002
