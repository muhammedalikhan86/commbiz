# 4. File Structure
File Type: Variable Length Comma Delimited, UTF-8  
The file consists of 7 record types. These are:

| Record Type | Description                | No. of Fields | Freq | Mandatory/Optional | Comments                                                                 |
|-------------|----------------------------|---------------|------|--------------------|--------------------------------------------------------------------------|
| 01          | File Header Record         | 9             | 1    | Mandatory          | It must be the first record of the file. It features the file delivery date and time |
| 02          | Group Header Record        | 8             | Many | Mandatory          | It features the processing date                                           |
| 03          | Account Header Record      | 31            | Many | Mandatory          | This record is intended to classify the transactional data by account.       |
| 16          | Transaction Detail Record  | 7             | Many | Optional           | One record for each transaction item. Supports UTF-8 data                  |
| 49          | Account Trailer Record     | 3             | Many | Mandatory          | The Account Trailer record provides account level control totals.           |
| 98          | Group Trailer Record       | 4             | Many | Mandatory          | The Group Trailer record provides group level control totals.              |
| 99          | File Trailer Record        | 4             | 1    | Mandatory          | The File Trailer record provides file control totals.                      |

## 4.1 Field Notations
A number of notations are used in describing the field properties for each record type. They are tabulated as follows:

| Notation | Description                                                                 |
|----------|-------------------------------------------------------------------------------|
| AN       | Denotes an alphanumeric data type under the Type column.                     |
| Date     | Denotes a date field under the Type column. The date is to be presented in CCYYMMDD format. |
| N        | Denotes a numeric field under the Type column.                                |
| M        | A mandatory field is represented by a value of 'M' under the Man/Opt column.   |
| O        | An optional field is represented by a value of 'O' under the Man/Opt column.   |
| UTF-8    | UTF-8 is a variable-width character encoding used for electronic communication. Examples of UTF-8 encoded data is smiles ☺ , Latin characters etc. |

# 4.2 File Naming Convention

The default filename is:

BTBS-<Receiver ID>-<yyyy><MM><dd><HH><mm><ss><fff>

Receiver ID (see Record Type 01 Field #03 below)

Examples:

CommBiz Automated – BTBS-MAILBOX1-20161018054040073  
CommBiz UI – BTBS-MAILBOX1-20161018054040073.txt

# 4.3 Supported Accounts

This file type provides balances and transactions for standard transaction accounts and business loans. Credit Cards and Home Loans are not supported. If you are unsure, please contact the bank.

# 4.4 A Note about Amount Fields

Fields that contain amounts are always represented as an integer in the smallest unit of the designated currency. For a currency with 2 decimal places, the rightmost 2 digits will represent the ‘cents’. For example, the amount 1234 could be understood as one thousand two hundred and thirty four Japanese Yen, or as Twelve dollars and thirty four cents, depending on the designated currency.

If a field is designated as being signed, then positive numbers will not have a sign, but negative numbers will have a minus sign (“-”) immediately to the left of the digits in the number.

# 4.5 Record Type 01 File Header Record
| Field # | Field Description | Field Details | Comments / Purposes | Sample Data BTRS |
|---------|-------------------|---------------|---------------------|------------------|
|         |                  | Man/Opt       | Min/Max Length      |                  |
| 01      | Record Type       | M             | N                   | 2/2              | Marks the beginning of the file. It has a constant value of 01. |
|         |                   |               |                     |                  | 01 |
| 02      | Sender Identification | M | AN | 3/3 | Identifies sender of the collection file. It has a constant value of CBA. |
|         |                   |               |                     |                  | CBA |
| 03      | Receiver Identification | O | AN | 1/8 | An identifier for the receiver. It can be set to anything that the customer requires (up to 8 characters). |
|         |                   |               |                     |                  | MAILBOX1 |
| 04      | File Creation Date | M | Date | 6/6 | Contains file creation date in YYYYMMDD format. |
|         |                   |               |                     |                  | 160126 |
| 05      | File Creation Time | M | N | 4/4 | Contains file creation time stamp in HHMM format. Right justified and zero filled. |
|         |                   |               |                     |                  | 0530 |
| 06      | File Identification Number | M | N | 1/3 | A sequence number commencing from 1, and to be incremented by 1. Its purpose is to identify one or more collection files produced on a business day. |
|         |                   |               |                     |                  | 1, 2, ... 999 |
| 07      | Physical Record Length | O | N | 0 | Currently defined as NULL. |
|         |                   |               |                     |                  |          |
| 08      | Physical Block Size | O | N | 0 | Currently defined as NULL. |
|         |                   |               |                     |                  |          |
| 09      | Version Number | M | N | 1/1 | The version number is BAI version number 3. |
|         |                   |               |                     |                  | 3   |

# 4.6 Record Type 02 Group Header Record
| Field # | Field Description       | Field Details | Comments / Purposes                                                                 | Sample Data BTRS |
|---------|-------------------------|---------------|--------------------------------------------------------------------------------------|------------------|
|         |                         | Man/Opt       | Type                                                                               | Min/Max Length   |
| 01      | Record Type             | M             | N                                                                                 | 2/2              | Denotes a Group Header record. It has a constant value of 02. |
| 02      | Ultimate Receiver Identification | O | N | 0/0 | Currently defined as NULL. |
| 03      | Originator              | M             | AN                                                                                | 3/3              | Denotes the originator of transaction data. It has a constant value of CBA. |
| 04      | Group Status            | M             | N                                                                                 | 1/1              | Set group status to 1. |
| 05      | As of Date (Settlement Date/Cutoff Date) | M | Date | 6/6 | Settlement date for BTRS data. Presented in YYMMDD format. |
| 06      | As at Time              | O             | N                                                                                 | 0/0              | Corresponds to the time stamp component, presented in HHMM format. This field will be right justified and zero filled. Currently defined as NULL. |
| 07      | Currency                | O             | N                                                                                 | 3/3              | Currently set to AUD. |
| 08      | As of Date Modifier     | M             | N                                                                                 | 1/1              | Contains a code to indicate the 'As of Date' modifier: 1 = interim/previous day 2 = final/previous day 3 = interim/same day 4 = final/same day |

# 4.7 Record Type 03 Account Header Record
| Field # | Field Description         | Man/Opt | Field Details | Min/Max Length | Comments/Purposes                                                                 | Sample Data BTRS |
|---------|---------------------------|---------|---------------|----------------|--------------------------------------------------------------------------------------------------------------------------|------------------|
| 01      | Record Type               | M       | N             | 2/2            | Denotes an Account Header record. It has a constant value of 03.                                                                      |                  |
| 02      | Customer Account Number   | M       | N             | 14/24          | A unique reference to identify a CBA bank account number. The field comprises two parts; an 6 character BSB and an 8 character account number. | 06218110 068083  |
| 03      | Currency Code             | O       | AN            | 0/3            | Null for accounts in the same currency as defined in the 02 record or the three character currency code for accounts in other currencies (e.g., USD for US dollar accounts). | USD EUR          |
| 04      | Closing Balance Type Code | M       | N             | 3/3            | Type code for the Closing Balance. It has a constant value of 015.                                                                    |                  |
| 05      | Closing Balance Amount    | M       | Numeric       |                | Contains the Closing Balance for the specified date in field 05 of the 02 Group Header Record. This is an amount field and follows the conventions defined in the "Note about Amount Fields" (above). | 12345 -1020202   |
| 06      | Total Items Count         | O       | AN            | 0/0            | Currently set as NULL.                                                                                                 |                  |
| 07      | Total Funds Type          | O       | AN            | 0/0            | Currently set as NULL.                                                                                                 |                  |
| 08      | Total Credits Type Code   | M       | N             | 3/3            | Type code for total credits in this 03 Account Header Record. It has a constant value of 100.                                   | 100              |
| 09      | Total Credits Amount      | M       | Numeric       |                | Total value of credit transactions in this 03 Account Header Record. This is an amount field and follows the conventions defined in the "Note about Amount Fields" (above). | 12345, 20202     |
| 10      | Total Credit Items Count  | M       | Numeric       |                | Total number of credit items in this 03 Account Header record.                                                                     | 1,150,...        |
| 11      | Total Credit Funds Type   | O       | AN            | 0/0            | Currently defined as NULL.                                                                                                |                  |
| 12      | Total Debits Type Code    | M       | N             | 3/3            | Type code for total debits in this 03 Account Header Record. It has a constant value of 400.                                    | 400              |

| Field # | Field Description | Field Details | Comments/ Purposes | Sample Data BTR5 |
|---------|-------------------|---------------|--------------------|------------------|
| 13      | Total Debits Amount | M/N           | Numeric            | Total value of debit transactions in this 03 Account Header Record. This is an amount field and follows the conventions defined in the "Note about Amount Fields" (above). | 12345, 20202 |
| 14      | Total Debit Items Count | M/N           | Numeric            | Total number of debit items in this 03 Account Header Record. | 1,150,... |
| 15      | Total Debit Funds Type | O             | AN                 | 0/0              | Currently defined as NULL |
| 16      | Accrued Debit Interest Type Code | M/N           | 3/3                | Type code for Accrued Debit Interest in this 03 Account Header Record. It has a constant value of 900. | 900 |
| 17      | Accrued Debit Interest Amount | M/N           | Numeric            | Accrued Debit Interest Amount This is an amount field and follows the conventions defined in the "Note about Amount Fields" (above). | 000 |
| 18      | Accrued Debit Interest Item Count | O             | N                  | 0/0              | Currently defined as NULL |
| 19      | Accrued Debit Interest Funds Type | O             | AN                 | 0/0              | Currently defined as NULL |
| 20      | Accrued Credit Interest Type Code | M/N           | 3/3                | Type code for Accrued Credit Interest in this 03 Account Header Record. It has a constant value of 901. | 901 |
| 21      | Accrued Credit Interest Amount | M/N           | Numeric            | Accrued Credit Interest Amount This is an amount field and follows the conventions defined in the "Note about Amount Fields" (above). | 000 |
| 22      | Accrued Credit Interest Item Count | O             | N                  | 0/0              | Currently defined as NULL |
| 23      | Accrued Credit Interest Funds Type | O             | AN                 | 0/0              | Currently defined as NULL |
| 24      | Credit Limit Type Code | M/N           | 3/3                | Type code for credit limit on credit/corporate card facilities in this 03 Account Header Record. It has a constant value of 904. | 904 |

| Field # | Field Description | Field Details | Field Details | Comments/ Purposes | Sample Data BTR5 |
|---------|-------------------|---------------|---------------|--------------------|------------------|
|         |                  | Man/Opt       | Type          | Min/Max Length     |                  |
| 25      | Credit Limit Amount | M | N | Numeric | Currently defined as NULL for AUD and foreign currency bank accounts. This is an amount field and follows the conventions defined in the "Note about Amount Fields" (above). |  |
| 26      | Credit Limit Count | M | N | 0/0 | Currently defined as NULL |  |
| 27      | Credit Limit Funds Type | O | AN | 0/0 | Currently defined as NULL |  |
| 28      | Interest Rate Type Code | M | N | 3/3 | Type code for Interest Rate on credit/corporate card facilities in this 03 Account Header Record. It has a constant value of 905. | 905 |
| 29      | Interest Rate Amount | M | N | Numeric | Currently defined as NULL for AUD and foreign currency bank accounts |  |
| 30      | Interest Rate Count | M | N | 0/0 | Currently defined as NULL |  |
| 31      | Interest Rate Funds Type | O | AN | 0/0 | Currently defined as NULL |  |

# 4.8 Record Type 16 Transaction Detail Record

| Field # | Field Description       | Man/Opt | Field Details | Comments/Purposes                                                                 | Sample Data BTRS      |
|---------|-------------------------|---------|---------------|--------------------------------------------------------------------------------------------------------------------------|-----------------------|
| 01      | Record Type             | M       | AN            | 2/2                                                                                                                           | 16                    |
|         |                        |         |               | Denotes a Transaction Detail record. It has a constant value of 16.                                                                |
| 02      | BTRS Type Code          | M       | N             | 3/3                                                                                                                           | 165, 475, 930        |
|         |                        |         |               | The type code indicates the type of transaction. For a list of the subset of standard BTRS codes used by CBA, and the customized codes, see Appendix 1. |
|         | NPP Specific details   |         |               | NPP transactions will be identified with four new numeric codes: 948, 949, 988 & 989. See Appendix 1 for further information |
|         | PayTo Specific details |         |               | PayTo transactions will be identified with four new numeric codes 990, 991, 992 & 993. See Appendix 1 for further information |
| 03      | Amount                  | M       | Numeric       | 12345, 200100                                                                                                                 |
|         |                        |         |               | The value of the transaction This is an unsigned amount field and follows the conventions defined in the "Note about Amount Fields" above. Whether this is a debit or credit should be derived from the Type code (field 2) |
| 04      | Fund type               | O       | N             | 0/0                                                                                                                           | NULL                  |
|         |                        |         |               | Currently defined as NULL                                                                                                     |
| 05      | Bank Reference Number   | O       | AN            | 1/20                                                                                                                          | D5335031 95211234    |
|         |                        |         |               | Contains unique bank transaction reference ID                                                                               |                      |

| Field # | Field Description | Man/Opt | Field Details | Type | Min/ Max Length | Comments/ Purposes | Sample Data BTRs |
|---------|-------------------|---------|---------------|------|-----------------|--------------------|------------------|
| 06      | Customer Reference Field | M | UTF-8 | 0/50 | The customer reference field may contain a reference traceable by the customer. Different type codes will contain different types of reference. For example a cashed cheque will contain the cheque number. The list of type codes in Appendix 1 includes details of what customer reference is included for each type code. NPP Specific details: NPP end-to-end ID will be populated, up to 35 characters in length, if available. NPP end-to-end ID is assigned by the sending bank and remains unchanged throughout the end-to-end chain. Some banks may assign the Direct Entry Lodgement Reference as the NPP end-to-end ID, for NPP payments processed to a BSB and account number. PayTo Specific details: For credit payments, Creditor reference will be populated if present, else will be set to End-To-End-ID |
| 07      | Text               | M | UTF-8 | 0/2000 | This field contains a complete narrative for the transaction. Where there is more than 1 line of narrative, lines are separated by a pipe character ("|"). This narrative will include both information from CBA and also potentially details supplied by the person creating the transaction. Size is increased to support any additional data in future. This BTRS report has specific narratives defined for every type of transaction. See section 4.8.1 for more information. |  |

| Field # | Field Description | Man/Opt | Type | Min/ Max Length | Comments/ Purposes | Sample Data BTRS |
|---------|-------------------|---------|------|----------------|--------------------|------------------|
|         |                  |         |      |                 | PayTo specific changes: Append Creditor Reference (if present) to Text field prefixed by a pipe symbol (please refer to field #16 below) |  |
|         |                  |         |      |                 | PayTo and NPP specific changes: Append Payment Service to Text field prefixed by pipe symbol (please refer to field #17 below) |  |

# 4.8.1 Text field specification (Record Type 16, Field 7)

Text field is separated by " character with allocated positions to display enriched transaction information.

Information mentioned as optional will be present in the report if available. There are also placeholder positions defined to support any future additions to the information being presented.

The report specification will be amended to provide additional data when it becomes available in the placeholder fields.

| Field # | Field Name                | Man/Opt | Type | Min/Max Length | Comments/Purposes                                                                 | Sample Data                                                                 |
|---------|---------------------------|---------|------|----------------|--------------------------------------------------------------------------------------------------------------------------------------------------|-----------------------------------------------------------------------------|
|         | Transaction Narrative - 1 | M       | Text | 70             | Transaction Information (Bank provided)<br>NPP transactions may contain narrative including 'transfer to/from' (depending on sending bank) <PaylD name> | Transfer From DAVID BLACK<br>Transfer To S WILSON<br>Reversal<br>Returned Payment |
|         | Transaction Narrative - 2 | O       | Text | 70             | Transaction Information (Bank/Customer provided)<br>NPP transactions may contain narrative PaylD <PaylD type> from <Sender initiating application> | 99618025865 3<br>PaylD Phone from CommBank App<br>Transfer To ian king gee<br>Original transaction date 07-05-2020 |
|         | Transaction Narrative - 3 | O       | Text | 70             | Transaction Information (Bank/Customer provided)<br>NPP transactions may contain narrative up to 280 character free text. | Sammy wages<br>Account closed<br>Original transaction date 07-05-2020 |
|         | Remitter Name             | O       | Text | 70             | Remitter Name if available.                                                                                                                     | N D Parker                                                                    |
|         | Transaction Long description | O       | Text | 280            | Customer provided - for NPP transactions.                                                                                                      | 99618025865 3<br>Sammy wages                                                |
|         | Debtor ID                 | O       | Text | 256            | This position displays various types of Debtor/Payer ID populated for NPP credit transactions.                                                   | abcd@email.com<br>0401113333<br>552033xxxxxx2707<br>912301234444777789   |

| Field # | Field Name | Field Technical Details | Comments/Purposes | Sample Data BTRS |
|---------|------------|--------------------------|-------------------|------------------|
|         |            | Man/Opt                  | Type              | Min/ Max Length  |                  |
|         |            |                          |                   |                  | Debtor account – When NPP transaction is initiated from Debtor account. |
|         |            |                          |                   |                  | Card token      |
|         |            |                          |                   |                  | Masked card     |
|         |            |                          |                   |                  | PayID Identifier|
| 7       | Debtor Name| O                        | Text              | 140              | A B Joshi       |
|         |            |                          |                   |                  | S Parker        |
| 8       | PaylD Type | O                        | Text              | 30               | EMAL             |
|         |            |                          |                   |                  | TELI            |
|         |            |                          |                   |                  | AUBN etc.       |
| 9       | PaylD Identifier| O                        | Text              | 256              | abcde@emai.com  |
|         |            |                          |                   |                  | 0401111333      |
| 10      | PaylD Name | O                        | Text              | 140              | Tony So         |
|         |            |                          |                   |                  | S Jones         |
| 11      | ISO Reason Code| O                        | Text              | 4                |                  |
|         |            |                          |                   |                  | Displayed for returned and rejected transactions. Includes but not limited to the below (As per NPP standard): AC03 – No Account AC07 – Account Closed BE06 – Refer to Customer |
| 12      | Number of cheques| O                        | Text              | 16               | Num Chqs 1      |
|         |            |                          |                   |                  | Reserved field for future changes to display number cheques for cheque deposit transactions. |
| 16      | Creditor reference| O                        | Text              | 35               |                  |
|         |            |                          |                   |                  | PayTo Transactions: Creditor reference will be populated if it is present. |
| 17      | Payment Service| O                        | Text              | 35               |                  |
|         |            |                          |                   |                  | PayTo and NPP Transactions: Payment Service will be populated. |

| Field # | Field Name | Field Technical Details | Comments/Purposes | Sample Data BTRS |
|---------|------------|--------------------------|-------------------|------------------|
| 18–50   | Reserved positions for future expansion | Man/Opt O Text TBD1 Currently defined as NULL Concatenated pipes | to indicate 37 placeholder variable fields reserved for future information | / |
| 51      | Last field indicator | M Text 1 Indicate end of the transaction description | / |

Note: 13–15 are already reserved for the fields: category purpose , _usi_text , _usi_identifier as part of previous releases.

# 4.9 Record Type 49 Account Trailer Record
| Field # | Field Description | Field Details | Comments/Purposes | Sample Data BTRS |
|---------|-------------------|---------------|-------------------|------------------|
| 01      | Record Type       | M/N           | N                | 2/2              | Denotes an Account Trailer record. It has a constant value of 49. |
|         |                   |               |                  |                  |                 |
| 02      | Account Control Total | M | N | Numeric | Contains the total amount of all the amount fields in the preceding 03 and 16 record types. This is a signed amount field and follows the conventions defined in the "Note about Amount Fields" (above). | 7200. - 430050 |
|         |                   |               |                  |                  |                 |
| 03      | Number of Records | M | N | Numeric | Contains the count of records for this account including the 03, 16 and 49 record. | 106            |

# 4.10 Record Type 98 Group Trailer Record
| Field # | Field Description | Field Details | Comments/Purposes | Sample Data BTRS |
|---------|-------------------|---------------|-------------------|------------------|
| 01      | Record Type       | M             | N                | 2/2              | Denotes a Group Trailer record. It has a constant value of 98. |
|         |                   |               |                  |                  |                 |
| 02      | Group Control Total | M | N | Numeric | Contains the total amount of all the amount fields in the preceding 49 record types. This is a signed amount field and follows the conventions defined in the "Note about Amount Fields" (above) | 99887766. - 999878 |
|         |                   |               |                  |                  |                 |
| 03      | Number of Accounts | M | N | Numeric | The number of 03 records in this group | 3              |
|         |                   |               |                  |                  |                 |
| 04      | Number of Records | M | N | Numeric | Contains the total number of records for this group including the 02, 03, 16, 49 and 98 records. | 18             |

# 4.11 Record Type 99 File Trailer Record

| Field # | Field Description       | Field Details | Comments/ Purposes                                                                 | Sample Data BTRS      |
|---------|-------------------------|---------------|------------------------------------------------------------------------------------------------------------------|-----------------------|
| 01      | Record Type             | M N           | Denotes a File Trailer record. It has a constant value of 99.                                                            | 99                    |
| 02      | File Control Total      | M N           | Numeric Contains the total amount of all the amount fields in the preceding 98 record type/s This is a signed amount field and follows the conventions defined in the "Note about Amount Fields" above. | 99887766, -999878     |
| 03      | Number of Groups        | M N           | Numeric Number of 02 records in this file                                                                                  | 1                     |
| 04      | Number of Records       | M N           | Numeric Contains the total number of records in this file including this 99 record.                                   | 79                    |

# Appendix 1: Common BTRS codes

Type codes within the BTRS standard are used to define the meaning of amounts within the 03 and 16 record types. Type codes can be grouped into 'ranges', with all type codes within a range having similar meanings. A list of these ranges and their meanings is given below.

| Type Codes | Description |
|-------------|-------------|
| 001–099     | Account status type codes |
| 100         | Total credits summary type code |
| 101–399     | Credit summary and detail type codes |
| 400         | Total debits summary type codes |
| 401–699     | Debit summary and detail type codes |
| 700–799     | Loan summary and detail type codes |
| 900–919     | Custom account status type codes. Specific to CBA only |
| 920–959     | Custom credit summary and detail. Specific to CBA only |
| 960–999     | Custom debit summary and detail. Specific to CBA only |

The following list of transaction codes is complete at the time of publication, but other codes may be introduced in future.

| BTRS Code | Description | Dr/Cr | Customer Reference (Field 6) |
|-----------|-------------|-------|-----------------------------|
| 115       | Lockbox Deposit | CR    |                             |
| 165       | Direct Entry received | CR    |                             |
| 171       | Loan Deposit | CR    |                             |
| 174       | Other Deposit | CR    |                             |
| 175       | Cheque and Cash Deposit | CR    | Agent Number               |
| 187       | Bank Cheque | CR    |                             |
| 201       | Transfer - Automatic | CR    |                             |
| 206       | Money Transfer – CBA-CBA | CR    |                             |
| 208       | Money Transfer – IMT/RTGS OFI | CR    | CBA Reference number       |
| 215       | Trade Finance Settlement | CR    |                             |
| 237       | Direct Debit Settlement | CR    | Trace Account              |
| 242       | Collection of Interest Income | CR    |                             |
| 244       | Interest/Matured Principal Payment | CR    |                             |
| 252       | Reversal | CR    |                             |
| 255       | Cheque Return | CR    |                             |
| 257       | Return of an outbound direct entry | CR    |                             |
| 275       | Sweep transaction (ZBA = zero balance account) | CR    |                             |
| 349       | Principal Payments | CR    |                             |
| 354       | Interest | CR    |                             |

| BTRS Code | Description | Dr/Cr | Customer Reference (Field 6) |
|-----------|-------------|-------|-----------------------------|
| 357       | Adjustment  | CR    |                             |
| 398       | Fee – Reversal | CR    |                             |
| 399       | Miscellaneous Credit | CR    |                             |
| 455       | Outbound direct entry | DR    |                             |
| 475       | Presented Cheque | DR    | Cheque number               |
| 477       | Bank Prepared Debit | DR    |                             |
| 481       | Loan Payment | DR    |                             |
| 501       | Transfer - Automatic | DR    |                             |
| 506       | Money Transfer - CBA-CBA | DR    |                             |
| 508       | Money Transfer - IMT/RTGS OFI | DR    | CBA Reference number        |
| 512       | Trade Finance Settlement | DR    |                             |
| 514       | Travel Money Purchase | DR    |                             |
| 552       | Reversal | DR    |                             |
| 557       | Return of an inbound direct entry | DR    | Trace Account               |
| 568       | Returned Cheque | DR    |                             |
| 575       | Sweep transaction (ZBA = zero balance account) | DR    |                             |
| 595       | Cash Withdraw CBA (branch, ATM) and OFI ATM | DR    |                             |
| 631       | Adjustment | DR    |                             |
| 654       | Interest | DR    |                             |
| 658       | Principal Payments | DR    |                             |
| 696       | Collections | DR    |                             |
| 698       | Fees - Charged | DR    |                             |
| 699       | Miscellaneous Debit | DR    |                             |
| 920       | Merchant Settlement | CR    |                             |
| 925       | Card Transaction - Purchase Refunds | CR    |                             |
| 926       | Scheme Debit Chargeback | CR    |                             |
| 930       | BPAY Settlement (Biller) | CR    |                             |
| 931       | BPAY Return (Returned Payment) | CR    |                             |
| 939       | Salary Payment | CR    |                             |
| 940       | eLockbox Settlement | CR    | Number of debits and credits |
| 941       | Disability Pension | CR    |                             |
| 942       | Family Allowance | CR    |                             |
| 943       | Unemployment Benefit | CR    |                             |
| 944       | Age Pension | CR    |                             |
| 945       | Carer's Pension | CR    |                             |
| 946       | Service Pension | CR    |                             |

| BTRS Code | Description | Dr/Cr | Customer Reference (Field 6) |
|-----------|-------------|-------|-----------------------------|
| 947       | Government Contribution | CR    |                             |
| 948       | Inbound NPP Payment     | CR    |                             |
| 949       | Returned Outbound NPP Payment | CR    |                             |
| 956       | Dividend               | CR    |                             |
| 960       | Merchant Settlement Debit | DR    |                             |
| 962       | Cheque Issuance Facility (Bulk transfer) | DR    |                             |
| 963       | Fee Redirection        | DR    |                             |
| 964       | Card Transaction – Purchase | DR    |                             |
| 966       | Scheme Debit Representation Sale Chargeback | DR    |                             |
| 970       | BPAY Payment           | DR    |                             |
| 971       | BPAY Settlement Refund (Biller) | DR    |                             |
| 980       | eLockbox Settlement Debit | DR    | Number of debits and credits |
| 988       | Outbound NPP Payment   | DR    |                             |
| 989       | Returned Inbound NPP Payment | DR    |                             |
| 990       | Inbound PayTo Payment  | CR    |                             |
| 991       | Returned Outbound PayTo Payment | CR    |                             |
| 992       | Outbound PayTo Payment | DR    |                             |
| 993       | Returned Inbound PayTo Payment | DR    |                             |

### 6. Appendix 2: Changes from the CBA BAII File Spec

For customers who are transitioning from CBA’s BAII reports, here is a summary of what has changed.

- **File Header (01) Record**
  - The version number has incremented to 3

- **Account Header (03) Record**
  - Removal of totals 902 and 903 – 8 files in total removed from this record type
  - Account ID now includes the full 6 digit BSB. Previously the “06” was dropped off the front of the BSB.

- **Transaction Detail (16) Record**
  - Field 2 now contains a true transaction type code, instead of a ‘miscellaneous credit’ (399) or ‘miscellaneous debit’ (699) code. See Appendix 1 for details of these codes and their meanings. These are much more fine grained than those used previously.
  - Field 5 used to contain either a numeric or alphanumeric code indicating a transaction type. This has been replaced with a unique transaction ID – this number can be quoted in any queries with the bank and will unambiguously identify the transaction in question.
  - Field 6 contains a customer reference which will vary by transaction type. This field is now populated for a greater number of transactions than in the BAII format. – Appendix 1 includes the meaning of this field for each transaction type.

  > Field 7 contains a richer transaction description than was available in the BAII format. The description in BAII was limited to 34 characters in total. BTR5 descriptions are fuller, and may contain multiple lines of description, delimited with a pipe character `|`  
> 
> **Account Trailer (98) Record**  
The Number of accounts (field 3) has been added to this record type, bringing it into line with the international standard.

# 6.1 Sample data file
01,CBA,,200914,1230,1,,,3/ 
02,,CBA,1,200911,,AUD,2/ 
03,06200014628225,,015,643796176,,,100,23269,7,,400,4400,11,,900,000,,,901,000,,,904,,,,905,,,/ 
16,257,4499,,D025200000326001NPA,06200014767356,Return Invalid BSB Number|CRS_NPP TYPE 2|CRS_NPP TYPE 2|DE INVALID 
BSB|||||||||||||||||||||||||||||||||||||||||||||||/ 
16,257,4497,,D025200000327002NPA,06200014767356,Return Invalid BSB Number|CRS_NPP TYPE 2|CRS_NPP TYPE 2|DE INVALID 
BSB|||||||||||||||||||||||||||||||||||||||||||||||/ 
16,257,4492,,D025200000332001NPA,06200014767356,Return Invalid BSB Number|CRS_NPP TYPE 2|CRS_NPP TYPE 2|DE INVALID 
BSB|||||||||||||||||||||||||||||||||||||||||||||||/ 
16,257,4491,,D025200000333001NPA,06200014767356,Return Invalid BSB Number|CRS_NPP TYPE 2|CRS_NPP TYPE 2|DE INVALID 
BSB|||||||||||||||||||||||||||||||||||||||||||||||/ 
16,257,4490,,D025200000334001NPA,06200014767356,Return Invalid BSB Number|CRS_NPP TYPE 2|CRS_NPP TYPE 2|DE INVALID 
BSB|||||||||||||||||||||||||||||||||||||||||||||||/ 
16,990,400,,K140000007800_00_00_00_000001, 45678 creditor reference,Approved Payment Return|IFW AUTO MATION|||NPP-70216|06226811341898|IFW AUTO MATION   
||||FOCR|||||45678 creditor reference|sct||||||||||||||||||||||||||||||||||/ 
16,992,400,,K140000007801_00_00_00_000001,NOTPROVIDED,Approved Payment Return|IFW AUTO MATION|||NPP-70217|06226811341898|IFW AUTO MATION   
||||FOCR||||||sct||||||||||||||||||||||||||||||||||/ 
16,988,400,,C200911065643_00_00_00,NOTPROVIDED,Transfer To Scott Roberts Sean|PayID Email from CommBank App|BDP-T3606_CBA_To_OFI  CCTI_CBA to OFI...||BDP-
T3606_CBA_To_OFI  CCTI_CBA to OFI HP- SCT_BSBACCTransferusingNPP_160944467|||EMAL|leetest@npp.com|Scott Roberts Sean and Julia Jackson 
TonyJones2|||||||||||||||||||||||||||||||||||||||||/ 
16,988,400,,C200911065403_00_00_00,NOTPROVIDED,Transfer To Scott Roberts Sean|PayID Email from CommBank App|BDP-T3606_CBA_To_OFI  CCTI_CBA to OFI...||BDP-
T3606_CBA_To_OFI  CCTI_CBA to OFI HP- SCT_BSBACCTransferusingNPP_160910110|||EMAL|leetest@npp.com|Scott Roberts Sean and Julia Jackson 
TonyJones2|||||||||||||||||||||||||||||||||||||||||/ 
16,988,400,,C200911065425_00_00_00,NOTPROVIDED,Transfer To Scott Roberts Sean|PayID Email from CommBank App|BDP-T3606_CBA_To_OFI  CCTI_CBA to OFI...||BDP-
T3606_CBA_To_OFI  CCTI_CBA to OFI HP- SCT_BSBACCTransferusingNPP_160931925|||EMAL|leetest@npp.com|Scott Roberts Sean and Julia Jackson 
TonyJones2|||||||||||||||||||||||||||||||||||||||||/ 
16,988,400,,C200911065627_00_00_00,NOTPROVIDED,Transfer To Scott Roberts Sean|PayID Email from CommBank App|BDP-T3606_CBA_To_OFI  CCTI_CBA to OFI...||BDP-
T3606_CBA_To_OFI  CCTI_CBA to OFI HP- SCT_BSBACCTransferusingNPP_160928262|||EMAL|leetest@npp.com|Scott Roberts Sean and Julia Jackson 
TonyJones2|||||||||||||||||||||||||||||||||||||||||/ 
16,988,400,,C200911065617_00_00_00,NOTPROVIDED,Transfer To Scott Roberts Sean|PayID Email from CommBank App|BDP-T3606_CBA_To_OFI  CCTI_CBA to OFI...||BDP-
T3606_CBA_To_OFI  CCTI_CBA to OFI HP- SCT_BSBACCTransferusingNPP_160918902|||EMAL|leetest@npp.com|Scott Roberts Sean and Julia Jackson 
TonyJones2|||||||||||||||||||||||||||||||||||||||||/ 
16,988,400,,C200911065128_00_00_00,NOTPROVIDED,Transfer To Scott Roberts Sean|PayID Email from CommBank App|BDP-T3606_CBA_To_OFI  CCTI_CBA to OFI...||BDP-
T3606_CBA_To_OFI  CCTI_CBA to OFI HP- SCT_BSBACCTransferusingNPP_160934977|||EMAL|leetest@npp.com|Scott Roberts Sean and Julia Jackson 
TonyJones2|||||||||||||||||||||||||||||||||||||||||/ 
16,988,400,,C200911065210_00_00_00,NOTPROVIDED,Transfer To Scott Roberts Sean|PayID Email from CommBank App|BDP-T3606_CBA_To_OFI  CCTI_CBA to OFI...||BDP-
T3606_CBA_To_OFI  CCTI_CBA to OFI HP- SCT_BSBACCTransferusingNPP_160917246|||EMAL|leetest@npp.com|Scott Roberts Sean and Julia Jackson 
TonyJones2|||||||||||||||||||||||||||||||||||||||||/ 
16,988,400,,C200911065446_00_00_00,NOTPROVIDED,Transfer To Scott Roberts Sean|PayID Email from CommBank App|BDP-T3606_CBA_To_OFI  CCTI_CBA to OFI...||BDP-
T3606_CBA_To_OFI  CCTI_CBA to OFI HP- SCT_BSBACCTransferusingNPP_160952894|||EMAL|leetest@npp.com|Scott Roberts Sean and Julia Jackson 
TonyJones2|||||||||||||||||||||||||||||||||||||||||/ 
16,988,400,,C200911065507_00_00_00,ϪϪUTF8,Transfer To Scott Roberts Sean|PayID Email from CommBank App|BDP-T3606_CBA_To_OFI  CCTI_CBA to OFI...||BDP-
T3606_CBA_To_OFI  CCTI_CBA to OFI HP- SCT_BSBACCTransferusingNPP_160913588|||EMAL|leetest@npp.com|Scott Roberts Sean and Julia Jackson 
TonyJones2|||||||||||||||||||||||||||||||||||||||||/
16,988,400,,C200911065635_00_00_00,,Transfer To Scott ϪϪUTF8 Roberts Sean|PayID Email from CommBank App|BDP-T3606_CBA_To_OFI  CCTI_CBA to OFI...||BDP-
T3606_CBA_To_OFI  CCTI_CBA to OFI HP- SCT_BSBACCTransferusingNPP_160936591|||EMAL|leetest@npp.com|Scott Roberts Sean and Julia Jackson 
TonyJones2|||||||||||||||||||||||||||||||||||||||||/ 
16,988,400,,C200911065150_00_00_00,NOTPROVIDED,Transfer To Scott Roberts Sean|PayID Email from CommBank App|BDP-T3606_CBA_To_OFI  CCTI_CBA to OFI...||BDP-
T3606_CBA_To_OFI  CCTI_CBA to OFI HP- SCT_BSBACCTransferusingNPP_160956469|||EMAL|leetest@npp.com|Scott Roberts Sean and Julia Jackson 
TonyJones2|||||||||||||||||||||||||||||||||||||||||/ 
49,643851514,20/ 
03,06226800653167,,015,000,,,100,000,0,,400,000,0,,900,000,,,901,000,,,904,,,,905,,,/ 
49,000,2/ 
03,06226811598407,,015,10312157,,,100,000,0,,400,10000,1,,900,000,,,901,000,,,904,,,,905,,,/ 
16,970,10000,,N091100512222001NPA,,MONASH COUNCIL RATES NetBank BPAY 1826|0001340538|||||||||||||||||||||||||||||||||||||||||||||||||/ 
49,10332157,3/ 
98,654183671,3,27/ 
99,654183671,1,29/