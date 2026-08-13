5 



<!-- Start of picture text -->
5<br><!-- End of picture text -->







Conmonwealthiak()y 



Comnonwealth Bark , 



Comnonwealth Bark , 

# **DELIST - CSV format (Legacy)** 

The Direct Entry Transactions Report can be downloaded in a comma delimited format. 

This file format can be used to import account information into third party Account Software packages. 

# **File details** 

|File type<br>fixed width comma delimited, ASCII text|
|---|
|The file consists of 2 record types.  These are:|
|Transaction detail record|
|Total record|



# **Transaction detail** 

|**Field Description**|**Field**<br>**Size**|**Position**|**Format / Comment /**<br>**Purpose**|
|---|---|---|---|
|BSB Account Number|12|1 – 12|Numeric|
|Comma|1|13|Alphanumeric|
|Date of Processing|10|14 – 23|Date (dd/mm/ccyy)|
|Comma|1|24|Alphanumeric|
|Title of Account|32|25 – 56|Alphanumeric|
|Comma|1|57|Alphanumeric|
|Amount|11|58 – 68|Currency ($$$$$$$$.cc)|
|Comma|1|69|Alphanumeric|
|Credit or Debit|2|70 – 71|Alphanumeric (CR / DR)|
|Comma|1|72|Alphanumeric|
|Lodgement Reference|18|73 – 90|Alphanumeric|
|Comma|1|91|Alphanumeric|
|Trace Account|16|92 – 107|Alphanumeric|
|Comma|1|108|Alphanumeric|
|Transaction Description<br>Refer page 7 for additional detail|16|109 – 124|Alphanumeric|
|Comma|1|125|Alphanumeric|
|Unused|10|126-135|Alphanumeric|



NB: The account number component of the Trace Account may be either left or right justified and can contain zero padding or filled white space or a combination of the two. 





Page **5** of **14** 

# **Total Record** 

|**Field Description**|**Field**<br>**Size**|**Position**|**Format / Comment /**<br>**Purpose**|
|---|---|---|---|
|Grand Total|10|1 – 10|Alphanumeric (Fixed<br>description)|
|Comma|1|11|Alphanumeric|
|Date of Processing|10|12 - 21|Date (dd/mm/ccyy)|
|Comma|1|22|Alphanumeric|
|Trans|5|23 - 27|Alphanumeric (Total<br>Number of Transactions -<br>Fixed description)|
|Comma|1|28|Alphanumeric|
|Total Number of Transactions|10|29 - 38|Alphanumeric (Numeric<br>value)|
|Comma|1|39|Alphanumeric|
|CR Amt|7|40 - 46|Alphanumeric (Credit<br>Amount -Fixed<br>description)|
|Comma|1|47|Alphanumeric|
|Credit $ Amount|14|48 - 61|Currency<br>($$$$$$$$$$$.cc)|
|Comma|1|62|Alphanumeric|
|Dr Amt|7|63 - 69|Alphanumeric (Debit<br>Amount - Fixed<br>description)|
|Comma|1|70|Alphanumeric|
|Debit $ Amount|14|71 - 84|Currency<br>($$$$$$$$$$$.cc)|
|Comma|1|85|Alphanumeric|
|Unused|14|86-135|Alphanumeric|







Page **6** of **14** 

# **Transaction Description** 

For returned, rejected or dishonoured transactions; the reason for return, rejection or dishonour is provided in the Transaction Description field. 

|**Return, rejection or**<br>**dishonour code**|**Explanation of the Code on your statement**|
|---|---|
|DE INVALID BSB|An invalid BSB (Bank/State/Branch) number was used<br>for the transaction.|
|DE PAYMENT STOPP|This signifies that your customer does not have the<br>relative authority for Direct Debit drawings.|
|DE A/C CLOSED|The account being credited or debited has been<br>closed.  Please refer back to the account holder for the<br>correct information.|
|DE DEC”D CUST|The customer is deceased.|
|DE NO ACCOUNT|The account number contains an invalid format, or<br>does not exist.  Please refer back to the account holder<br>for the correct information.|
|DE REFER TO CUST|A Debit has been dishonoured.|
|DE INVALID USER|This code advises that an invalid User ID number was<br>forwarded to the Bank.  This number is held in the<br>header record of the original file supplied.  Please refer<br>to your normal Bank contact.|
|DE TECH INVALID|The Bank is unable to identify a character appearing<br>on the transaction.|



Statement narratives provided by the Bank are in accordance with specifications prescribed by the Australian Payment Clearing Association (APCA). 





Page **7** of **14** 

# **Sample File** 

```
200012345678,13/09/2011, Company A Account 1            ,       5.00,DR,5353109491119999  ,062-441 10011010,CBA MERCHANT FEE,
200012345678,13/09/2011, JADE ALEXANDER                 ,       5.00,DR,DD 3010113        ,732-299   606060,DE PAYMENT STOPP,
200012345678,13/09/2011, SONYA HAAK                     ,      50.00,DR,A/C00035703799 01/,572-200   005535,DE NO ACCOUNT   ,
200012345678,13/09/2011, Raymond T Stokes               ,      50.00,DR,DD 3010118        ,014-726189997722,DE A/C CLOSED   ,
200012345678,13/09/2011, LIYANA ABDUL                   ,      50.00,DR,DD 3006554        ,062-999 10610610,DE TECH INVALID ,
200012345678,13/09/2011, Company A                      ,     113.93,DR,HD inv 602045     ,164-000101482739,HEALTH DIRECTON ,
200012345678,13/09/2011, Company A Account 1            ,    2158.70,DR,TRF Comp C 13/09  ,062-000 12345678,COMMBIZ TRANSFER,
200012345678,13/09/2011, Company A                      ,      66.28,CR,000001900190019   ,242-200000633633,DINERS     70.00,
200012345678,13/09/2011, SARGENT SUPER FUND A/C         ,     100.01,CR,SUPER 14109307    ,362-999000595959,DE INVALID BSB  ,
200012345678,13/09/2011, MR B L BRYANT                  ,     100.68,CR,SALARY 00258666   ,014-274369322448,DE A/C CLOSED   ,
200012345678,13/09/2011, HOCKING Pty Ltd                ,     500.81,CR,PMNT INV 9782326  ,111-000 16396507,DE NO ACCOUNT   ,
200012345678,13/09/2011, COMPANY A                      ,    1030.77,CR,MISS SMITH 1240447,633-000106831899,BENDIGO BANK    ,
200012345678,13/09/2011, THE BINKS SUPER FUND           ,    2491.50,CR,A/C00022683010    ,704-877   266429,DE INVALID BSB  ,
200012345678,13/09/2011, Company A Account 1            ,    3138.00,CR,5353109491119999  ,062-067 1190070 ,CBA CREDIT CARDE,
200012345678,13/09/2011, COMPANY A                      ,    3465.00,CR,PMNT COMP A 123456,666-000  2344566,GERFLOR AUST.P  ,
200012345678,13/09/2011, A Fletcher                     ,    3807.91,CR,SALARY 00564235   ,777-999 56748392,DE NO ACCOUNT   ,
200012345678,13/09/2011, COMPANY A                      ,    5629.19,CR,AGC111001370777   ,064-000010482777,AUSTRALIA POST  ,
300010012345,13/09/2011, MRS MELDA JOY KNOWLES          ,      15.00,DR,DD 27060984       ,084-999834303030,DE DEC"D CUST   ,
300010012345,13/09/2011, D Castellini                   ,      20.00,DR,DD 27028438       ,013-999556677880,DE DEC"D CUST   ,
300010012345,13/09/2011, Raymond T Stokes               ,      20.00,DR,DD 27081725       ,802-990   609030,DE INVALID BSB  ,
300010012345,13/09/2011, Company B                NGL*  ,      72.00,DR,Salary Adjust 2345,063-000 10012345,Company B       ,
300010012345,13/09/2011, Company B                NGL*  ,      76.18,DR,Cutoff 13-Sep-2011,063-000 10012352,Company C       ,
300010012345,13/09/2011, Company B                NGL*  ,  398671.10,DR,PM FILE 13/09/11  ,063-000 10012352,Company C       ,
300010012345,13/09/2011, Company B                      ,    2020.00,CR,9792979292        ,032-777   880880,AMEX GR  2020.00,
300010012345,13/09/2011, Company B                      ,   40768.12,CR,LLORD INV BB1289  ,014-572483896666,WAMM  SPJ & MWMD,
300010012345,13/09/2011, Company B                      ,   57759.59,CR,Murchison Metals L,066-130 10247201,MURCHISON METALS,
300010012345,13/09/2011, Company B Account 1            ,   88462.21,CR,5353109491118888  ,062-067 1190070 ,CBA CREDIT CARDE,
300010012345,13/09/2011, Company B Account 1            ,  213656.41,CR,0000000DR0000011CR,062-000 12345678,CBA AUTOPAY     ,
300010012345,13/09/2011, Company B Account 1            ,  226235.20,CR,0000000DR0000036CR,062-000 12345678,CBA AUTOPAY     ,
300010012345,13/09/2011, Company B Account 1            ,  234151.90,CR,0000000DR0000028CR,062-000 12345678,CBA AUTOPAY     ,
300010012345,13/09/2011, Company B Account 1            ,  469394.71,CR,0000000DR0000019CR,062-000 12345678,CBA AUTOPAY     ,
GRANDTOTAL,13/09/2011,TRANS,        31, CR AMT,    1352778.29, DR AMT,     401306.91,
```

# **CSV format (New variant February 2017)** 

The Direct Entry Transactions Report can be downloaded in a variable length comma delimited format. 



Page **8** of **14** Commercial in Confidence 



Conmonwealth Csi: f 

# **Sample File** 

```
299912345678,10/02/2017,HAHLANI TALENT MR,35.00,DR,192184242366,306-999000987562,DE REFER TO CUST,
299912345678,10/02/2017,FRAMPTON DAVID MR,25.00,DR,704585125173,065-987 10398367,DE PAYMENT STOPP,
299912345678,10/02/2017,STEVENS CAROL MRS,100.00,DR,685763501464,062-99926245672,DE TECH INVALID,
299912345678,10/02/2017,MCLENNAN PATRICIA MS,62.00,DR,187481558974,099-408513112362,DE REFER TO CUST,
299912345678,10/02/2017,DEBORAH MASON,41.00,DR,613994722477,767-088000123439,DE PAYMENT STOPP,
299912345678,10/02/2017,NSW,543.87,CR,09-FEB-2016,065-999010788883,NSWenergy PTY LT,
```

```
299912345678,10/02/2017,R O AND RJ HOLLINS,69.00,DR,341663975639,499-799003456211,DE NO ACCOUNT,
299912345678,10/02/2017,CARTER-LEWIS PAIGE MRS,101.00,DR,311297202986,734-245123422100,DE REFER TO CUST,
299912345678,10/02/2017,GANE KAREN MRS,20.00,DR,740286089862,067-899 10212361,DE REFER TO CUST,
299912345678,10/02/2017,MARDINI PATRICK,125.00,DR,748494183392,062-888 10424688,DE REFER TO CUST,
299912345678,10/02/2017,CRAMPTON EDGAR MR,47.13,DR,529052647302,560-999177444494,DE INVALID BSB,
299912345678,10/02/2017,MCGOVERN LESLIE MR,399.51,DR,772260028462,134-879145123453,DE A/C CLOSED,
299912345678,10/02/2017,LAWRENCE PRINCE,160.00,DR,668244475691,799-089800123904,DE PAYMENT STOPP,
299912345678,10/02/2017,ELLIS NELL JOSEPHINE MRS,57.64,DR,241677025582,167-897531605731,DE DEC"D CUST,
299912345792,10/02/2017,SAMPLE AUSTRALIA,881.37,CR,0000005DR0000012CR,062-875012123401,CBA TEST,
299912345792,10/02/2017,SAMPLE AUSTRALIA,2708.90,CR,0000000DR0000011CR,062-875012123401,CBA TEST,
299912345792,10/02/2017,SAMPE AUSTRALIA PTY LTD,5059.95,CR,9876543231,062-877 11234562,AMEX GR 45561.12,
GRANDTOTAL,10/02/2017,TRANS,17, CR AMT,9194.09, DR AMT,1242.28,
```

# **CSV format (New variant February 2018) – BTRS Complement** 

The Direct Entry Transactions Report can be downloaded in a variable length comma delimited format. Some fields has been modified to permit easier association with the corresponding transaction in the BTRS report. 

This file format can be used to import account information into third party Account Software packages. 

# **File details** 

Variable length comma delimited, ASCII text File type 

<u>The file consists of 2 record types.  These are:</u> 

Transaction detail record 



Page **11** of **14** Commercial in Confidence 



Conmonwealth Csi: f 

# **Sample File** 

```
06202877777777,171204,NYKANEN SMITH,30048,DR,31955283671,062-246 28000000,DE DEC"D CUST,D733800815550001NPA
06202877777777,171204,PAUL  SMITH,5732,DR,41687306302,062-275 00000000,DE PAYMENT STOPP,D733800815071001NPA
06202877777777,171204,WARBURTON SMITH E,8079,DR,36685965042,062-528 10000000,DE DEC"D CUST,D733800815605001NPA
06202877777777,171204,THELMA SMITH,1469,DR,42522316233,762-293  0000000,DE REFER TO CUST,H100251054039M7KAHK6
GRANDTOTAL,171204,TRANS,18,CR AMT,0,DR AMT,355861,
```



Page **14** of **14** Commercial in Confidence 

