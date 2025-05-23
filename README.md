# 🧾 Cash Register System API

> Modular .NET-based backend simulation for retail checkouts, loyalty systems, and digital receipt processing.

This project includes a complete **.NET API**, in-memory data stores, a console simulation of store logic, and comprehensive test coverage.

---

## 🔌 API Capabilities

The API exposes full RESTful CRUD endpoints for the following entities:

### 🛒 Products
**Fields:** `Name`, `Upc`, `Price`, `Vat`  
Register products, retrieve inventory, simulate price logic.

### 🎁 Promotions
**Fields:** `Type`, `Value`, `Start`, `End`, `Description`  
Built-in logic: `FlatOff`, `PercentOff`, `OneThirdOff`

### 💳 Gift Cards
Purchasable as products  
**Fields:** `Balance`, `IsActive`, `ActivatedAt`, `ValidUntil`  
Updated automatically on use

### 🎟️ Loyalty Cards
Persistent, re-usable cards  
**Fields:** `Balance`, `ActivatedAt`, `BalanceResetAt`, `IsActive`

### 🧾 Receipts
Full receipt structure including:
- `ReceiptLines` (name, upc, unit price, qty, discount)
- `VatLines` (rate, base, amount, total)
- `Payments`, `Change`, `Timestamp`
- Optional `RawText` field for audit/debug logging

---

## 📦 Check Parser

Parses scanned receipts into structured DTOs

- Matches product names/UPCs
- Applies discounts and VAT detection
- Outputs structured, server-usable receipts
- **No sensitive data stored or parsed**

---

## 💻 Console Simulation

Simulates real-life POS checkout experience:

- Random product selection
- Applies promo logic
- Interacts with GiftCard & LoyaltyCard APIs
- Accepts input for card/cash payments
- Builds and submits full receipt to API

---

## ⚙️ Core Logic (`CashRegister.Core`)

- **PricingEngine** — Applies all promo logic  
- **ReceiptCalculator** — Totals items, calculates VAT lines  
- **PaymentProcessor** — Simulates full payment logic  

---

## ✅ Unit + Integration Tests (`CashRegister.Test`)

- ✔ VAT calculations across multiple rates (21%, 12%, 0%)
- ✔ Promo logic (FlatOff, PercentOff, OneThirdOff)
- ✔ Receipt generation via DTO parsing
- ✔ API integration for Products, Promotions, GiftCards, LoyaltyCards, and Receipts
- ✔ GiftCard and LoyaltyCard full lifecycle simulation

---

## 🧠 Ideal For

- Prototyping checkout/tax logic
- Loyalty program simulation
- Interview take-home API challenges
- Backend logic demos or education

---

## 🚀 Get Started

**Environment:**

* Recommended IDE: Visual Studio 2019
* Runtime: .NET 5.0 SDK

Before running:

```bash
dotnet clean
dotnet build
```

### 🟢 Run API Server

```bash
cd Server
dotnet run
```

* Runs at `https://localhost:5001`
* Available endpoints:

  * `/api/products`
  * `/api/promotions`
  * `/api/receipts`
  * `/api/giftcards`
  * `/api/loyaltycards`

### 🧾 Run Check Parser

```bash
cd CheckParser
dotnet run -- "PATH_TO_YOUR_RECEIPT_FILE.txt"
```

* Parses raw .txt receipts into receipt DTOs and pushes them to the API.

### 💻 Run Console Simulation

```bash
cd CashRegister.Console
dotnet run
```

* Simulates real-time POS checkout with promo/gift/loyalty application.

Use Postman, Swagger, or CLI tools to interact with endpoints.

---

