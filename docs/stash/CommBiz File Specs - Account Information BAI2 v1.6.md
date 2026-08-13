f Commonwealth Bank of Australia 



<!-- Start of picture text -->
Account Information (BAI2)<br><!-- End of picture text -->



<!-- Start of picture text -->
—<br><!-- End of picture text -->

# Account Information (BAI2) — File Specification 



<!-- Start of picture text -->
File Specification<br><!-- End of picture text -->

Version 1.6 

13 January 2023 





















|Version Control|4|
|---|---|
|1.0 BAI2 File Format|5|
|1.1 File Details|5|
|1.2 File Header Record|5|
|1.3 Group Header Record|6|
|1.4 Account Header Record|6|
|1.5 Type Code Table|7|
|1.6 Transaction Detail Record|8|
|1.7 Account Trailer Record|10|
|1.8 Group Trailer Record|10|
|1.9 File Trailer Record|11|
|2. EDTS File Format|12|
|2.1<br>File details|12|
|2.2 Client header record|13|
|2.3 Account header record|13|
|2.4 Account balance record|14|
|2.5 Credit transaction record|14|
|2.6 Debit transaction record|17|
|2.7 Account total record|18|
|2.8 Client trailer record|19|
|3. QIF file format|20|
|3.1 Account header|20|
|3.2 Transaction records|20|
|4.0 QIF file format (Quicken AUS, 2004 and earlier)|21|
|4.1 Account header|21|
|4.2 Transaction records|21|
|5.0Appendix 1: Sample Files|22|
|5.1 BAI2 Data Format|22|
|5.2 EDTS Data Format|23|
|5.3 QIF Data Format|24|
|5.4QIF(QuickenAUS,2004andearlier)DataFormat|24|





<!-- Start of picture text -->
4.0 QIF file format (Quicken AUS, 2004 and earlier)<br><!-- End of picture text -->



<!-- Start of picture text -->
5.4 QIF (Quicken AUS, 2004 and earlier) Data Format<br><!-- End of picture text -->



<!-- Start of picture text -->
24<br><!-- End of picture text -->



#### Version Control 





|1.5|13/02/2018|Inclusion of version control<br>Update to include NPP (New Payments Platform)<br>information for BAI2 Bank Generated, BAI2 On-<br>Demand & EDTS files<br>Update to QIF formats<br>Note: All readers must refer to v1.5 only; v1.4 isto<br>be disregarded|
|---|---|---|
|1.6|13/01/2023|Updateto include Scan Coin transaction description<br>information for BAI2 report.|













































































































<!-- Start of picture text -->
Group Header Record<br><!-- End of picture text -->

#### 1.3 Group Header Record 







|~~Ultimatereceiver~~<br>|~~PY~~|~~Alwaysnull~~|
|---|---|---|
|<br><br>As of date|Numeric|<br>Date for which the data is<br>applicable Format is<br>yymmdd|
|Currency code|Alpha|Always “AUD” for this<br>record|
|As of date modifier<br>.4 Account Header Record||Always “2”|
|Customer account number|Alphanumeric|Branch and account<br>number for DDA accounts<br>orjust account number for<br>foreign currency and credit<br>card accounts|
|Currency code|Alpha|Null forAUD accounts or<br>the three character<br>currency code for foreign<br>currency accounts (e.g.|



##### 1.4 Account Header Record 

|||USD for US dollar<br>accounts)|
|---|---|---|
|Type code|Numeric|Type code from the type<br>table below (refer #1)|
|Items count|Numeric|Null except for type codes<br>100 and 400, set to<br>number of CR and DR<br>transactions respectively|









<!-- Start of picture text -->
Type Code Code Table<br><!-- End of picture text -->

#### 1.5 Type Code Code Table 

|015|Closing<br>balance||Not<br>applicable —<br>set to 000|
|---|---|---|---|
|100|Total<br>CR|901|Not<br>applicable —<br>set to 000|
|400|Total<br>DR|902|Not<br>applicable —<br>set to 000|
|||903|Not<br>applicable —<br>set to 000|
|||904|Credit limit<br>- only for<br>credit card<br>accounts|
|||905|Interest rate<br>- only for<br>credit card<br>accounts|





<!-- Start of picture text -->
Transaction Detail<br><!-- End of picture text -->



<!-- Start of picture text -->
Record<br><!-- End of picture text -->

###### 1.6 Transaction Detail Record 













|Type code|Numeric|“399” for credit transactions or “699”<br>for debit transactions|
|---|---|---|
|~~a~~|~~ee~~|~~ee~~|
|Bank reference<br>number|Alpha or numeric|Three charactertransaction code.<br>.<br>.<br>CommBiz exports using “Request<br>Transaction History” only include<br>numerics. The Bank-generated<br>files are dictated by customer<br>preference.<br>Refer “Transaction Codes for BAI2”.<br>NPPBAI2 Bankgenerated file Update:<br>NPP transactions willbe identified<br>with thenew ‘NPP’code in alpha BA/2<br>files orwith fournewcodes (948,<br>949, 988 & 989) innumericBAI2<br>files. For furtherinformation, refer to<br>“Tran Codes forAccount Information”|
|Customer<br>reference<br>number|Alphanumeric|Credit transactions will contain the<br>AgentnumberforAgent book<br>deposits, or the Agent/Terminal<br>number for EFTPOS settlements.<br>Debit transactions will contain the<br>cheque number for cheque debits.<br>NPPBAI2 Bankgenerated file Update:<br>An NPP end-to-end ID willbe<br>populated, up to 35 characters in<br>length, ifavailable|



||NPPend-to-end ID isassignedbythe<br>sendingbankandremainsunchanged<br>throughout the end-to-end chain.<br>Some banksmayassign the Direct<br>EntryLodgement Reference as the<br>NPPend-to-end ID, forNPP<br>paymentsprocessed to aBSBand<br>accountnumber|
|---|---|
|Transaction<br>Alphanumeric<br>reference field|Containsfree form text description of<br>the transaction.<br>NPPBAI2 Bankgenerated file Update:<br>NPP transactions maycontain<br>narrative such as ‘transfer to/from’<br>depending on the sending bank, along<br>with <aliasname>, <Pay!D type>and<br>anyfree text, truncated to 35<br>characters<br>NPPBAI2 OnDemand Update:<br>NPP transactions maycontain<br>narrative including<br>‘transferto/from’(dependingon<br>sendingbank),<br><aliasname>, <PaylD type>, up to<br>280 character free text. Narratives will<br>be separatedbya pipe ie |<br>Scan Coin BAI2 Bankgenerated file<br>Update:<br>Forcoin machine deposits, full<br>transaction description to beprintedin<br>this field.<br>E.g. COINMACHINEDEPOSIT<br>$5000.00"/Branch 06 2000/Mary's<br>Rent|





<!-- Start of picture text -->
transaction description to be printed in<br><!-- End of picture text -->



<!-- Start of picture text -->
Account Trailer Record<br><!-- End of picture text -->

##### 1.7 Account Trailer Record 













|Account control total|Numeric|Sum of all the amount<br>fields in the preceding<br>type 03, 16 and 88<br>records for this account<br>(refer #2).|
|---|---|---|
|Number of records|Numeric|Total number of records<br>for this account including<br>the type 03, 16 and 88<br>records and thistype 49<br>record.|





<!-- Start of picture text -->
Group Trailer Record<br><!-- End of picture text -->

### 1.8 Group Trailer Record 













|Control total|Numeric|Sum ofthe account<br>control total fields in all<br>the type 49 records for<br>this group (refer #2).|
|---|---|---|
|Number of records|Numeric|Total number of records in<br>this group including the<br>type 02, 03 16 and 49<br>records and this 98 type<br>record.|





<!-- Start of picture text -->
File Trailer Record<br><!-- End of picture text -->

##### 1.9 File Trailer Record 













|Control total|Numeric|Sum ofthegroup control<br>total fields in all type 98<br>records for this file (refer<br>#2).|
|---|---|---|
|Number ofgroups|Numeric|Total number ofgroups in<br>the file (will usually be<br>one).|
|Number of records<br>tes|Numeric|Total number of records in<br>the file|
|#1|The TYPE CODE, AMOUNT<br>are all repeated for each Ty|, ITEMS COUNT and FUNDS TYPE fields<br>pe Code in the Type Code table.|
|#2|If the value of this amount <br>included at the beginning o|field is negative then a negative sign is<br>f the amount value.|
|#3|Record type 88 is a continu<br>a record type exceeded the<br>divide the record. For exam<br>not currently used.|ation record. It would be used ifthe data in<br> physical record size or if it was desirable to<br>ple, splitting atext field. This record type is|



Notes 





































































<!-- Start of picture text -->
2.2<br><!-- End of picture text -->



<!-- Start of picture text -->
Client header record<br><!-- End of picture text -->

##### 2.2 Client header record 





|Record<br>type|1|1|Numeric|Always “1”|
|---|---|---|---|---|
|Filler|2|4|Numeric|Always<br>“0000”|
|Filler|||Numeric|Always<br>“00000000”|
|Filler|14|43|Alpha|Always<br>blanks|
|Client<br>number|57|4|Numeric||





<!-- Start of picture text -->
2.3<br><!-- End of picture text -->



<!-- Start of picture text -->
Account header record<br><!-- End of picture text -->

#### 2.3 Account header record 









|Record<br>type|1|1|Numeric|Always “2”|
|---|---|---|---|---|
|Branch<br>number|2|4|Numeric||
|Account<br>number|||Numeric|Right justified,<br>blank filled|
|Export<br>date|14||Numeric|Format is<br>ddmmyy|
|Client<br>number|57|4|Numeric||





<!-- Start of picture text -->
2.4<br><!-- End of picture text -->



<!-- Start of picture text -->
Account balance record<br><!-- End of picture text -->

#### 2.4 Account balance record 





|Record<br>type|1|1|Numeric|Always “3”|
|---|---|---|---|---|
|Branch<br>number|2|4|Numeric||
|Account<br>number|||Numeric|Right justified,<br>blank filled|
|Account<br>balance|14|13|Numeric|Format is<br>$$$$$$$$$$$cec|
|Balance<br>sign|27|1|Blank or<br>(-)|Blank indicates a<br>credit balance (-)<br>indicates a debit<br>balance|
|Filler<br>(not in<br>use)|28|11|Numeric|“00000000000”|
|Filler<br>(not in<br>use)|39|11|Numeric|“00000000000”|
|Client<br>number|57|4|Numeric||





<!-- Start of picture text -->
2.5<br><!-- End of picture text -->



<!-- Start of picture text -->
Credit transaction record<br><!-- End of picture text -->

##### 2.5 Credit transaction record 



<!-- Start of picture text -->
Record 1 1 Numeric Always “4”<br>type<br><!-- End of picture text -->

|Branch<br>number|2|4|Numeric||
|---|---|---|---|---|
|Account<br>number|||Numeric|Right justified,<br>blank filled|
|Transaction<br>reference|14||Alpha/numeric}<br>Blanks|Agent number,<br>depositing<br>branch, POS<br>terminal ID<br>(rightjustified,<br>blank filled) or<br>blanks|
|Transaction<br>type code|22|3|Alpha/<br>numeric|NPPBank<br>generated file<br>Update:<br>NPP<br>transactions<br>willbe<br>identified with<br>the new ‘NPP’<br>code in alpha<br>EDTS files or<br>with two new<br>creditcodes<br>(948, 949) in<br>numeric EDTS<br>files.<br>For further<br>information,<br>refer to “Tran<br>Codes for<br>Account<br>Information”|



|Transaction<br>amount|25|11|Numeric|Format is<br>$$$$$$$$$ec|
|---|---|---|---|---|
|Number of<br>cheques<br>|36||Numeric<br>|Number of<br>cheques in<br>deposit (right<br>justified, zero<br>filled)<br>|
|Lodgement<br>~~ae]~~|45|16<br>|Alpha/<br>|Left justified and<br>~~e~~|





<!-- Start of picture text -->
2.6<br><!-- End of picture text -->



<!-- Start of picture text -->
Debit transaction record<br><!-- End of picture text -->

##### 2.6 Debit transaction record 

|Field name|Relativ<br>e<br>address|Lengt<br>h|Content<br>s|Comments|
|---|---|---|---|---|
|Record<br>type|1|1|Numeric|Always “5”|
|Branch<br>number|2|4|Numeric||
|Account<br>number|||Numeric|Rightjustified,<br>blank filled|
|Transaction<br>reference|14||Numeric|Initiating<br>branch (right<br>justified, blank<br>filled)|
|Transaction<br>type code|22|3|Alpha/<br>numeric|NPPBank<br>generated file<br>Update:<br>NPP<br>transactions will<br>be identified<br>with the new<br>‘NPP’code in<br>alphaEDTS<br>files or with two<br>new debitcodes<br>(988, 989) in<br>numeric EDTS<br>files.<br>For further<br>information,<br>refer to “Tran<br>Codes for<br>Account<br>Information”|
|Transaction<br>amount|25|11|Numeric|Format is<br>$$$$$$$$$cec|
|Transaction<br>number|36||Numeric|Cheque serial<br>number or<br>branch no (right<br>justified, zero<br>filled)|



Lodgement 45 16 reference 



<!-- Start of picture text -->
reference<br><!-- End of picture text -->



<!-- Start of picture text -->
45<br><!-- End of picture text -->



<!-- Start of picture text -->
16<br><!-- End of picture text -->

Alpha/ Left justified, numeric blank filled 



<!-- Start of picture text -->
2./<br><!-- End of picture text -->



<!-- Start of picture text -->
Account total record<br><!-- End of picture text -->

#### 2./ Account total record 

|Field<br>name|Relativ<br>e<br>address|Lengt<br>h|Content<br>s|Comments|
|---|---|---|---|---|
|Record<br>type|1|1|Numeric|Always “6”|
|Branch<br>number|2|4|Numeric||
|Account<br>number|||Numeric|Rightjustified,<br>blank filled|
|Total<br>amount<br>of<br>credits|14|13|Numeric|Format is<br>$$$$$$$$$$$cec|
|Total<br>amount<br>of<br>debits|27|13|Numeric|Format is<br>$$$$$$$$$$$cec|
|Total<br>number<br>of<br>credits|40|5|Numeric||
|Total<br>number<br>of<br>debits|45|5|Numeric||
|Client<br>number|57|4|Numeric||



































































- 

- 





















































- 

- 































<!-- Start of picture text -->
5.0<br><!-- End of picture text -->



<!-- Start of picture text -->
Appendix 1: Sample Files<br><!-- End of picture text -->

## 5.0 Appendix 1: Sample Files 



<!-- Start of picture text -->
5,1<br><!-- End of picture text -->



<!-- Start of picture text -->
BAI2 Data Format<br><!-- End of picture text -->



#### 5,1 BAI2 Data Format 



###### **Sample EDTS data file** 

|1000000000000|0111|
|---|---|
|2292912345678120118|0111|
|32929123456780000233683862-0000000000000000000000<br>4292912345678 14007AGN00000007185000000000|0111|
|4292912345678 14007AGN00000007335000000000 4292912345678<br>14008AGN00000009865000000000||
|4292912345678 14009AGN00000019205000000000||
|4292912345678 14007AGN00000022095000000001<br>4292912345678 14034AGN00000150081000000003||
|5292912345678<br>CHQ00000015138000137142||
|5292912345678<br>CHQ00000050533000137143||
|6292912345678000000021576600000000656710000600002|0111|
|8999999999999|0111|







!Account N2929-12345678 DACCOUNT 01 TBank ^ !Type:Bank D7/3/2018 T-10,138.56 M268082PTM0205 Star City L ^ D8/3/2018 T4,500.00 M2020;1 Cheques deposited L ^ D8/3/2018 T7,840.88 M2020;1 Cheques deposited L ^ D8/3/2018 T30,000.00 MCDA 133233 FIN MARKETS L ^ 





!Type:Bank D17/03/18 T-24,238,768.14 PMISCELLANEOU ^ D17/03/18 T24,238,365.84 PMISCELLANEOU ^ 

