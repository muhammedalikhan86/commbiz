| 

# **TABLE OF CONTENTS** 

Introduction 3 Contacts for Assistance 3 Abbreviations Used in this Document 3 File Name Format 3 File Description and Business Rules 3 File Contents – Data Rows Format 4 Examples 12 

Page 2 of 12 

# **INTRODUCTION** 

CommBiz IPFX Bulk Settlement upload is designed to make the outgoing international payment process easy and seamless.  This function is enabled when a file is uploaded using **File Transfer** - **Import** into CommBiz to seamlessly create FX trades and attach settlement instructions from the file. 

This document outlines the CommBiz IPFX Bulk Settlement Upload File specification and associated business rules. 

# **CONTACTS FOR ASSISTANCE** 

For assistance creating and testing the upload file, please contact either your Relationship Manager, Global Markets FX dealer, or CommBiz IPFX Support on 



1300 222 339   or 

fxcentre@cba.com.au 

For details on how to upload and process the file through CommBiz IPFX, please refer to the CommBiz IPFX Bulk Settlements User Guide. 

# **ABBREVIATIONS USED IN THIS DOCUMENT** 

The following abbreviations are used in the file contents format validations 

|**Abbreviation**|**Description**|**Character Set**|
|---|---|---|
|A|Alphabetic Characters|A-Z, a-z|
|N|Numeric Characters|0-9|
|AN|Alphanumeric|A-Z, a-z, 0-9|



# **FILE NAME FORMAT** 

The file name of the import file must follow the below rules: 

1. Maximum length of the file name is 60 characters. 

2. The allowable characters in the file name consist of the following 

   - a. Upper or Lower Case Alpha (A-Z, a-z) 

   - b. Numeric (0-9) 

   - c. Full Stop (.) 

   - d. Hyphen (-) 

   - e. Underscore (_) 

3. The leading character cannot be a hyphen or full stop. 

# **FILE DESCRIPTION AND BUSINESS RULES** 

The CommBiz IPFX Bulk Settlement Upload File is composed of a number of data rows that represent foreign currency transactions.  The file format must conform to the below rules: 

1. The file is in the format comma separated values (CSV).  A comma is used to separate data value fields. 

2. No Header or Footer rows are to be included 

3. The file must contain between 1 and 200 rows of data 

4. The data rows must conform to the format specified in the ‘File Contents Data Rows Format’ section of this document. 

5. Commbiz Markets Bulk Settlement Upload allows up to 15 trades to be created from one file.  Therefore the number of different currency pairs required to settle all transactions in the upload file must be no greater than 15. 

Page 3 of 12 

# **FILE CONTENTS – DATA ROWS FORMAT** 

|**Field**<br>**Position**|**Field**<br>**Name**|**Requirement**|**Format Validation**|**Sample**|**Comments**|
|---|---|---|---|---|---|
|1|Transaction<br>Type|Mandatory|Value must be “FX”|FX|Acceptable<br>combinations = FX, fx,<br>Fx, fX|
|2|Transaction<br>Description|Mandatory|Up to 12AN|Payment 2|Can be used for<br>reconciliation purposes<br>on statements e.g.<br>Invoice reference ID|
|3|I BUY<br>Currency|Mandatory|Must be exactly 3A (See<br>CommBizIPFXfor<br>available currency<br>codes)<br>Must be in capital letters|USD||
|4|I BUY<br>Amount|Conditional –<br>Mandatory if<br>field 6 is not<br>populated|Decimal point is optional<br>1N to 11N before<br>decimal point<br>1N or 2N after the<br>decimal point<br>Must be > 0<br>Convert all Currency<br>Amounts to Currency<br>specific formats -<br>perform rounding<br>functions|999.99|BUY Amount Value|



Page 4 of 12 

|**Field**<br>**Position**|**Field**<br>**Name**|**Requirement**|**Format Validation**|**Sample**|**Comments**|
|---|---|---|---|---|---|
|5|I SELL<br>Currency|Mandatory|Must be exactly 3A (See<br>CommBizIPFXfor<br>available currency<br>codes)<br>Must be in capital letters|AUD||
|6|I SELL<br>Amount|Conditional –<br>Mandatory if<br>field 4 is not<br>populated|Decimal point is optional<br>1N to 11N before<br>decimal point<br>1N or 2N after the<br>decimal point<br>Must be > 0<br>Convert all Currency<br>Amounts to Currency<br>specific formats -<br>perform rounding<br>functions|343|Sell Amount Value|



Page 5 of 12 

|**Field**<br>**Position**|**Field**<br>**Name**|**Requirement**|**Format Validation**|**Sample**|**Comments**|
|---|---|---|---|---|---|
|7|I SELL<br>Instruction|Mandatory|Up to 34AN (Space - and<br>‘ are not permitted)<br>Account number must be<br>validated against DDR<br>accounts to detect<br>settlement type<br>OR 3A<br>NOS<br>BPA<br>EFT<br>IMT<br>MAN<br>DOC<br>MON|06200012345678|Debit account number<br>in full, including 6-digit<br>BSB code for<br>DDA accounts<br>3 Alphabetic codes<br>correspond to<br>Settlement Types:<br>1. NOS - Direct Payment<br>to CBA<br>2. BPA - BPAY<br>3. EFT - EFT<br>4. IMT - Inward IMT<br>5. MAN - Manual<br>6. DOC - Documentary<br>Trade<br>7. MON - Money Market|
|8|Intermediar<br>y Bank –<br>Bank Code|Optional|Up to 11AN|11223|Optional|
|9|Intermediar<br>y Institution<br>– Country|Optional - If<br>field 8 is NULL|Exactly 2A (See<br>Appendix for country<br>codes)<br>Must be in capital letters|US|Optional|



Page 6 of 12 

|**Field**<br>**Position**|**Field**<br>**Name**|**Requirement**|**Format Validation**|**Sample**|**Comments**|
|---|---|---|---|---|---|
|10|Beneficiary<br>Bank –<br>Bank Code|Conditional<br>Mandatory –<br>Dependent on<br>settlement<br>instruction.<br>Required for<br>Address Book<br>Entry<br>settlement<br>types|Up to 11AN|11223|Optional for Address<br>Book category -<br>Domestic|
|11|Beneficiary<br>Bank<br>– Country|Conditional<br>Mandatory -<br>Required if<br>settlement<br>instruction is<br>Address Book<br>(SWI or RTG)|Exactly 2A  (Country<br>codes)<br>Must be in capital letters|US||
|12|I BUY<br>Instruction|Mandatory|Up to 34AN  (Space -<br>and ‘ are not permitted)<br>OR<br>3A with values:<br>MAN<br>DOC<br>MON|98765432100|Full Account Number<br>including 6 digits of BSB<br>code for domestic<br>accounts<br>OR<br>Alphabetic code<br>corresponding to<br>CommBizIPFX<br>Settlement Types:<br><br>MAN - Manual|



Page 7 of 12 

|**Field**<br>**Position**|**Field**<br>**Name**|**Requirement**|**Format Validation**|**Sample**|**Comments**|
|---|---|---|---|---|---|
||||||<br>DOC -<br>Documentary Trade<br><br>MON - Money<br>Market|
|13|Beneficiary<br>– Account<br>Name|Conditional -<br>Mandatory if<br>field 12 is not<br>3A|Up to62AN|ABC Limited|Mandatory for Facility<br>accounts (CBA accounts<br>& DDR) and Address<br>Book beneficiaries|
|14|Beneficiary<br>–<br>Address line 1|Conditional -<br>Mandatory if<br>field 12 is not<br>3A|Up to 40AN|1 Fifth Av|Mandatory for Facility<br>accounts (CBA accounts<br>& DDR) and Address<br>Book beneficiaries|
|15|Beneficiary<br>–<br>Address line 2|Mandatory|Must be blank - the<br>payment will be rejected<br>if a value is specified||Not in use - leave blank|
|16|Beneficiary<br>–<br>Address line 3|Mandatory|Must be blank - the<br>payment will be rejected<br>if a value is specified||Not in use - leave blank|
|17|Beneficiary<br>–<br>City/Suburb|Conditional<br>Mandatory - if<br>field 12 is a<br>new<br>Beneficiary|Up to 19AN|New York City|Mandatory if field 12 is a<br>Beneficiary that does<br>not exist in the Address<br>Book|



Page 8 of 12 

|**Field**<br>**Position**|**Field**<br>**Name**|**Requirement**|**Format Validation**|**Sample**|**Comments**|
|---|---|---|---|---|---|
|18|Beneficiary<br>– State|Optional|Up to 4AN|NY (New York)|Optional|
|19|Beneficiary<br>– Postcode|Optional|Up to 8AN|90000|Optional|
|20|Beneficiary<br>– Country|Conditional -<br>Mandatory if<br>field 12 is a<br>Beneficiary|Exactly 2A  (Country<br>Codes)<br>Must be in capital letters|US|Mandatory if field 12 is a<br>Beneficiary (Address<br>Book entry)|
|21|I BUY<br>Payment<br>details|Conditional -<br>Mandatory if<br>field 12 equals<br>MAN<br>Buy Currency<br>(field 3) is<br>IDR/ RON/<br>CNH|Up to 105AN|Payment for invoices<br>001|Only Mandatory for<br>Settlement Type Manual<br>and/or if<br>Buy Currency (field 3) is<br>IDR/ RON/ CNH|
|22|I SELL<br>Payment<br>details|Conditional -<br>Mandatory if<br>field 7 equals<br>MAN|Up to 105AN|Debit for invoices 001|Settlement explanation /<br>bank directions where<br>Settlement Type is<br>Manual|



Page 9 of 12 

|**Field**<br>**Position**|**Field**<br>**Name**|**Requirement**|**Format Validation**|**Sample**|**Comments**|
|---|---|---|---|---|---|
|23|Purpose of<br>Payment|Conditional<br>Mandatory if<br>Buy Currency<br>(field 3) is<br>IDR or<br>CNH (with<br>Beneficiary<br>Bank - Bank<br>code - field 10<br>is in China)|If Currency is IDR<br><br>Up to 90AN<br>IF Currency is CNH (with<br>bank in China) can<br>ONLY have any of the<br>below values.<br><br>'Trade for Goods'<br><br>'For Services'<br><br>'Capital Transfers'<br><br>'Charity Donation'<br><br>'Other'<br>Case insensitive|IDR<br>Transactions<br>CNH<br>For Services|Spaces will be stripped.<br>Case insensitive|
|24|CNAPS<br>Code|Conditional<br>Mandatory if<br>Buy Currency<br>(field 3) is<br>CNH (with<br>Beneficiary<br>Bank - Bank<br>code - field 10<br>is in China)|Up to 34AN (Space - and<br>‘ are not permitted)|123NAsad22||



Page 10 of 12 

|**Field**<br>**Position**|**Field**<br>**Name**|**Requirement**|**Format Validation**|**Sample**|**Comments**|
|---|---|---|---|---|---|
|25|Beneficiary<br>Company<br>Name|Conditional<br>Mandatory if<br>Buy Currency<br>(field 3) is<br>KRW and<br>settlement<br>instruction is<br>Address Book<br>(SWI or RTG)|Up to 40AN|ANC Corp. Ltd<br>David Potter|Enter beneficiary<br>person/company name|
|26|Beneficiary<br>Contact<br>Number|Conditional<br>Mandatory if<br>Buy Currency<br>(field 3) is<br>KRW and<br>settlement<br>instruction is<br>Address Book<br>(SWI or RTG)|Up to 4N “–“ up to 5N “–“<br>up to 8N|011-232-232123||
|27|Social<br>Security<br>Number<br>(SSN)|Conditional<br>Mandatory if<br>Buy Currency<br>(field 3) is<br>KRW and<br>beneficiary is<br>individual|Up to 30AN (Space - and<br>‘ are not permitted)|ANS12312|Optional. However, this<br>field needs to be<br>populated if the Buy<br>Currency (field 3) is<br>KRW and beneficiary is<br>individual for successful<br>payment processing|



Page 11 of 12 

# **EXAMPLES** 

## **Sample 1: I Buy is an address book Beneficiary** 

FX,NewRecord,EUR,,AUD,8,DOC,,,BLOMFRPPXXX,FR,FR7630066100410001057380116,Sir Eiffel Tower,101 French district,,,Paris,,,FR,Sample Buy details,Sample Sell details,,,,, 

## **Sample 2: I Buy and I Sell are non-CBA payment types** 

FX,New Settlement,USD,,AUD,500,MAN,,,,,DOC,,,,,,,,,Sample Buy,Sample Sell 

## **Sample 3: I Buy CNH is an address book Beneficiary** 

FX,Fx Trade 1,CNH,,AUD,200.00,06200011332218,,,CITICNSXXXX,CN,993396885019,CNHaccnt5,ABC,L2,11 Pit St,Beijing,BJ,25262,CN,Test.',Sample Sell Payment details,Trade for Goods,12345,,, 

## **Sample 4: I Buy IDR is an address book Beneficiary** 

FX,Fx Trade 1,IDR,,AUD,200.00,06200011332218,,,AMSNIDJ1XXX,ID,993396885032,IDRaccnt5,ABC,L2,11 Pit St,Jakarta,JK,25262,ID,Test..,Sample Sell Payment details,Test1 

## **Sample 5: I Buy KRW is an address book Beneficiary** 

FX,Fx Trade 1,KRW,,AUD,200.00,06200011332218,,,BBVAKRSEXXX,KR,993396885011,KRWaccnt5,ABC,L2,11 Pit St,Seoul,KR,25262,KR,Test,Sample Sell Payment details,,,Test,1234-654-234,4356 

Page 12 of 12 

