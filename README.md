# POS (Point of Sale) System

This repository contains a Point of Sale (POS) application written in C#. It is designed to manage sales transactions, product inventories, deals, and order processing for small or medium-sized businesses.

## Features

- **Product Management:**  
  - Add, update, and delete products with details such as name, description, price, stock, and barcode.
  - Organize products by categories.

- **Order Processing:**  
  - Create and manage customer orders.
  - Add products (and deals) to customer carts.
  - Track order totals, payments, and calculate change automatically.

- **Cart Service:**  
  - Add items to cart, increment/decrement quantities, and clear cart after payment.
  - Track quantities and pricing dynamically.

- **Deals and Promotions:**  
  - Manage special deals with associated items and prices.
  - Add, update, and retrieve deals.

- **Database Integration:**  
  - Uses SQLite for persistent local storage.
  - Handles CRUD operations for products, categories, orders, order items, deals, and deal items.

- **Search Functionality:**  
  - Search for products and deals by name or attributes.

## Data Models

- **ProductItem:** Represents a product in inventory.
- **Order & OrderItem:** Represent customer orders and their line items.
- **Deal & DealItem:** Represent special promotions and their contents.
- **Category:** Organizes products for easy browsing and management.

## Technologies Used

- **C#**
- **SQLite** (via `SQLiteAsyncConnection`)
- **MVVM** pattern with CommunityToolkit.Mvvm for maintainable and testable code.
- **.NET MAUI** and CommunityToolkit.Maui (if used for UI, inferred from toolkit references).

## Getting Started

1. **Clone the repository:**
   ```sh
   git clone https://github.com/abdullahsajid/pos.git
   ```

2. **Open in Visual Studio:**  
   Open the solution and restore dependencies.

3. **Build and Run:**  
   Build the project and run it using your preferred target (desktop or mobile).

4. **Database Initialization:**  
   The app automatically initializes SQLite tables for products, categories, orders, deals, etc.

## Screenshots

Below are some screenshots of the POS system in action:

<img src="https://github.com/user-attachments/assets/19edb03b-1308-4a51-9438-b9a84af60682" alt="POS System Main Screen" width="600"/>
<img src="https://github.com/user-attachments/assets/3a103865-b26b-4e5b-91a9-1871aa47a9c9" alt="Order Processing Screen" width="600"/>
<img src="https://github.com/user-attachments/assets/f358f1ae-d21b-4299-811f-e78727ebe178" alt="Deals and Promotions Screen" width="600"/>

## Contributing

Contributions are welcome! Please create an issue or pull request with your improvement.

## License

This project does not currently specify a license.

## Author

[abdullahsajid](https://github.com/abdullahsajid)

---

> **Note:** This POS system is intended as a starting point for further customization and deployment. Adapt and extend it as needed for your business context.
