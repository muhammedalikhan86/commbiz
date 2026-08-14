# Priority Payments & Non CBA Payment Requests (MT101) CommBiz File Specification International Money Transfers,

Version No. 9.2

10 August 2022

Public


## 1. File Specification

## 1.1 Transaction Fields

Each transaction consists of the following fields:

|   | # | Field Name | Description |
| --- | --- | --- |
|   | Transaction Type | [To specify whether the transaction is an International Money Transfer (IMT), Priority Payment (PP) or a Non CBA Payment Request (NonCBA) |
| 2 | Transaction Description | Short description of the transaction (for your records). |
| 3 | Process Date | The value date of the transaction. |
| 4 | Payment Currency | The currency for the credit side of the transaction (see Appendix B| for currency codes). |
| 5 | Payment Amount | The amount to be credited. |
| 6 | Debit Amount | The amount to be debited. |
| 7 | Debit Account — Account Number | The debit account number. |
|   | 8 | Dealer Code | The reference code given by a CBA Foreign Exchange dealer for a negotiated customer rate. |
| 9 | Dealer Exchange Rate | The exchange rate given by a CBA Foreign Exchange dealer. |
|   |   | 10 | Intermediary Bank — Bank Code | The bank code of the intermediary bank. |
|   | 11 | Intermediary Bank — Name | The name of the intermediary bank. |
|   | 12 | Intermediary Bank — City | The name of the city where the intermediary bank resides. |
|   | 13 | Intermediary Bank — Country Code | The country code for the intermediary bank (see Appendix A for country codes). |
|   |   | 14 | Beneficiary Bank — Bank Code | The bank code of the beneficiary bank. |
|   | 15 | Beneficiary Bank — Name | The name of the beneficiary bank. |
|   | 16 | Beneficiary Bank — City | The name of the city where the beneficiary bank resides. |
|   | 17 | Beneficiary Bank — Country Code | The country code for the beneficiary bank (see Appendix A for country codes). |
|   | 18 | Beneficiary | — Account Number | The beneficiary account number. |
|   | 19 | Beneficiary — Account Name | [The name of the beneficiary account. |
|   | 20 | Beneficiary — Address line 1 | Beneficiary address line 1. |
|   | 21 | Beneficiary — Address line 2 | Beneficiary address line 2. |
|   | 22 | Beneficiary — Address line 3 | Beneficiary address line 3. |
|   | 23 | Beneficiary — City | The city where the beneficiary resides. |
|   | 24 | Beneficiary — State | The state/region, etc where the beneficiary resides. |


|   | 25 | Beneficiary — Postcode | The postal code where the beneficiary resides. |
| --- | --- | --- |
|   | 26 | Beneficiary — Country Code | The country code for the beneficiary (see Appendix A for country codes). |
|   | # | Field Name | Description |
| 27 | Payment Details | Long description of the transaction. Sent with the payment for ladvice to the beneficiary. |
| 28 | of Charge | (Non CBA Payment Requests only) Specify if the beneficiary will the changes, or if you as the ordering customer will. |
| 29 | |Urgent Payment | (Non CBA Payment Requests only) Specify if you wish for your bank [to process the payment as urgent. |
| 30 | [Ordering Bank — BIC | (Non CBA Payment Requests only) Specify the BIC of the bank where the debit account (field 7) is held. |

## 1.2 File Format Rules

| No. | Rule Description |
| --- | --- |
| 1 | Each transaction in a file: ® must be a one debit / one credit transaction; ® IMT, Priority Payment or Non CBA Payment Request (i.e. payment types can be can be an mixed in a file). |
| 2 | Each transaction is separated by a CRLF (Carriage Return and Line Feed) character. Do not insert a CRLF character at the end of the last record in a file. It will create an additional blank record which will result in a warning message: “A formatting error has occurred for 1 row/s. Please ensure the row contains 27 fields.” |
| 3 | Each file can contain up to 350 transactions. |
| 4 | Each field in the transaction must end in a comma, except the last field. |
| 5 | Unused fields must be included in the file (i.e.,,). This includes fields that are not applicable to the payment type. |
| 6 | Each numeric (N) field can contain: 0123456789 Note: For amount fields a decimal point is also allowed. |
| 7 | Each alphanumeric (AN) field can contain: ABCDEFGHIJKLMNOPQRSTUVWXYZ abcdefghijklmnopqgrstuvwxyz 0123456789 Space = HEX 20 Hyphen = HEX 2D Apostrophe = HEX 27 Note: The first character in an alphanumeric field must be Ato Z, ato z, or 0 to 9. |
| 8 | Each alpha (A) field can contain: ABCDEFGHIJKLMNOPQRSTUVWXYZ abcdefghijklmnopqgrstuvwxyz |
| 9 | Country codes and currency codes must be upper case characters.|
| 10 | Ensure that a comma is not included within a field, e.g, “Invoices 20061234, 20061235". Ensure that the ampersand character (“&") is not used at all. |
|11 | IMT or Non CBA payment request — Intermediary Bank (fields 10-13) if payment through an intermediary bank is required, the final payment details must contain a SWIFT BIC code in field 10. This can either be populated directly in the import file, or other detail provided in fields 10- 13 to allow a manual search in CommBiz to locate the appropriate SWIFT BIC. |
| 12 | IMT or Non CBA payment request — Beneficiary Bank (fields 14-17) — generally, the most unique piece of information to identify the beneficiary bank is the SWIFT BIC code. However, some banks require the branch name to be quoted as well. The SWIFT BIC can either be populated directly in the import file, or other detail provided in fields 14-17 to allow a manual search in CommBiz to locate the correct bank. |

## 1.3 Sample File (one International Money Transfer and one Priority Payment)

PP,Import PP,090226,,1,,200010969292,,,,,,,032000,,,,1234567,ABC and Co,,,,,,,,a, IMT,Import IMT,090226,USD,,100,200010969292,,,,,,,ABNAUS4TLTR,,,US,12345,JAKA Exports,1 Good St,,,,,90210,US,abcd
NONCBA,ImportNonCBA,090227,USD,112.86,,8877665544,,,,,,,CITIUS33,,,US,100660296USD115601,Shefa li,5 Modern St,,,,,,US,NONCBAFCA Invoice 11,BEN,,CALCUS6LXXX

## 1.4 IMT payment Field Definition

|No.|Field Name| Requirement| Format Validation | Sample |
| --- | --- | --- | --- | --- |
|  1 | Transaction Type | Mandatory | * Must be “IMT” or “imt". | IMT |
| 2 | Transaction Description | Mandatory | * | Oct imports |
| 3 | Process Date | Mandatory | * Must be exactly 6N. Must be a valid date specified in YYMMDD format. * Must be greater than or equal to the import date. * Must be less than or equal to 7 days from the import date. | 231106 |
| 4 | Payment Currency | Mandatory | Must be exactly 3A upper case characters. See Appendix B for currency codes. | usb |
| 5 | Payment Amount | Conditional — Populate this field if know the | you amount you want | in the quoted in | « currency field 4. Either field 5 or field 6 is mandatory. | * Decimal point is optional. 1N to 11N before decimal point. 1N or 2N after the decimal point. Must be > 0. | 200145.80 |
| 6 | Debit Amount | Conditional — Populate this field if want to you pay an equivalent amount in the currency quoted in field 4. Either field 5 or 6 is mandatory. | Decimal point is optional. e 1N to 11N before decimal point. 1N or 2N after the decimal point. * Mustbe >0. |   |
| 7 | Debit Account — Account Number | Mandatory | Up to 34AN (space, — and , are not permitted). Must be an active CommBiz account where the import user has transaction entitlements to “Create” IMTs. | 200010130497 (account number must begin with the last 4 digits of the BSB code); |
| 8 | Dealer Code | Conditional — Mandatory if field 9 is populated. | Must be exactly 3N. Must not equal 205 or 285. | 012 |
| 9 | Dealer Exchange Rate | Conditional — Mandatory if field 8 is populated. | Decimal point is optional. 1N to 4N before the decimal point. 1N to 4N after the decimal point. | 73.0145 |
| 10 | Bank Code | Intermediary Bank — | Optional — one or more of fields 10-12 are required for CommBiz bank search. Mandatory in final payment if the IMT requires an intermediary bank. | Up to 11AN. For Straight-Through-Processing must be either an 8 or (no spaces). Use the CommBiz Bank 11AN SWIFT BIC code (2m screen of “Create IMT") or see Appendix Search function a SWIFT BIC code. C to locate or verify | ABNAUS33 or ABNAUS33XXX or 12345678912 |
| 11 | Intermediary Bank — | Name | Optional — one or more of fields 10-12 are required for CommBiz bank search. | Up to 30AN. Can be used to perform a manual search in CommBiz upon importing the file. | ABN Amro Bank |
| 12 | Intermediary Bank — | Optional City | — one or more of fields 10-12 are required for CommBiz bank search. | Can be used to perform a manual search in CommBiz upon importing the file. | New York |
| 13 | Intermediary Institution — Country | Conditional — Mandatory if the IMT requires an intermediary bank. | Must be exactly 2A upper case characters. See Appendix A for| country codes. | US |
| 14 | Beneficiary Bank — Bank Code | Conditional — one or more of fields 14-16 are | ® required for CommBiz bank search. | eo For Straight-Through-Processing must be either an 8 or (no spaces). Use the CommBiz Bank 11AN SWIFT BIC code Search function (2™ screen of “Create IMT") or see Appendix C to locate or verify a SWIFT BIC code. | CITIUS33 or CITIUS33XXX or 12345678912 |
| 15 | Beneficiary Bank — Name | Conditional — one or more of fields 14-16 are | required for CommBiz bank search. | Upto 30AN. Can be used to perform a manual search in CommBiz upon importing the file. | Citibank |
| 16 | Beneficiary Bank — City | Conditional — one or more of fields 14-16 are | * required for CommBiz bank search. | Upto 12AN. Can be used to perform a manual search in CommBiz upon importing the file. | New York |
| 17 | Beneficiary Bank — Country | Mandatory | Must be exactly 2A upper case characters. See Appendix A for| country codes. | US |
| 18 | Beneficiary — Account Number | Mandatory | Up to 34AN (space — and , are not permitted). | 22331322 |
| 19 | Beneficiary — Account Name | Mandatory | Upto 62AN. Must only contain letters, numbers, spaces, hyphens or apostrophes. Must contain at least 1A character (does not contain only numbers and /or special characters) | ABC Limited |
| 20 | Beneficiary — Address line 1 | Mandatory | 40AN. * Must only contain letters, numbers, spaces, hyphens or apostrophes. | 101 Fifth Avenue |
| 21 | Reserved For Future Use | Mandatory | Must be blank — the payment will be rejected if a value is specified |   |
| 22 | Reserved For Future Use | Mandatory | Must be blank — the payment will be rejected if a value is specified |   |
| 23 | Beneficiary — City | Optional | e Upto 19AN. Must only contain letters, numbers or spaces. | New York |
| 24 | Beneficiary — State | Optional | Must only contain letters and/or numbers. | NY |
| 25 | Beneficiary — Postcode | Optional | UptoB8AN. * Must only contain letters or numbers. | 90000 |
| 26 | Beneficiary — Country | Mandatory | Must be exactly 2A upper case characters. See Appendix A for| country codes. | US |
| 27 | Beneficiary Payment Details | Mandatory | Upto 105AN. | Invoice 23433 |


## 1.5 Priority Payment Field Definition

|  No. | Field Name | Requirement | Format Validation | Sample |
| --- | --- | --- | --- | --- |
| 1 | Transaction Type | Mandatory | Must be “PP” or "pp". | PP |
| 2 | Transaction Description | Mandatory |   | Sales comm |
| 3 | Process date | Mandatory | Must be exactly 6N. Must be a valid date specified in YYMMDD format. Must be greater than or equal to the import date. * Must be less than or equal to 14 months from the import date. | 231106 |
| 4 | Payment Currency | Not applicable for a Priority Payment |   |   |
| 5 | Payment Amount | Mandatory | * Decimal point is optional. * 1N to 11N before decimal point. 1N or 2N after the decimal point. >0. | 989.99 |
| 6 | Debit Amount | Not applicable for a Priority Payment |   |   |
| 7 | Debit — Account Number | Mandatory | Up to 34AN (space — and ,, are not permitted). * Must be an active CommBiz account where the import user | number must begin with the has transaction entitlements to “Create” Priority Payments. | 200010130497 (account last 4 digits of BSB code) |
| 8 | Dealer Code | Not applicable for a Priority Payment |   |   |
| 9 | Dealer Exchange Rate | Not applicable for a Priority Payment |   |   |
| 10 | Bank Code | Intermediary Bank - | Not applicable for a Priority Payment |   |   |
| 11 | Name | Intermediary Bank - | Not applicable for a Priority Payment |   |   |
| 12 | City | Intermediary Bank - | Not applicable for a Priority Payment |   |   |
| 13 | Country | Intermediary Bank - | Not applicable for a Priority Payment |   |   |
| 14 | Beneficiary Bank — Bank Code (i.e. BSB number) | Mandatory | Must be exactly 6N. | 032000 |
| 15 | Beneficiary Bank — Name | Not applicable for a Priority Payment |   |   |
| 16 | Beneficiary Bank — City | Not applicable for a Priority Payment |   |   |
| 17 | Beneficiary Bank — Country | Not applicable for a Priority Payment |   |   |
| 18 | Beneficiary — Account Number | Mandatory | e Must be 3AN to 9AN. | 600310145 |
| 19 | Beneficiary Name | Mandatory | Upto 32AN. * Must only contain letters, numbers or spaces. | XYZ Pty Ltd |
| 20 | Beneficiary — Address line 1 | Optional | Upto 40AN Must only contain letters, numbers or spaces. | 221 George St |
| 21 | Beneficiary — Address line 2 | Optional | Upto 40AN * Must only contain letters, numbers or spaces. |   |
| 22 | Beneficiary — Address line 3 | Optional | * Must only contain letters, numbers or spaces. |   |
| 23 | Beneficiary — City | Optional | Upto19AN. * Must only contain letters, numbers or spaces. | Sydney |
| 24 | Beneficiary — State | Optional | Upto 4AN. Must only contain letters and/or numbers. | Nsw |
| 25 | Beneficiary — Postcode | Optional | e * Must only contain letters or numbers. | 2000 |
| 26 | Beneficiary — Country Code | Optional | * Must be exactly 2A upper case characters. See Appendix A for | AU country codes. |   |
| 27 | Beneficiary Payment Details | Optional | Upto 105AN. | Sales commission SeptOct |


## 1.6 Non CBA Payment Request Field Definition
|  No. | Field Name | Requirement | Format Validation | Sample |
| --- | --- | --- | --- | --- |
| 1  | Transaction Type | Mandatory | Must be “NONCBA” or “NonCBA". | NonCBA |
| 2 | Transaction Description | Mandatory | Upto 12AN. | BoNY Xfer2 |
|  3 | Process date | Mandatory | Must be exactly 6N. Must be a valid date specified in YYMMDD format. Must equal to the import date. | 231106 |
| 4 | Payment Currency | Mandatory | Must be exactly 3A upper case characters. See Appendix B for currency codes. | usb |
| 5 | Payment Amount | Mandatory | «Decimal point is optional. 1N to 11N before decimal point. or 2N after the decimal point. * Mustbe>0. | 989.99 |
| 6 | Debit Amount | Not applicable for a Non CBA Payment Request |   |   |
| 7 | Debit — Account Number | Mandatory | Up to 34AN (space — and , are not permitted). * Must be aNon CBA Account active on CommBiz where the import user has transaction entitlements to “Create” Non CBA Payment Requests on the account. | 57-12AF12345 |
| 8 | Dealer Code | Not applicable for a Non CBA Payment Request |   |   |
| 9 | Dealer Exchange Rate | Not applicable for a Non CBA Payment Request |   |   |
|   | No. | Field Name | Requirement | Format Validation | Sample |
| 10 | Intermediary Bank Bank Code | | Optional — one or more of fields 10-12 are required | ® for CommBiz bank search. Mandatory in final payment if the Non CBA Payment Request requires an intermediary bank. | |e Upto 11AN. For Straight-Through-Processing must be either an 8 or (no spaces). See Appendix C to locate | ABNAUS33XXX 11AN SWIFT BIC code or verify a SWIFT BIC code. | ABNAUS33 or |
| 11 | Name | Intermediary Bank - | Optional — one or more of | fields 10-12 are required | ® for CommBiz bank search. | Up to 30AN. Can be used to perform a manual search in CommBiz upon importing the file. | ABN Amro Bank |
| 12 | Intermediary Bank - | Optional City | — one or more of fields 10-12 are required | ® for CommBiz bank search. | |e Upto 12AN. Can be used to perform a manual search in CommBiz upon importing the file. | New York |
| 13 | Intermediary Institution — Country | Conditional — Mandatory if the Non CBA Payment Request requires an intermediary bank. | Must be exactly 2A upper case characters. See Appendix A for | US country codes. |   |
| 14 | Beneficiary Bank — Bank Code | Conditional — one or more | of fields 14-16 are required for CommBiz bank search. | Up to 11AN. For Straight-Through-Processing must be either an 8 or (no spaces). See Appendix C to locate | 11AN SWIFT BIC code or verify a SWIFT BIC code. | CITIUS33 or |
| 15 | Beneficiary Bank — Name | Conditional — one or more | of fields 14-16 are required for CommBiz bank search. | Up to 30AN. Can be used to perform a manual search in CommBiz upon importing the file. | Citibank |
| 16 | Beneficiary Bank — City | Conditional — one or more | of fields 14-16 are required for CommBiz bank search. | Up to 12AN. * Can be used to perform a manual search in CommBiz upon importing the file. | New York |
| 17 | Beneficiary Bank — Country | Mandatory | Must be exactly 2A upper case characters. See Appendix A for | US country codes. |   |
| 18 | Beneficiary — Account Number | Mandatory | Up to 34AN (space — and , are not permitted). | 22331322 |
| 19 | Beneficiary — Account Name | Mandatory | Upto 62AN. * Must only contain letters, numbers or spaces. Must contain at least 1A character and not contain only numbers or characters | ABC Limited |
| 20 | Beneficiary — Address line 1 | Mandatory | Must only contain letters, numbers or spaces. Physical address (No post office box address) | 101 Fifth Avenue |
| 21 | Reserved For Future | N/A Use |   | Must be blank — the payment will be rejected if a value is specified |   |
| 22 | Reserved For Future | N/A Use |   | Must be blank — the payment will be rejected if a value is specified |   |
| --- | --- | --- | --- | --- |
| 23 | Beneficiary — City | Optional | Upto 19AN. Must only contain letters, numbers or spaces. | New York |
| 24 | Beneficiary — State | Optional | eo Must only contain letters and/or numbers. | NY |
| 25 | Beneficiary — Postcode | Optional | Upto BAN. Must only contain letters or numbers. | 90000 |
| 26 | Beneficiary — Country | Mandatory |   | Must be exactly 2A upper case characters. See Appendix A for | US country codes. |   |
| 27 | Beneficiary Payment Details | Mandatory | Upto 105AN. | Invoices 1223 and 3334 Sept-Oct |
| 28 | Details of Charge | Mandatory | eo “BEN ” - charge any bank fees to beneficiary “OUR ” - charge bank fees to ordering customer | BEN |
| 29 | Urgent Payment | Optional | "Y ” torequest urgent payment, otherwise null. | Y |
| 30 | Ordering Bank - BIC | Mandatory |   | Must match exactly the BIC of the Non CBA account being debited. BIC can be sourced on the CommBiz Account Information screen per Appendix E. | CALCUSBLXXX |


## 2. Appendix A — Country Codes

| Code | Country |
| --- | --- |
| AD | Andorra |
| AE | United Arab Emirates |
| AF | Afghanistan |
| AG | Antigua And Barbuda |
| Al | Anguilla |
| AL | Albania |
| AM | Armenia |
| AN | Netherlands Antilles |
| AO | Angola |
| AQ | Antarctica |
| AR | Argentina |
| AS | American Samoa |
| AT | Austria |
| AU | Australia |
| AW | Aruba |
| AX | Aland Islands |
| AZ | Azerbaijan |
| BA | Bosnia And Herzegovina |
| BB | Barbados |
| BD | Bangladesh |
| BE | Belgium |
| BF | Burkina Faso |
| BG | Bulgaria |
| BH | Bahrain |
| BI | Burundi |
| BJ | Benin |
| BM | Bermuda |
| BN | Brunei Darussalam |
| BO | Bolivia |
| BR | Brazil |
| BS | Bahamas |
| BT | Bhutan |
| BV | Bouvet Island |
| BW | Botswana |
| BY | Belarus |
| BZ | Belize |
| CA | Canada |
| CD | Democratic Rep Of Congo |
| CF | Central African Republic |
| CG | Congo |
| CH | Switzerland |
| Cl | Cote Divoire |
| CK | Cook Islands |
| CL | Chile |
| Cm | Cameroon |
| CN | China |
| CO | Colombia |
| CR | Costa Rica |
| CS | Serbia And Montenegro |
| Cu | Cuba |
| cv | Cape Verde |
| CY | Cyprus |
| cz | Czech Republic |
| DE | Germany |
| DJ | Djibouti |
| DK | Denmark |
| DM | Dominica |
| DO | Dominican Republic |
| Dz | Algeria |
| EC | Ecuador |
| EE | Estonia |
| EG | Egypt |
| EH | Western Sahara |
| ER | Eritrea |
| ES | Spain |
| ET | Ethiopia |
| Fl | Finland |
| FJ | Fiji |
| FK | Falkland Islands |
| FM | Micronesia |
| FO | Faroe Islands |
| FR | France |
| GA | Gabon |
| GB | United Kingdom |
| GD | Grenada |
| GE | Georgia |
| GF | French Guiana |
| GG | Guernsey |
| GH | Ghana |
| Gl | Gibraltar |
| GL | Greenland |
| GM | Gambia |
| GN | Guinea |
| GP | Guadeloupe |
| GQ | Equatorial Guinea |
| GR | Greece |
| GS | South Georgia Island |
| GT | Guatemala |
| GU | Guam |
| GW | Guinea-Bissau |
| GY | Guyana |
| HE | Herzegovina |
| HK | Hong Kong |
| HM | Heard & Mcdonald Islands |
| HN | Honduras |
| HR | Croatia |
| HT | Haiti |
| HU | Hungary |
| ID | Indonesia |
| IE | Ireland |
| IL | Israel |
| IM | Isle Of Man |
| IN | India |
| 10 | British Indian Ocean Ter |
| 1Q | Iraq |
| IR | Iran |
| IS | Iceland |
| IT | Italy |
| JE | Jersey |
| JM | Jamaica |
| Jo | Jordan |
| JP | Japan |
| KE | Kenya |
| KG | Kyrgyzstan |
| KH | Cambodia |
| KI | Kiribati |
| KM | Comoros |
| KN | Saint Kitts And Nevis |
| KP | Dem Peoples Rep Of Korea |
| KR | Republic Of Korea |
| KW | Kuwait |
| KY | Cayman Islands |
| KZ | Kazakhstan |
| LA | Lao Peoples Dem Republic |
| LB | Lebanon |
| LC | Saint Lucia |
| LI | Liechtenstein |
| LK | Sri Lanka |
| LR | Liberia |
| LS | Lesotho |
| LT | Lithuania |
| LU | Luxembourg |
| LV | Latvia |
| LY | Libyan Arab Jamahiriya |
| MA | Morocco |
| MC | Monaco |
| MD | Republic Of Moldova |
| MG | Madagascar |
| MH | Marshall Islands |
| MK | Macedonia |
| ML | Mali |
| MM | Myanmar |
| MN | Mongolia |
| MO | Macau |
| MP | Northern Mariana Islands |
| MQ | Martinique |
| MR | Mauritania |
| MS | Montserrat |
| MT | Malta |
| SS | South Sandwich Islands |
| ST | Sao Tome And Principe |
| SV | El Salvador |
| SY | Syrian Arab Republic |
| SZ | Swaziland |
| TC | Turks And Caicos Islands |
| TD | Chad |
| TF | French Southern Territory |
| TG | Togo |
| TH | Thailand |
| TJ | Tajikistan |
| TK | Tokelau |
| TL | East Timor |
| TM | Turkmenistan |
| TN | Tunisia |
| TO | Tonga |
| TR | Turkey |
| TT | Trinidad And Tobago |
| TV | Tuvalu |
| TW | Taiwan |
| TZ | Tanzania |
| UA | Ukraine |
| UC | Uganda |
| UM | Us Minor Outlying Islands |
| US | United States |
| UY | Uruguay |
| UZ | Uzbekistan |
| VA | Vatican City State |
| VC  | St Vincent & Grenadines |
| VE | Venezuela |
| VG | British Virgin Islands |
| VI | Us Virgin Islands |
| VN | Vietnam |
| VU | Vanuatu |
| WF | Wallis And Futuna Islands |
| WS | Samoa |
| YE | Yemen |
| YT | Mayotte |
| YU | Serbia Montenegro |
| ZA | South Africa |
| ZM | Zambia |
| ZW | Zimbabwe |


## 3. Appendix B — Currency Codes

| Currency | Code |
| --- | --- |
| Australian Dollar | AUD |
| US Dollar | UsD |
| Great British Pound | GBP |
| Canadian Dollar | CAD |
| Czech Koruna | CZK |
| Danish Kroner | DKK |
| Euro | EUR |
| Fijian Dollar | FJD |
| Hong Kong Dollar | HKD |
| Indian Rupee | INR |
| Indonesian Rupiah | IDR |
| Israeli New Shekel | ILS |
| Japanese Yen | JPY |
| Kuwaiti Dinar | KWD |
| New Cal/Tahiti Franc | XPF |
| New Zealand Dollar | NZD |
| Norwegian Kroner | NOK |
| Oman Rial | OMR |
| P.N.G. Kina | PGK |
| Philippine Peso | PHP |
| Polish Zloty | PLN |
| Saudi Riyal | SAR |
| Singapore Dollar | SGD |
| Solomon Island Dollar | SBD |
| South African Rand | ZAR |
| South Korean Won | KRW |
| Sri Lankan Rupee | LKR |
| Swedish Kronor | SEK |
| Swiss Franc | CHF |
| Thai Baht | THB |
| Turkish Lira | TRY |
| U.A.E. Dirham | AED |
| Vanuatu Vatu |   |
| Vietnamese Dong | VND |


## 4. Appendix C — Locate or Verify a SWIFT BIC Code

SWIFT is the industry-owned co-operative supplying secure, standardised messaging services to financial institutions worldwide. SWIFT members are identified by a Bank Identifier Code (BIC). To find a SWIFT BIC, or to verify an existing BIC, go to the following web page:

https://wise.com/gb/swift-codes/bic-swift-code-checker

Type in the details to search for and click on “Search”. The “Country name” field is not usually required unless the city name being searched for could exist in more than one country. In this example the search is for the SWIFT BIC for CIMB Bank, Kuala Lumpur, Malaysia:


## The search results have returned a SWIFT BIC code of CIBBMYKLXXX

A SWIFT BIC code is comprised or three or four segments. Using CIBBMYKLXXX as an example, these are:

CiBB

\- 4 character bank code

MY

\- 2 character country code

\- 2 character city code

KL

XXX

\- 3 character branch code (optional)

SWIFT BIC codes can be quoted without the “XXX” at the end. The “XXX” indicates that the BIC code is valid for all branches of that bank in that region or country.

The 8-character BIC code is the minimum that must be used. If a branch code is quoted in the fourth segment, this should be used if appropriate.

This code can be used in fields 10 or 14 of an IMT payment in the CommBiz import file.


## 5. Appendix D — Check an IBAN (International Bank Account Number)

An International Bank Account Number (IBAN) is an initiative of the European Committee for Banking Standards to facilitate the automatic processing of cross-border credit transfers. It was later adopted by the International Standards Organisation (ISO). Since 1st January 2007 the IBAN of the beneficiary and the SWIFT BIC of the beneficiary bank are mandatory in payments to a bank account in the European Union (EU).

If are paying funds to a counterparty in one of the EU countries, it is their responsibility to advise you you their IBAN and the SWIFT BIC of their bank. These details should be quoted on letterheads and invoices from counterparty, if not you will need to obtain these details from them prior to making a payment. your

An IBAN is a series of alphanumeric characters (maximum of 34) that uniquely identifies an account held at a bank anywhere in the world. It contains an ISO country code, two check digits and the basic bank account number of the account. The banking industry in each country in the EU has specified the length and composition of the IBAN for their country.

When an IBAN is printed on paper (e.g. letterheads and invoices) it is normally shown split into groups of four characters for recognition. When used electronically (e.g. in a payment message) it is a single easy string without spaces, as per the following samples:

France

electronic FR1420041010050500013M02606

-

Netherlands

NL91 ABNA 0417 1643 00

\- on paper

electronic

-

A bank in Australia cannot provide you with the IBAN of the counterparty you are paying, nor advise if the IBAN quoted is correct. Currently, bank systems in Australia can only check that an IBAN quoted is the correct length for the country the payment is going to.

To check the validity of an IBAN, the following websites may be of assistance:

\- on paper FR14 2004 1010 0505 0001 3M02 606

## United Nations Centre for Trade Facilitation and Electronic Business

https://www.tbg5-finance.org/?ibancheck.shtml


## 6. Appendix E — Locate the SWIFT BIC of your Non CBA Account

To locate the SWIFT BIC (Bank Identification Code) of your Non CBA Account, navigate to the account

information screen from the CommBiz homepage, hover on “Accounts

and select “Account

”

then select the account you wish to view and click “Search

Information

”

”

,

.
