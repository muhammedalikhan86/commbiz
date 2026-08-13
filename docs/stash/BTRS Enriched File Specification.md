Bank f Commonwealth 

# BTRS Enriched File Specification 



<!-- Start of picture text -->
Balance Transaction Transaction Reporting Standard<br><!-- End of picture text -->

Balance Transaction Transaction Reporting Standard (BAI3) 



<!-- Start of picture text -->
(BAI3)<br><!-- End of picture text -->





<!-- Start of picture text -->
Version<br><!-- End of picture text -->





Version 2.0 (8.06.2022) 















1 Commonwealth Bank of Australia  |  BTRS Enriched File Specification | August 2022 

## Contents 

|CONTENHS....se|eeecesesesesesesesesesesecccesescscaeseaeacacaeacansessesesasasaacacaeacacacaeaeatsascesasasaeacacacacacacaeaeassasaseseaeacacacacacaeaeaeaesesesessseaeaeaeaeacaeacatasieaseseeeeeeaeates2|
|---|---|
|<a<br>11g|l0 [0Co<br>6 (0)eeeee<br>eet|
|2.<br>Imp|ortant INFOrMAtION .......ceeesesecesscsseseseseesseescseseseeacsesesecaesesesacsescsssesesacscsesecassesesacassessaesesacsssesecacseseacassesessseeatasseeesasaeeneesanenseeataeaeen|
|x FAY =1<sup>6</sup>|<sup>°1(0)</sup>01 O10)0105 0)ER<br>eres|
|A.<br>File|Structure woesescssesesesesesesesessseeseseesesesseaeaeacacassesesesesesesesesesssaaeaeaeaesesussssssssesesssssesasaeaeaeacsesusssssseeseeeeeeeeessaeatseeseseeseesaeataeates|
|4.1|Field Notations<sup>......sscesesssesesesesesesesesssesesesesesesesescseacacaeacsessesesesesesssesssesaeaeaeaeseaeesssssssesesesesssesseeeaeaeaesesesesesnenseseeeeteeeeetatataeeeseeees</sup>|
|4.2|File Naming Convention<sup>......ccecsseseseeeesessscesesesesescacacacacaeaeseseseseseacacacacacaeacaeaeaesesesesaseaeaeacaeaeacaeaeaeaeaeseaessseseaeatatatasatstaeeeeeeees</sup>|
|4.3|SUPPOrted ACCOUNTS .....cccccssssesesessscesesesesceseesescsesecacsesesescaesesesaeeeacassesesacsesesecanseseeasacsesssesecasseseseacansesecataeseeseeeacaseeecataseeeesanneeeesO|
|4.4|A Note aboutAMOuNt Fields...eseseessesecesesesesesseseseseseseseseseseseeenenenensaesesessseseseseneseseseseneeeeasisseseseseneseeeteteneteteneesseeeeeeeenenee|
|4.5|Record Type 01 File Header ReCOrd ........scscsscsssssssseessssssessscacsceeceeseseseeacsesesecaeecasaeseeacassesecaeseseeeeasaeeeaseecasaseeesasasseeesanenseessesO|
|4.6|Record Type 02 Group Header RECOmd ........csssccscsssssceseesscesesescseesecacseeeecaeseseeacaeecaeseeeacaceeesaeaeseeesasesteeseesatneesesatenseeetanenee|
|4.7|Record Type 03 Account Header ROCOmd........csscssscsccsessssesesssessesseeseseseeeceesesceecasseseasseeacacsesecaeaeseeecasaeseeaeeeatasseeesatseeeeesaneees|
|4.8|Record Type 16 Transaction Detail RECOI .......:.ccseccccsssssseeesseesssescacseeeeeeseseeeeasseseeecsescasaeeecaeaeeeeeasaseeeeeasensseseesateseneeeens DO|
|4.8.|1<br>Text field specification (Record Type 16, Field 7) ....c.ssssssssssssssessessssssssesessssssssssssssessssesseseessestsseesessssassseseesesssseeseess LF|
|4.9|Record Type 49 Account Trailer RECOIr.L.....:.cccccscssscscessssssssesescseesseeeeseseeeeeeseseeseasseasaeeeeaeaseeeeseeeeeessaseeseesseeatenseeeteneeeeeeeeee LL|
|4.10|Record Type 98 Group Trailer RECOrL........:cceccscssssescesssssesseeseseseeecaeseececasscseeasaeacassececaesesecasaeeeeeeaseseeseeeatasseeesatenseeetneees LL|
|4.11|Record Type 99 File Trailer RECO .........ccscssseccssssssesceseeeeeeeeaeseseeecaeseeeeasaesesseeeecaeaeeeeesaeeeeesasisseeeeanieasineeeatisestettisteeetereees LO|
|5.<br>App|endix 1: Common BTRS Codes........scsssccsesssseseesssesseseseseseseceeseeeecasseseeacsescassesecaeaesecacaeeeesasaeseeseeeasaseeeesatiseseetsseseeattateneetens LO|
|6.<br>App|endix 2: Changes from the CBA BAI2 File Sp@c.........ccccssssseccssesssesseeecssseceeseeeecaeseseeecasseseaesecacaeseeesarseeeeetasseeeesteteseeeeeees QL|
|6.1|Sample Data File...ccccccscsssssscssssscesesesescsceeceeseseescasseseeacseseassesecacsesesecasseseeasanseassesecacsesecacaeaeeecasaeseeasauacaseeeesatsseeesatssseestnenes<br>DO|
|6.1.1.|HowtouseSampledatafile...ceeesesessscssesessscesseeseseseeececseseeeaesesesaceeacacsesecacsesesecasseseeasacseassesacacsesecasasseeecatassecseecatasseeesesOO|





<!-- Start of picture text -->
How to use Sample data file...<br><!-- End of picture text -->











<!-- Start of picture text -->
OO<br><!-- End of picture text -->



Bank J Commonwealth 



























































































3 Commonwealth Bank of Australia  |  BTRS Enriched File Specification | August 2022 



<!-- Start of picture text -->
4.<br><!-- End of picture text -->



<!-- Start of picture text -->
File Structure<br><!-- End of picture text -->

### 4. File Structure 



File Type: Variable Length Comma Delimited, UTF-8 The file consists of 7 record types. These are: 



<!-- Start of picture text -->
The file consists of 7 record types. These<br><!-- End of picture text -->



<!-- Start of picture text -->
are:<br><!-- End of picture text -->



<!-- Start of picture text -->
Description<br><!-- End of picture text -->



<!-- Start of picture text -->
Freq<br><!-- End of picture text -->



<!-- Start of picture text -->
Comments<br><!-- End of picture text -->

|Record<br>Type|Description|No. of<br>Fields|Freq|Mandatory/<br>Optional|Comments|
|---|---|---|---|---|---|
|O01|File Header<br>Record|9|1|Mandatory —|It must bethe first record ofthe file.<br>It<br>features the file delivery date and time|
|02|Group Header<br>Record|8|Many|Mandatory|It features the processing date|
|03|Account Header<br>Record|31|Many|Mandatory _|This record is intended to classifythe<br>transactional data by account.|
|16|Transaction<br>Detail Record|7|Many|Optional|One record for each transaction item.<br>Supports UTF-8 data|
|49|Account Trailer<br>Record|3|Many|Mandatory|TheAccount Trailer record provides account<br>level control totals.|
|98|Group Trailer<br>Record|4|Many|Mandatory|— The Group Trailer record provides group<br>level control totals.|
|99|File Trailer<br>Record|4|1|Mandatory|The File Trailer record provides file control<br>totals.|





<!-- Start of picture text -->
4.1<br><!-- End of picture text -->



<!-- Start of picture text -->
Notations<br><!-- End of picture text -->

#### 4.1 Field Notations 



<!-- Start of picture text -->
A number of notations are used in describing the field<br><!-- End of picture text -->



<!-- Start of picture text -->
properties for each record type. They are tabulated as follows:<br><!-- End of picture text -->

A number of notations are used in describing the field properties for each record type. They are tabulated as follows: 

|Notation|Description|
|---|---|
|AN|Denotes an alphanumeric datatype under the Type column.|
|Date|Denotes a date field under the Type column. The date is to be presented in<br>CCYYMMDD format.|
|N|Denotes a numeric field under the Type column.|
|M|A mandatory field is represented by avalue of ‘M’ under the Man/Opt column.|
|O|An optional field is represented by avalue of ‘O’ under the Man/Opt column.|
|UTF-8|UTF-8 is avariable-width character encoding used for electronic<br>communication. Examples of UTF-8 encoded dataissmiles<br>_, Latin characters<br>etc.|





<!-- Start of picture text -->
_, Latin characters<br><!-- End of picture text -->



<!-- Start of picture text -->
etc.<br><!-- End of picture text -->



Bank J Commonwealth 



<!-- Start of picture text -->
4.2<br><!-- End of picture text -->



<!-- Start of picture text -->
Convention<br><!-- End of picture text -->

## 4.2 File Naming Convention 

The default filename is: 





<!-- Start of picture text -->
ID>-<<yyyy>><<MM>><<dd>><<HH>><<mm>><<ss>><<fff>>.<br><!-- End of picture text -->

BTRS-<Receiver ID>-<<yyyy>><<MM>><<dd>><<HH>><<mm>><<ss>><<fff>>. 

Receiver ID (see Record Type 01 Field #03 below) 

Examples: 

CommBiz Automated — BTRS-MAILBOX1-20161018054040073 



<!-- Start of picture text -->
CommBiz Ul -<br><!-- End of picture text -->





CommBiz Ul - BTRS-MAILBOX1-20161018054040073.txt 



<!-- Start of picture text -->
4.3<br><!-- End of picture text -->



<!-- Start of picture text -->
Supported Accounts Accounts<br><!-- End of picture text -->

## 4.3 Supported Accounts Accounts 

This file type provides balances and transactions for standard transaction accounts and business loans. Credit Cards and Home Loans are not supported. If you are unsure, please contact the bank. 



<!-- Start of picture text -->
and Home Loans<br><!-- End of picture text -->



<!-- Start of picture text -->
 supported. If you are unsure, please contact<br><!-- End of picture text -->



<!-- Start of picture text -->
the bank.<br><!-- End of picture text -->



<!-- Start of picture text -->
4.4<br><!-- End of picture text -->



<!-- Start of picture text -->
A Note<br><!-- End of picture text -->



<!-- Start of picture text -->
 Amount<br><!-- End of picture text -->



## 4.4 A Note about Amount Fields 

Fields that contain amounts are always represented as an integer in the smallest unit of the designated currency. For a currency that has 2 decimal places, the rightmost 2 digits will represent the ‘cents’. For example, the amount 1234 could be understood as one thousand two hundred and thirty four Japanese Yen, or as Twelve Twelve dollars and thirty four cents, depending on the designated currency. 



<!-- Start of picture text -->
could be understood as one thousand two hundred and thirty four Japanese Yen, or as Twelve Twelve dollars and thirty four<br><!-- End of picture text -->



<!-- Start of picture text -->
a sign, but negative numbers will have a<br><!-- End of picture text -->

If a field is designated as being signed, then positive numbers will not have a sign, but negative numbers will have a minus sign (“-“) immediately to the left of the digits in the number. 



<!-- Start of picture text -->
minus sign<br><!-- End of picture text -->



<!-- Start of picture text -->
J<br><!-- End of picture text -->

Bank J Commonwealth 



<!-- Start of picture text -->
4.5<br><!-- End of picture text -->



<!-- Start of picture text -->
Record Type 01 File Header Record<br><!-- End of picture text -->

## 4.5 Record Type 01 File Header Record 



<!-- Start of picture text -->
Field Details<br><!-- End of picture text -->



<!-- Start of picture text -->
Comments/<br><!-- End of picture text -->



<!-- Start of picture text -->
Purposes<br><!-- End of picture text -->

|Field<br>#|Field<br>Description|F|ield Det|ails|Comments/ Purposes|Sample<br>DataBTRS|
|---|---|---|---|---|---|---|
|||Man/<br>Opt|Type|Min/ Max<br>Length|||
|01|Record Type|M|N|2/2|Marks the beginning ofthe file.<br>It<br>has a constant value of 01.|01|
|02|Sender<br>Identification|M|AN|3/3|Identifies sender of the collection<br>file.<br>It has a constantvalue of<br>CBA.|CBA|
|03|Receiver<br>Identification|O|AN|1/8|An identifier forthe receiver. It<br>can be set to anything thatthe<br>customer requires (up to 8<br>characters).|MAILBOX1|
|04|‘File Creation<br>Date|M|Date|6/6|Contains file creation date in<br>YYMMDD format.|160126|
|05|‘File Creation<br>Time|M|N|4/4|Contains file creation time stamp<br>in HHMM format.<br>Right justified and zero filled.|0530|
|06|File<br>Identification<br>Number|M|N|1/3|Asequence numbercommencing _<br>from 1, and to be incremented by<br>1.<br>Its purpose is to identify one or<br>more collection files produced on<br>a business day.|1, 2... 999|
|O7|Physical<br>Record Length|O|N|6)|Currentlydefined as NULL.||
|08|Physical Block<br>Size|O|N|0|Currently defined as NULL.||
|09|~=CVersion<br>Number|M|N|1/1|The version number is BAl version<br>number 3.|3|





Bank J Commonwealth 



<!-- Start of picture text -->
4.6<br><!-- End of picture text -->



<!-- Start of picture text -->
Record Type 02 Group Header Record<br><!-- End of picture text -->

## 4.6 Record Type 02 Group Header Record 



<!-- Start of picture text -->
Field Details<br><!-- End of picture text -->



<!-- Start of picture text -->
Comments/ Purposes<br><!-- End of picture text -->

|Field<br>#|Field<br>Description||Field Det|ails|Comments/ Purposes|Sample<br>DataBTRS|
|---|---|---|---|---|---|---|
|||Man/<br>Opt|Type|Min/ Max<br>Length|||
|O01|Record Type|M|N|2/2|Denotes a Group Header record.<br>It has a constant value of 02.|02|
|02|Ultimate<br>Receiver<br>Identification|O|N|0/0|Currently defined as NULL.||
|03|Originator|M|AN|3/3|Denotes the originator of<br>transaction data.<br>It has a<br>constant value of CBA.|CBA|
|04|Group Status|M|N|1/1|Set group status value to 1.|1|
|05<br>=|Asof Date<br>(Settlement<br>Date/ Cutoff<br>Date)|M|Date|6/6|Settlement date for BTRS data.<br>Presented inYYMMDD format.|160125|
|06|AsatTime|O|N|0/0|Corresponds to the time stamp<br>component, presented in HHMM<br>format.<br>This field will be right justified and<br>zero filled.<br>Currently defined as NULL.||
|Of|Currency|O|N|3/3|Currently set toAUD|AUD|
|08<br>=|Asof Date<br>Modifier|M|N|1/1|Contains acode to indicate the<br>‘As of Date’ modifier:<br>1 = interim/previous day<br>2 = final/previous day<br>3 = interim/same day<br>4 = final/same day|2|





Bank J Commonwealth 



<!-- Start of picture text -->
4.7<br><!-- End of picture text -->



<!-- Start of picture text -->
Record Type 03 Account Account Header Record<br><!-- End of picture text -->

## 4.7 Record Type 03 Account Account Header Record 



<!-- Start of picture text -->
Field Details<br><!-- End of picture text -->



<!-- Start of picture text -->
Comments/ Purposes<br><!-- End of picture text -->

|Field<br>#|Field<br>Description|F<br>Man/<br>Opt|ield De<br>Type|tails<br>Min/Max<br>Length|Comments/ Purposes|Sample<br>DataBTRS|
|---|---|---|---|---|---|---|
|O01|~=Record<br>Type|M|N|2/2|Denotes an Account Header record.<br>It<br>has a constant value of 03.|03|
|O02|Customer<br>Account<br>Number|M|N|14/14|Aunique reference to identify<br>aCBA<br>bank account number.<br>The field comprisestwo parts; a 6<br>character BSB and an 8 character<br>account number.|06218110<br>068083|
|03|Currency<br>Code|O|AN|0/3|Null for accounts in the same currency<br>as defined in the 02 record or the<br>three character currency code for<br>accounts in other currencies (e.g. USD<br>for US dollar accounts).|USD<br>EUR|
|04|= Closing<br>Balance<br>Type code|M|N|3/3|Type code forthe Closing Balance. It<br>has a constant value of 015.|015|
|05 ~|~ Closing<br>Balance<br>amount|M|N|Numeric|Containsthe Closing Balance forthe<br>specified date in field 05 of the 02<br>Group Header Record.<br>This is an amount field and follows the<br>conventions defined in the “Note about<br>Amount Fields” (above).|12345<br>-1020202|
|06|‘Total Items<br>Count|O|AN|0/0|Currently set as NULL.||
|O07|Total Funds<br>Type|O|AN|0/0|Currently set as NULL.||
|08|‘Total<br>Credits<br>Type Code|M|N|3/3|Type code for total credits in this 03<br>Account Header Record.<br>It has a constant value of 100.|100|
|09 _~—|s-Total<br>Credits<br>Amount|M|N|Numeric|Total value of credit transactions in<br>this 03 Account Header Record.<br>This is an amount field and follows the<br>conventions defined in the “Note about<br>Amount Fields” (above).|12345,<br>20202|
|10|‘Total Credit<br>Items<br>Count|M|N|Numeric|Total number of credit items inthisO3<br>Account Header record.|1, 150,...|
|11<br>~—|s“Total Credit<br>Funds Type|O|AN|0/0|Currently defined as NULL.||
|12 ~|—Total<br>Debits<br>Type Code|M|N|3/3|Type code for total debits in this 03<br>Account Header Record.<br>It has a constant value of 400.|400|





Bank J Commonwealth 



<!-- Start of picture text -->
Field Details<br><!-- End of picture text -->



<!-- Start of picture text -->
Comments/ Purposes<br><!-- End of picture text -->

|Field<br>#|Field<br>Description|Man/<br>Opt|Field De<br>Type|tails<br>Min/Max<br>Length|Comments/ Purposes|Sample<br>DataBTRS|
|---|---|---|---|---|---|---|
|13 ~|—sTotal<br>Debits<br>Amount|M|N|Numeric|Total value of debit transactions in this<br>03 Account Header Record.<br>This is an amount field and follows the<br>conventions defined in the “Note about<br>Amount Fields” (above).|12345,<br>20202|
|14|=Total Debit<br>Items<br>Count|M|N|Numeric|Total number of debit itemsinthisO3<br>Account Header Record.|1, 150,...|
|15|Total Debit<br>Funds Type|O|AN|0/0|Currently defined as NULL||
|16|Accrued<br>Debit<br>Interest<br>Type Code|M|N|3/3|Type code for Accrued Debit Interest<br>in this O03 Account Header Record.<br>It has a constant value of 900.|900|
|17|Accrued<br>Debit<br>Interest<br>Amount|M|N|Numeric|Accrued Debit Interest Amount<br>This is an amount field and follows the<br>conventions defined in the “Note about<br>Amount Fields” (above).|000|
|18|Accrued<br>Debit<br>Interest<br>Item Count|O|N|0/0|Currently defined as NULL||
|19|Accrued<br>Debit<br>Interest<br>Funds Type|O|AN|0/0|Currently defined as NULL||
|20|= Accrued<br>Credit<br>Interest<br>Type Code|M|N|3/3|Type code forAccrued Credit Interest<br>in this O03 Account Header Record.<br>It has a constant value of 901.|901|
|21|Accrued<br>Credit<br>Interest<br>Amount|M|N|Numeric|Accrued Credit Interest Amount<br>This is an amount field and follows the<br>conventions defined in the “Note about<br>Amount Fields” (above).|000|
|22|Accrued<br>Credit<br>Interest<br>Item Count|O|N|0/0|Currently defined as NULL||
|23|Accrued<br>Credit<br>Interest<br>Funds Type|O|AN|0/0|Currentlydefined as NULL||
|24|Credit Limit<br>Type Code|M|N|3/3|Type code for credit limit on<br>credit/corporate card facilities in this<br>03 Account Header Record.<br>It has a constant value of 904.<br><br>|904<br>|





Bank f Commonwealth 



<!-- Start of picture text -->
Field Details<br><!-- End of picture text -->



<!-- Start of picture text -->
Comments/ Purposes<br><!-- End of picture text -->

|Field<br>#|Field<br>Description|F|ield De|tails|Comments/ Purposes|Sample<br>DataBTRS|
|---|---|---|---|---|---|---|
|||Man/<br>Opt|Type|Min/Max<br>Length|||
|25<br>~|~ Credit Limit<br>Amount|M|N|Numeric|Currentlydefined as NULL forAUD<br>and foreign currency bank accounts<br>This is an amount field and follows the<br>conventions defined in the “Note about<br>Amount Fields” (above).||
|26|CreditLimit<br>Count|M|N|0/0|Currently defined as NULL||
|27|Credit Limit<br>Funds Type|O|AN|0/0|Currently defined as NULL||
|28|Interest<br>Rate Type<br>Code|M|N|3/3|Type code for Interest Rate on<br>credit/corporate card facilities in this<br>03 Account Header Record.<br>It has a constant value of 905.|905|
|29 ~—|s Interest<br>Rate<br>Amount|M|N|Numeric|Currentlydefined as NULL forAUD<br>and foreign currency bank accounts||
|30 ~—|s Interest<br>Rate Count|M|N|0/0|Currentlydefined as NULL||
|31|‘Interest<br>Rate Funds<br>Type|O|AN|0/0|Currently defined as NULL||





Bank J Commonwealth 



<!-- Start of picture text -->
4.8<br><!-- End of picture text -->



<!-- Start of picture text -->
Record<br><!-- End of picture text -->



<!-- Start of picture text -->
Type 16 Transaction Transaction Detail Record<br><!-- End of picture text -->

## 4.8 Record Type 16 Transaction Transaction Detail Record 



<!-- Start of picture text -->
Field Details<br><!-- End of picture text -->



<!-- Start of picture text -->
Comments/ Purposes<br><!-- End of picture text -->

|Field|Field|Fie|ld Detai|ls|Comments/ Purposes|Sample|
|---|---|---|---|---|---|---|
|#|Description|Man/ Opt|Type|Min/ Max<br>Length||DataBTRS|
|O01|Record<br>Type|M|AN|2/2|Denotes aTransaction Detail record.<br>It has a constant value of 16.|16|
|02|BTRS Type<br>Code|M|N|3/3|The type code indicates the type of<br>transaction. For alist of the subset of<br>standard BTRS codes used by CBA,<br>and the customized codes, see<br>Appendix 1.<br>NPPSpecific details:<br>NPP transactions willbe<br>identified with fournewnumeric<br>codes:<br>948, 949, 988 & 989<br>See Appendix 1 for further<br>information<br>PayTo Specific details:<br>PayTo transactions willbe<br>identified with fournewnumeric<br>codes 990, 991, 992 & 993<br>See Appendix 1 for further<br>information|165, 475,<br>930|
|03|Amount|M|N|Numeric|The value of the transaction<br>This isan unsigned amount field and<br>follows the conventions defined in<br>the “Note about Amount Fields”<br>(above). Whether this is a debit or<br>credit should be derived from the<br>Type code (field 2)|12345,<br>200100|
|04|Fund type|O|N|0/0|Currently defined as NULL||
|05<br>~|~ Bank<br>Reference<br>Number|O|AN|1/20|Contains unique bank transaction<br>reference ID|D5335031<br>95211234<br>NPA|





<!-- Start of picture text -->
Bank<br>J Commonwealth<br><!-- End of picture text -->





<!-- Start of picture text -->
Field Details<br><!-- End of picture text -->



<!-- Start of picture text -->
Comments/ Purposes<br><!-- End of picture text -->

|Field<br>#|Field<br>Description|Fi<br>Man/ Opt|eld Detai<br>Type|ls<br>Min/ Max<br>Length|Comments/ Purposes|Sample<br>DataBTRS|
|---|---|---|---|---|---|---|
|06|Customer<br>Reference<br>Field|M|UTF-<br>8|0/50|The customer reference field may<br>contain a reference traceable bythe<br>customer. Different type codes will<br>contain different types of reference.<br>For example a cashed cheque will<br>contain the cheque number.<br>The list of type codes in Appendix 1<br>includes details of what customer<br>reference is included for each type<br>code.||
||||||NPPSpecific details: NPPend-<br>to-end ID willbe populated, up to<br>35 characters in length, if<br>available.<br>NPPend-to-end ID isassigned<br>by the sendingbankandremains<br>unchanged throughout the end-<br>to-end chain.<br>Some banksmayassign the<br>Direct EntryLodgement<br>Reference as the NPP end-to-<br>end ID, forNPPpayments<br>processed to a BSBandaccount<br>number.<br>PayTo Specific details: Forcredit<br>payments,||
|O7|=Text|M|UTF-<br>8|0/2000_|This field contains acomplete<br>narrative for the transaction. Where<br>there is morethan1<br>line of narrative,<br>lines are separated bya pipe<br>character (“|”). This narrative will<br>include both information from CBA<br>and also potentially details supplied<br>by the person creating the<br>transaction.<br>Size is increased to support any<br>additional data in future.<br>This BTRS report has specific<br>narratives defined for every type of<br>transaction. See section 4.8.1 for<br>more information.||





Bank J Commonwealth 

Field Field # Description 

Field Details 

Comments/ Purposes 

Sample Data BTRS BTRS 

Man/ Opt Type Min/ Max Length 



<!-- Start of picture text -->
Description<br><!-- End of picture text -->



<!-- Start of picture text -->
Field Details<br><!-- End of picture text -->



<!-- Start of picture text -->
Man/ Opt<br><!-- End of picture text -->



<!-- Start of picture text -->
Type<br><!-- End of picture text -->



<!-- Start of picture text -->
Length<br><!-- End of picture text -->



<!-- Start of picture text -->
Comments/ Purposes<br><!-- End of picture text -->



<!-- Start of picture text -->
Data BTRS BTRS<br><!-- End of picture text -->





Bank J Commonwealth 

































































































































































































































14 Commonwealth Bank of Australia  |  BTRS Enriched File Specification | August 2022 



<!-- Start of picture text -->
Field Name<br><!-- End of picture text -->



<!-- Start of picture text -->
Field Technical Details<br><!-- End of picture text -->



<!-- Start of picture text -->
Comments/Purposes<br><!-- End of picture text -->

|Field<br>#|Field Name|Field|Technica|l Details|Comments/Purposes|Sample Data<br>BTRS|
|---|---|---|---|---|---|---|
|||Man/<br>Opt|Type|Min/ Max<br>Length|||
||||||Debtor account — When NPP<br>transaction is initiated from Debtor<br>account.<br>Card token<br>Masked card<br>PaylD Identifier||
|7|Debtor Name|O|Text|140|Payer Name forNPPcredit<br>transactions (includingreturns).|AB<br>Joshi<br>S Parker|
|8|PaylD Type|O|Text|30|Displayed forNPPdebit<br>transaction. Includes but not<br>limited to the below (As perNPP =<br>standard):<br>EMAL or TELI orAUBN etc.|EMAL<br>TELI<br>AUBN|
|9|PaylD<br>Identifier|O|Text|256|Displayed forVPPdebit<br>transaction<br>PaylD Identifier such as phone<br>number, email address ,ABN etc.|~~abcd@email.com~~<br>0401111333|
|10|PayID Name|O|Text|140|PayID name displayed forWPP<br>debit transaction|TonySo<br>S Jones|
|11|ISO Reason<br>Code|O|Text|4|Displayed for returned and<br>rejected transactions.<br>Includes but not limited to the<br>below (As per NPP standard):<br>ACO0O3 — No Account<br>ACO07 — Account Closed<br>BEO6 — Refer to Customer||
|12|Number of<br>cheques|O|Text|16|Reserved field for future changes<br>to display number cheques for<br>cheque deposit transactions.|Num Chqs 1|
|16|Creditor<br>reference|O|Text|35|PayTo Transactions: Creditor<br>reference will be populated if it is<br>present.||
|17|Payment<br>Service|O|Text|35|PayTo and NPPTransactions:<br>PaymentService will be<br>populated.||





<!-- Start of picture text -->
Bank<br>J Commonwealth<br><!-- End of picture text -->





<!-- Start of picture text -->
Field Name<br><!-- End of picture text -->



<!-- Start of picture text -->
Field Technical Details<br><!-- End of picture text -->



<!-- Start of picture text -->
Comments/Purposes<br><!-- End of picture text -->

|Field<br>#|Field Name|Field|Technica|l Details|Comments/Purposes|Sample Data<br>BTRS|
|---|---|---|---|---|---|---|
|||Man/<br>Opt|Type|Min/ Max<br>Length|||
|18-50|Reserved<br>positionsfor<br>future<br>expansion|O|Text|TBD?|—Currently defined asNULL<br>Concatenated pipes<br>| to indicate<br>37 placeholdervariable fields<br>reserved for future information|AUARAVUUARRAOOOATRVOOOAL<br>UVTTTNTHTTI|
|51|Last field<br>Indicator|M|Text|1|Indicate end of the transaction<br>description|/|





<!-- Start of picture text -->
are already reserved for the fields:<br><!-- End of picture text -->

Note: <mark>13-15</mark> are already reserved for <mark>the</mark> fields: 



<!-- Start of picture text -->
+ Data size in the reserved position will be determined when the field is defined in future.<br><!-- End of picture text -->



+ Data size in the reserved position will be determined when the field is defined in future. Bank J Commonwealth 



<!-- Start of picture text -->
4.9<br><!-- End of picture text -->



<!-- Start of picture text -->
Record Type 49 Account 49 Account Account Trailer Record<br><!-- End of picture text -->

## 4.9 Record Type 49 Account 49 Account Account Trailer Record 



<!-- Start of picture text -->
Field Details<br><!-- End of picture text -->



<!-- Start of picture text -->
Comments/ Purposes<br><!-- End of picture text -->

|Fiel<br>#|d<br>Field<br>Description||Field De|tails|Comments/ Purposes|Sample Data<br>BTRS|
|---|---|---|---|---|---|---|
|||Man/<br>Opt|T<br>ype|Min/Max<br>Length|||
|01|~—Record Type|M|N|2/2|Denotes an Account Trailer record.<br>It<br>has a constant value of 49.|49|
|02|Account<br>Control Total|M|N|Numeric|Contains the total amount of all the<br>amount fields in the preceding O03 and<br>16 record types<br>This is asigned amount field and<br>follows the conventions defined in the<br>“Note aboutAmount Fields” (above).|7200, -<br>430050|
|03<br>4.10|Number of<br>Records<br>Record Ty|M<br>pe98|N<br>Group|Numeric<br>  Trailer|Contains the count of records for this<br>account including the 03, 16 and 49<br>record.<br>Record|106|
|Field<br>#|Field<br>Description||Field Deta|ils|Comments/ Purposes|Sample Data<br>BTRS|
|||Man/<br>Opt|Type|Min/ Max<br>Length|||
|01|~—Record Type|M|N|2/2|Denotes a Group Trailer record.<br>It has a constant value of 98.|98|
|02|Group Control<br>Total|M|N|Numeric|Containsthe total amount of all the<br>amount fields in the preceding 49<br>record types<br>This is asigned amount field and<br>follows the conventions defined in the<br>“Note aboutAmount Fields” (above)|99887766, -<br>999878|
|03|Number of<br>Accounts|M|N|Numeric|The number of03 records in thisgroup|3|
|04|Number of<br>Records|M|N|Numeric|Contains the total number of records<br>for this group including the 02, 03, 16,<br>49 and 98 records.|18|





Bank J Commonwealth 



<!-- Start of picture text -->
4.11<br><!-- End of picture text -->



<!-- Start of picture text -->
Record Type 99 99<br><!-- End of picture text -->



<!-- Start of picture text -->
File Trailer Record<br><!-- End of picture text -->

## 4.11 Record Type 99 99 File Trailer Record 



<!-- Start of picture text -->
Field #<br><!-- End of picture text -->



<!-- Start of picture text -->
Field Details<br><!-- End of picture text -->



<!-- Start of picture text -->
Comments/ Purposes<br><!-- End of picture text -->

|Field #|Field<br>Description|F<br>Man/<br>Opt|ield Det<br>Type|ails<br>Min/ Max<br>Length|Comments/ Purposes|Sample<br>DataBTRS|
|---|---|---|---|---|---|---|
|01|Record Type|M|N|2/2|Denotesa File Trailer record.<br>It has a constant value of 99.|99|
|02|File Control<br>Total|M|N|Numeric|Contains the total amount of all<br>the amount fields in the preceding<br>98 record type/s<br>This is asigned amount field and<br>follows the conventions defined in<br>the “Note about Amount Fields”<br>(above).|99887766,<br>-999878|
|03|Number of<br>Groups|M|N|Numeric|Number of 02 records in this file|1|
|04|Number of<br>Records|M|N|Numeric|Contains the total number of<br>records in this file including this<br>99 record.|79|





Bank J Commonwealth 

















































































































































































































19 Commonwealth Bank of Australia  |  BTRS Enriched File Specification | August 2022 



<!-- Start of picture text -->
Customer Reference (Field 6)<br><!-- End of picture text -->

|BTRS Code|Description|Dr/Cr|| Customer Reference (Field 6)|
|---|---|---|---|
|357|Adjustment|CR||
|398|Fee - Reversal|CR||
|399|Miscellaneous Credit|CR||
|455|Outbound direct entry|DR||
|475|Presented Cheque|DR|Cheque number|
|477|Bank Prepared Debit|DR||
|481|Loan Payment|DR||
|501|Transfer - Automatic|DR||
|506|Money Transfer<br>- CBA-CBA|DR||
|508|Money Transfer - IMT/RTGS OFI|DR|CBA Reference number|
|512|Trade Finance Settlement|DR||
|514|Travel Money Purchase|DR||
|552|Reversal|DR||
|557|Return of an inbound direct entry|DR|Trace Account|
|568|Returned Cheque|DR||
|575|Sweep transaction<br>(ZBA = zero balance account)|DR||
|595|Cash WithdrawCBA (branch,ATM) and OFIATM<br>=|DR||
|631|Adjustment|DR||
|654|Interest|DR||
|658|Principal Payments|DR||
|696|Collections|DR||
|698|Fees - Charged|DR||
|699|Miscellaneous Debit|DR||
|920|Merchant Settlement|CR||
|925|Card Transaction - Purchase Refunds|CR||
|926|Scheme Debit Chargeback|CR||
|930|BPAY Settlement (Biller)|CR||
|931|BPAY Return (Returned Payment)|CR||
|939|Salary Payment|CR||
|940|eLockbox Settlement|CR|Number of debits and credits|
|941|Disability Pension|CR||
|942|FamilyAllowance|CR||
|943|Unemployment Benefit|CR||
|944|Age Pension|CR||
|945|Carer’s Pension|CR||
|946|Service Pension|CR||





<!-- Start of picture text -->
Bank<br>J Commonwealth<br><!-- End of picture text -->





































































































































- 

- 

- 



























































21 Commonwealth Bank of Australia  |  BTRS Enriched File Specification | August 2022 











- 













22 Commonwealth Bank of Australia  |  BTRS Enriched File Specification | August 2022 





```
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
16,990,400,,K140000007800_00_00_00_000001,45678 creditor reference,Approved Payment Return|IFW AUTO MATION|||NPP-70216|06226811341898|IFW AUTO MATION
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
```

23 Commonwealth Bank of Australia  |  BTRS Enriched File Specification | August 2022 





<!-- Start of picture text -->
How to to use sample data sample data data file<br><!-- End of picture text -->

##### 6.1.1. How to to use sample data sample data data file 

###### 1. Open notepad 



<!-- Start of picture text -->
file section into the notepad.<br><!-- End of picture text -->

2. Copy the text mentioned sample data file section into the notepad. 

3. Save the notepad file with .csv extension 



<!-- Start of picture text -->
Use the .csv file for verification in your application.<br><!-- End of picture text -->

4. Use the .csv file for verification in your application. 



Bank J Commonwealth 

