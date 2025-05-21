VS 2019 version with .net 5.0

dotnet clean and dotnet run RECOMENDED

Start local server (5001) by Server1/Server1 dotnet run

Start check parse by Server1/CheckParser dotnet run -- "PATH TO .TXT CHECK FILE"

Start cashregister by Server1/CashRegister.Console dotnet run

acesss to api = (GET)

/api/Products , /api/Promotions , /api/ReceiptDto

if fails to do CechkParse and or CashRegister.Console..

cd to /Server1 dotnet clean and dotnet run SHOULD SOLVE IT

---------------------------------------------------------------
TWO NEW PROJECTS ADDED 
In CashRegister.Core is esental logic for check creation
In CashRegister.Test is Xunit tests of core logic and API call tests


Test File Descriptions

ApiIntegrationTests.cs
Performs live HTTP calls to the /api/products and /api/promotions endpoints to ensure the API is reachable and returns valid data.

ApiProductPromotionTests.cs
Fetches real product and promotion data from the API and applies promotion logic to ensure discounts are applied correctly.

ApiReceoptDtoTest.cs
Builds a complete ReceiptDto from real-time API data, validating the structure, pricing, totals, and VAT. Outputs a console-friendly summary of the generated receipt.

PaymentProcessorTests.cs
Tests the internal PaymentProcessor logic, including gift card usage, loyalty balance deduction, card/cash splits, and change handling.

PaymentTests.cs
Covers edge cases in payment application logic such as overpayments, loyalty card earnings, and combinations of payment methods.

PromotionTests.cs
Verifies correct application of all supported promotion types: FlatOff, PercentOff, and OneThirdOff, including stacking and UPC matching logic.

ReceiptDtoTests.cs
Validates the construction of ReceiptDto objects from core logic, ensuring correct item counts, payment representation, and VAT summary.

VatCalculationTests.cs
Tests VAT computation across multiple VAT rates, rounding scenarios, and 0% VAT use cases.
