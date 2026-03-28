// Stock Management Feature
// Add Product Feature
// Add Supplier Feature
// Add Category Feature
using System;
using System.Collections.Generic;
using System.Linq;

namespace InventoryManagementSystem
{
    

    public class Category
    {
        public int Id { get; private set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public Category(int id, string name, string description)
        {
            Id = id;
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Description = description ?? string.Empty;
        }

        public override string ToString()
        {
            return string.Format("[{0}] {1} - {2}", Id, Name, Description);
        }
    }

    public class Supplier
    {
        public int Id { get; private set; }
        public string Name { get; set; }
        public string ContactPerson { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }

        public Supplier(int id, string name, string contactPerson, string phone, string email, string address)
        {
            Id = id;
            Name = name ?? throw new ArgumentNullException(nameof(name));
            ContactPerson = contactPerson ?? string.Empty;
            Phone = phone ?? string.Empty;
            Email = email ?? string.Empty;
            Address = address ?? string.Empty;
        }

        public override string ToString()
        {
            return string.Format("[{0}] {1} | Contact: {2} | Phone: {3}", Id, Name, ContactPerson, Phone);
        }
    }

    public class Product
    {
        public int Id { get; private set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public int MinStockLevel { get; set; }
        public Category Category { get; set; }
        public Supplier Supplier { get; set; }
        public DateTime DateAdded { get; private set; }

        public Product(int id, string name, string description, decimal price, int quantity,
                      int minStockLevel, Category category, Supplier supplier)
        {
            Id = id;
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Description = description ?? string.Empty;
            Price = price >= 0 ? price : throw new ArgumentException("Price cannot be negative");
            Quantity = quantity >= 0 ? quantity : throw new ArgumentException("Quantity cannot be negative");
            MinStockLevel = minStockLevel >= 0 ? minStockLevel : throw new ArgumentException("Min stock level cannot be negative");
            Category = category ?? throw new ArgumentNullException(nameof(category));
            Supplier = supplier ?? throw new ArgumentNullException(nameof(supplier));
            DateAdded = DateTime.Now;
        }

        public decimal GetInventoryValue()
        {
            return Price * Quantity;
        }

        public bool IsLowStock()
        {
            return Quantity <= MinStockLevel;
        }

        public override string ToString()
        {
            string stockStatus = IsLowStock() ? " [LOW STOCK!]" : "";
            return string.Format("[{0}] {1}{2}\n    Description: {3}\n    Price: ${4:F2} | Quantity: {5} | Min Level: {6}\n    Category: {7} | Supplier: {8}\n    Added: {9:yyyy-MM-dd} | Value: ${10:F2}",
                Id, Name, stockStatus, Description, Price, Quantity, MinStockLevel,
                Category != null ? Category.Name : "N/A",
                Supplier != null ? Supplier.Name : "N/A",
                DateAdded, GetInventoryValue());
        }
    }

    public class User
    {
        public int Id { get; private set; }
        public string Username { get; set; }
        public string FullName { get; set; }
        public string Role { get; set; }
        private string Password { get; set; }

        public User(int id, string username, string fullName, string role, string password)
        {
            Id = id;
            Username = username ?? throw new ArgumentNullException(nameof(username));
            FullName = fullName ?? string.Empty;
            Role = role ?? "Staff";
            Password = password ?? throw new ArgumentNullException(nameof(password));
        }

        public bool Authenticate(string password)
        {
            return Password == password;
        }

        public override string ToString()
        {
            return string.Format("[{0}] {1} ({2}) - Role: {3}", Id, Username, FullName, Role);
        }
    }

    public enum TransactionType
    {
        StockIn,
        StockOut,
        Adjustment,
        Added,
        Updated,
        Deleted
    }

    public class TransactionRecord
    {
        public int Id { get; private set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public TransactionType Type { get; set; }
        public int QuantityChanged { get; set; }
        public int QuantityBefore { get; set; }
        public int QuantityAfter { get; set; }
        public string PerformedBy { get; set; }
        public DateTime Timestamp { get; private set; }
        public string Notes { get; set; }

        public TransactionRecord(int id, int productId, string productName, TransactionType type,
                               int quantityChanged, int quantityBefore, int quantityAfter,
                               string performedBy, string notes)
        {
            Id = id;
            ProductId = productId;
            ProductName = productName ?? "Unknown";
            Type = type;
            QuantityChanged = quantityChanged;
            QuantityBefore = quantityBefore;
            QuantityAfter = quantityAfter;
            PerformedBy = performedBy ?? "System";
            Timestamp = DateTime.Now;
            Notes = notes ?? string.Empty;
        }

        public override string ToString()
        {
            string change = QuantityChanged >= 0 ? string.Format("+{0}", QuantityChanged) : QuantityChanged.ToString();
            return string.Format("[{0}] {1:yyyy-MM-dd HH:mm} | {2,-12} | Product: {3} ({4})\n    Change: {5} | Before: {6} → After: {7}\n    By: {8} | Notes: {9}",
                Id, Timestamp, Type, ProductName, ProductId, change, QuantityBefore, QuantityAfter, PerformedBy, Notes);
        }
    }

    

    public class InventoryService
    {
        private List<Category> categories = new List<Category>();
        private List<Supplier> suppliers = new List<Supplier>();
        private List<Product> products = new List<Product>();
        private List<User> users = new List<User>();
        private List<TransactionRecord> transactions = new List<TransactionRecord>();

        private int nextCategoryId = 1;
        private int nextSupplierId = 1;
        private int nextProductId = 1;
        private int nextUserId = 1;
        private int nextTransactionId = 1;

        private User currentUser;

        public InventoryService()
        {
            users.Add(new User(nextUserId++, "admin", "Administrator", "Admin", "admin123"));
        }

        public bool Login(string username, string password)
        {
            var user = users.FirstOrDefault(u => string.Equals(u.Username, username, StringComparison.OrdinalIgnoreCase));
            if (user != null && user.Authenticate(password))
            {
                currentUser = user;
                return true;
            }
            return false;
        }

        public User GetCurrentUser() { return currentUser; }

        public void AddCategory(string name, string description)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Category name cannot be empty");

            var category = new Category(nextCategoryId++, name.Trim(), description != null ? description.Trim() : string.Empty);
            categories.Add(category);
            Console.WriteLine("Category added successfully: {0}", category.Name);
        }

        public List<Category> GetAllCategories() { return new List<Category>(categories); }

        public Category GetCategoryById(int id) { return categories.FirstOrDefault(c => c.Id == id); }

        public void AddSupplier(string name, string contactPerson, string phone, string email, string address)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Supplier name cannot be empty");

            var supplier = new Supplier(nextSupplierId++, name.Trim(), contactPerson != null ? contactPerson.Trim() : null,
                                       phone != null ? phone.Trim() : null, email != null ? email.Trim() : null, address != null ? address.Trim() : null);
            suppliers.Add(supplier);
            Console.WriteLine("Supplier added successfully: {0}", supplier.Name);
        }

        public List<Supplier> GetAllSuppliers() { return new List<Supplier>(suppliers); }

        public Supplier GetSupplierById(int id) { return suppliers.FirstOrDefault(s => s.Id == id); }

        public void AddProduct(string name, string description, decimal price, int quantity,
                              int minStockLevel, int categoryId, int supplierId)
        {
            ValidateProductInput(name, price, quantity, minStockLevel);

            var category = GetCategoryById(categoryId);
            if (category == null)
                throw new ArgumentException(string.Format("Category with ID {0} not found", categoryId));

            var supplier = GetSupplierById(supplierId);
            if (supplier == null)
                throw new ArgumentException(string.Format("Supplier with ID {0} not found", supplierId));

            var product = new Product(nextProductId++, name.Trim(), description != null ? description.Trim() : null,
                                     price, quantity, minStockLevel, category, supplier);
            products.Add(product);

            RecordTransaction(product.Id, product.Name, TransactionType.Added, quantity, 0, quantity,
                            "New product added to inventory");

            Console.WriteLine("Product added successfully: {0} (ID: {1})", product.Name, product.Id);
        }

        public List<Product> GetAllProducts() { return new List<Product>(products); }

        public Product GetProductById(int id) { return products.FirstOrDefault(p => p.Id == id); }

        public List<Product> SearchProducts(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return GetAllProducts();

            searchTerm = searchTerm.ToLower();
            return products.Where(p =>
                (p.Name != null && p.Name.ToLower().Contains(searchTerm)) ||
                (p.Description != null && p.Description.ToLower().Contains(searchTerm)) ||
                (p.Category != null && p.Category.Name != null && p.Category.Name.ToLower().Contains(searchTerm)) ||
                (p.Supplier != null && p.Supplier.Name != null && p.Supplier.Name.ToLower().Contains(searchTerm))
            ).ToList();
        }

        public void UpdateProduct(int productId, string name, string description,
                                 decimal? price, int? minStockLevel,
                                 int? categoryId, int? supplierId)
        {
            var product = GetProductById(productId);
            if (product == null)
                throw new ArgumentException(string.Format("Product with ID {0} not found", productId));

            string oldName = product.Name;
            int oldQty = product.Quantity;

            if (!string.IsNullOrWhiteSpace(name))
                product.Name = name.Trim();
            if (description != null)
                product.Description = description;
            if (price.HasValue)
            {
                if (price.Value < 0) throw new ArgumentException("Price cannot be negative");
                product.Price = price.Value;
            }
            if (minStockLevel.HasValue)
            {
                if (minStockLevel.Value < 0) throw new ArgumentException("Min stock level cannot be negative");
                product.MinStockLevel = minStockLevel.Value;
            }
            if (categoryId.HasValue)
            {
                var cat = GetCategoryById(categoryId.Value);
                if (cat == null) throw new ArgumentException("Category not found");
                product.Category = cat;
            }
            if (supplierId.HasValue)
            {
                var sup = GetSupplierById(supplierId.Value);
                if (sup == null) throw new ArgumentException("Supplier not found");
                product.Supplier = sup;
            }

            RecordTransaction(product.Id, product.Name, TransactionType.Updated, 0, oldQty, product.Quantity,
                            string.Format("Product updated. Old name: {0}", oldName));
            Console.WriteLine("Product updated successfully: {0}", product.Name);
        }

        public void DeleteProduct(int productId)
        {
            var product = GetProductById(productId);
            if (product == null)
                throw new ArgumentException(string.Format("Product with ID {0} not found", productId));

            RecordTransaction(product.Id, product.Name, TransactionType.Deleted, -product.Quantity,
                            product.Quantity, 0, "Product deleted from inventory");

            products.Remove(product);
            Console.WriteLine("Product deleted successfully: {0}", product.Name);
        }

        public void RestockProduct(int productId, int amount, string notes)
        {
            if (amount <= 0)
                throw new ArgumentException("Restock amount must be greater than zero");

            var product = GetProductById(productId);
            if (product == null)
                throw new ArgumentException(string.Format("Product with ID {0} not found", productId));

            int beforeQty = product.Quantity;
            product.Quantity += amount;

            RecordTransaction(product.Id, product.Name, TransactionType.StockIn, amount, beforeQty,
                            product.Quantity, notes);
            Console.WriteLine("Restocked {0} units of {1}. New quantity: {2}", amount, product.Name, product.Quantity);
        }

        public void DeductStock(int productId, int amount, string notes)
        {
            if (amount <= 0)
                throw new ArgumentException("Deduction amount must be greater than zero");

            var product = GetProductById(productId);
            if (product == null)
                throw new ArgumentException(string.Format("Product with ID {0} not found", productId));

            if (product.Quantity < amount)
                throw new InvalidOperationException(string.Format("Insufficient stock. Available: {0}, Requested: {1}", product.Quantity, amount));

            int beforeQty = product.Quantity;
            product.Quantity -= amount;

            RecordTransaction(product.Id, product.Name, TransactionType.StockOut, -amount, beforeQty,
                            product.Quantity, notes);
            Console.WriteLine("Deducted {0} units of {1}. Remaining: {2}", amount, product.Name, product.Quantity);
        }

        public List<Product> GetLowStockItems()
        {
            return products.Where(p => p.IsLowStock()).ToList();
        }

        public decimal GetTotalInventoryValue()
        {
            return products.Sum(p => p.GetInventoryValue());
        }

        public List<TransactionRecord> GetTransactionHistory(int? productId)
        {
            if (productId.HasValue)
                return transactions.Where(t => t.ProductId == productId.Value).ToList();
            return new List<TransactionRecord>(transactions);
        }

        private void RecordTransaction(int productId, string productName, TransactionType type,
                                      int quantityChanged, int qtyBefore, int qtyAfter, string notes)
        {
            var transaction = new TransactionRecord(nextTransactionId++, productId, productName, type,
                                                   quantityChanged, qtyBefore, qtyAfter,
                                                   currentUser != null ? currentUser.Username : "System", notes);
            transactions.Add(transaction);
        }

        private void ValidateProductInput(string name, decimal price, int quantity, int minStockLevel)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Product name cannot be empty");
            if (price < 0)
                throw new ArgumentException("Price cannot be negative");
            if (quantity < 0)
                throw new ArgumentException("Quantity cannot be negative");
            if (minStockLevel < 0)
                throw new ArgumentException("Minimum stock level cannot be negative");
        }
    }

    

    public class ConsoleUI
    {
        private InventoryService inventory;
        private bool running = true;

        public ConsoleUI()
        {
            inventory = new InventoryService();
        }

        public void Run()
        {
            ShowWelcomeScreen();

            if (!LoginScreen())
                return;

            while (running)
            {
                try
                {
                    ShowMainMenu();
                    string choice = Console.ReadLine();
                    if (choice != null)
                        choice = choice.Trim();
                    ProcessMenuChoice(choice);
                }
                catch (Exception ex)
                {
                    ShowError(ex.Message);
                }

                if (running)
                {
                    Console.WriteLine("\nPress any key to continue...");
                    Console.ReadKey(true);
                }
            }
        }

        private void ShowWelcomeScreen()
        {
            Console.Clear();
            Console.WriteLine("INVENTORY MANAGEMENT SYSTEM");
            Console.WriteLine();
        }

        private bool LoginScreen()
        {
            Console.WriteLine("Please login to continue");
            Console.WriteLine("(Default: admin / admin123)");
            Console.WriteLine();

            int attempts = 3;
            while (attempts > 0)
            {
                Console.Write("Username: ");
                string username = Console.ReadLine();
                Console.Write("Password: ");
                string password = ReadPassword();

                if (inventory.Login(username, password))
                {
                    Console.WriteLine();
                    Console.WriteLine("Welcome, {0}!", inventory.GetCurrentUser().FullName);
                    System.Threading.Thread.Sleep(1000);
                    return true;
                }

                attempts--;
                Console.WriteLine();
                Console.WriteLine("Invalid credentials. Attempts remaining: {0}", attempts);
            }

            Console.WriteLine("Too many failed attempts. Exiting...");
            return false;
        }

        private string ReadPassword()
        {
            System.Text.StringBuilder password = new System.Text.StringBuilder();
            ConsoleKeyInfo key;

            do
            {
                key = Console.ReadKey(true);
                if (key.Key != ConsoleKey.Enter && key.Key != ConsoleKey.Backspace)
                {
                    password.Append(key.KeyChar);
                    Console.Write("*");
                }
                else if (key.Key == ConsoleKey.Backspace && password.Length > 0)
                {
                    password.Length--;
                    Console.Write("\b \b");
                }
            } while (key.Key != ConsoleKey.Enter);

            return password.ToString();
        }

        private void ShowMainMenu()
        {
            Console.Clear();
            Console.WriteLine("MAIN MENU");
            Console.WriteLine();
            Console.WriteLine("  [1]  Manage Categories");
            Console.WriteLine("  [2]  Manage Suppliers");
            Console.WriteLine("  [3]  Manage Products");
            Console.WriteLine("  [4]  Stock Operations (Restock/Deduct)");
            Console.WriteLine("  [5]  View Low Stock Items");
            Console.WriteLine("  [6]  View Transaction History");
            Console.WriteLine("  [7]  Inventory Valuation Report");
            Console.WriteLine("  [0]  Exit");
            Console.WriteLine();
            Console.Write("Enter choice: ");
        }

        private void ProcessMenuChoice(string choice)
        {
            switch (choice)
            {
                case "1": ManageCategories(); break;
                case "2": ManageSuppliers(); break;
                case "3": ManageProducts(); break;
                case "4": StockOperations(); break;
                case "5": ViewLowStock(); break;
                case "6": ViewTransactions(); break;
                case "7": ViewInventoryValue(); break;
                case "0":
                    Console.WriteLine();
                    Console.WriteLine("Thank you for using Inventory Management System!");
                    running = false;
                    break;
                default:
                    throw new ArgumentException("Invalid menu choice");
            }
        }

        private void ManageCategories()
        {
            Console.Clear();
            Console.WriteLine("CATEGORY MANAGEMENT");
            Console.WriteLine();
            Console.WriteLine("1. Add Category");
            Console.WriteLine("2. View All Categories");
            Console.WriteLine();
            Console.Write("Choice: ");

            string subChoice = Console.ReadLine();

            if (subChoice == "1")
            {
                Console.Write("Category Name: ");
                string name = Console.ReadLine();
                Console.Write("Description: ");
                string desc = Console.ReadLine();

                inventory.AddCategory(name, desc);
            }
            else if (subChoice == "2")
            {
                var categories = inventory.GetAllCategories();
                if (categories.Count == 0)
                {
                    Console.WriteLine("No categories found.");
                    return;
                }

                Console.WriteLine();
                Console.WriteLine("Categories:");
                foreach (var cat in categories)
                    Console.WriteLine(cat);
            }
        }

        private void ManageSuppliers()
        {
            Console.Clear();
            Console.WriteLine("SUPPLIER MANAGEMENT");
            Console.WriteLine();
            Console.WriteLine("1. Add Supplier");
            Console.WriteLine("2. View All Suppliers");
            Console.WriteLine();
            Console.Write("Choice: ");

            string subChoice = Console.ReadLine();

            if (subChoice == "1")
            {
                Console.Write("Supplier Name: ");
                string name = Console.ReadLine();
                Console.Write("Contact Person: ");
                string contact = Console.ReadLine();
                Console.Write("Phone: ");
                string phone = Console.ReadLine();
                Console.Write("Email: ");
                string email = Console.ReadLine();
                Console.Write("Address: ");
                string address = Console.ReadLine();

                inventory.AddSupplier(name, contact, phone, email, address);
            }
            else if (subChoice == "2")
            {
                var suppliers = inventory.GetAllSuppliers();
                if (suppliers.Count == 0)
                {
                    Console.WriteLine("No suppliers found.");
                    return;
                }

                Console.WriteLine();
                Console.WriteLine("Suppliers:");
                foreach (var sup in suppliers)
                    Console.WriteLine(sup);
            }
        }

        private void ManageProducts()
        {
            Console.Clear();
            Console.WriteLine("PRODUCT MANAGEMENT");
            Console.WriteLine();
            Console.WriteLine("1. Add Product");
            Console.WriteLine("2. View All Products");
            Console.WriteLine("3. Search Products");
            Console.WriteLine("4. Update Product");
            Console.WriteLine("5. Delete Product");
            Console.WriteLine();
            Console.Write("Choice: ");

            string subChoice = Console.ReadLine();

            switch (subChoice)
            {
                case "1":
                    AddProductFlow();
                    break;
                case "2":
                    ViewAllProducts();
                    break;
                case "3":
                    SearchProductsFlow();
                    break;
                case "4":
                    UpdateProductFlow();
                    break;
                case "5":
                    DeleteProductFlow();
                    break;
            }
        }

        private void AddProductFlow()
        {
            if (inventory.GetAllCategories().Count == 0)
            {
                Console.WriteLine("Error: Please add at least one category first.");
                return;
            }
            if (inventory.GetAllSuppliers().Count == 0)
            {
                Console.WriteLine("Error: Please add at least one supplier first.");
                return;
            }

            Console.WriteLine();
            Console.WriteLine("Available Categories:");
            foreach (var cat in inventory.GetAllCategories())
                Console.WriteLine("  {0}. {1}", cat.Id, cat.Name);

            Console.WriteLine();
            Console.WriteLine("Available Suppliers:");
            foreach (var sup in inventory.GetAllSuppliers())
                Console.WriteLine("  {0}. {1}", sup.Id, sup.Name);

            Console.WriteLine();
            Console.Write("Product Name: ");
            string name = Console.ReadLine();
            Console.Write("Description: ");
            string desc = Console.ReadLine();
            Console.Write("Price: ");
            if (!decimal.TryParse(Console.ReadLine(), out decimal price))
                throw new ArgumentException("Invalid price format");
            Console.Write("Initial Quantity: ");
            if (!int.TryParse(Console.ReadLine(), out int qty))
                throw new ArgumentException("Invalid quantity format");
            Console.Write("Minimum Stock Level: ");
            if (!int.TryParse(Console.ReadLine(), out int minStock))
                throw new ArgumentException("Invalid minimum stock format");
            Console.Write("Category ID: ");
            if (!int.TryParse(Console.ReadLine(), out int catId))
                throw new ArgumentException("Invalid category ID");
            Console.Write("Supplier ID: ");
            if (!int.TryParse(Console.ReadLine(), out int supId))
                throw new ArgumentException("Invalid supplier ID");

            inventory.AddProduct(name, desc, price, qty, minStock, catId, supId);
        }

        private void ViewAllProducts()
        {
            var products = inventory.GetAllProducts();
            if (products.Count == 0)
            {
                Console.WriteLine("No products found.");
                return;
            }

            Console.WriteLine();
            Console.WriteLine("ALL PRODUCTS");
            Console.WriteLine();
            foreach (var prod in products)
            {
                Console.WriteLine(prod);
                Console.WriteLine(new string('-', 60));
            }
        }

        private void SearchProductsFlow()
        {
            Console.Write("Enter search term (name/description/category/supplier): ");
            string term = Console.ReadLine();

            var results = inventory.SearchProducts(term);
            if (results.Count == 0)
            {
                Console.WriteLine("No products found matching your search.");
                return;
            }

            Console.WriteLine();
            Console.WriteLine("SEARCH RESULTS ({0} found)", results.Count);
            Console.WriteLine();
            foreach (var prod in results)
            {
                Console.WriteLine(prod);
                Console.WriteLine(new string('-', 60));
            }
        }

        private void UpdateProductFlow()
        {
            Console.Write("Enter Product ID to update: ");
            if (!int.TryParse(Console.ReadLine(), out int id))
                throw new ArgumentException("Invalid ID format");

            var product = inventory.GetProductById(id);
            if (product == null)
            {
                Console.WriteLine("Product not found.");
                return;
            }

            Console.WriteLine();
            Console.WriteLine("Selected: {0}", product.Name);
            Console.WriteLine("Leave blank to keep current value");
            Console.WriteLine();

            Console.Write("New Name [{0}]: ", product.Name);
            string name = Console.ReadLine();
            Console.Write("New Description [{0}]: ", product.Description);
            string desc = Console.ReadLine();
            Console.Write("New Price [{0}]: ", product.Price);
            string priceStr = Console.ReadLine();
            Console.Write("New Min Stock Level [{0}]: ", product.MinStockLevel);
            string minStockStr = Console.ReadLine();

            decimal? price = null;
            if (!string.IsNullOrWhiteSpace(priceStr))
                price = decimal.Parse(priceStr);

            int? minStock = null;
            if (!string.IsNullOrWhiteSpace(minStockStr))
                minStock = int.Parse(minStockStr);

            inventory.UpdateProduct(id,
                string.IsNullOrWhiteSpace(name) ? null : name,
                string.IsNullOrWhiteSpace(desc) ? null : desc,
                price, minStock, null, null);
        }

        private void DeleteProductFlow()
        {
            Console.Write("Enter Product ID to delete: ");
            if (!int.TryParse(Console.ReadLine(), out int id))
                throw new ArgumentException("Invalid ID format");

            var product = inventory.GetProductById(id);
            if (product == null)
            {
                Console.WriteLine("Product not found.");
                return;
            }

            Console.Write("Are you sure you want to delete '{0}'? (yes/no): ", product.Name);
            string confirm = Console.ReadLine();
            if (confirm != null && confirm.ToLower() == "yes")
            {
                inventory.DeleteProduct(id);
            }
            else
            {
                Console.WriteLine("Deletion cancelled.");
            }
        }

        private void StockOperations()
        {
            Console.Clear();
            Console.WriteLine("STOCK OPERATIONS");
            Console.WriteLine();
            Console.WriteLine("1. Restock (Add Inventory)");
            Console.WriteLine("2. Deduct Stock (Sale/Usage)");
            Console.WriteLine();
            Console.Write("Choice: ");

            string choice = Console.ReadLine();

            Console.Write("Product ID: ");
            if (!int.TryParse(Console.ReadLine(), out int id))
                throw new ArgumentException("Invalid ID format");

            Console.Write("Quantity: ");
            if (!int.TryParse(Console.ReadLine(), out int qty))
                throw new ArgumentException("Invalid quantity");

            Console.Write("Notes (optional): ");
            string notes = Console.ReadLine();

            if (choice == "1")
                inventory.RestockProduct(id, qty, notes);
            else if (choice == "2")
                inventory.DeductStock(id, qty, notes);
        }

        private void ViewLowStock()
        {
            Console.Clear();
            Console.WriteLine("LOW STOCK ALERTS");
            Console.WriteLine();

            var lowStock = inventory.GetLowStockItems();
            if (lowStock.Count == 0)
            {
                Console.WriteLine("No low stock items. All inventory levels are healthy!");
                return;
            }

            Console.WriteLine("WARNING: {0} items need restocking!", lowStock.Count);
            Console.WriteLine();

            foreach (var prod in lowStock)
            {
                Console.WriteLine("[{0}] {1}", prod.Id, prod.Name);
                Console.WriteLine("    Current: {0} | Minimum Required: {1}", prod.Quantity, prod.MinStockLevel);
                Console.WriteLine("    Shortage: {0} units", prod.MinStockLevel - prod.Quantity);
                Console.WriteLine(new string('-', 50));
            }
        }

        private void ViewTransactions()
        {
            Console.Clear();
            Console.WriteLine("TRANSACTION HISTORY");
            Console.WriteLine();
            Console.WriteLine("1. View All Transactions");
            Console.WriteLine("2. View by Product ID");
            Console.WriteLine();
            Console.Write("Choice: ");

            string choice = Console.ReadLine();
            List<TransactionRecord> transactions;

            if (choice == "2")
            {
                Console.Write("Enter Product ID: ");
                if (!int.TryParse(Console.ReadLine(), out int pid))
                    throw new ArgumentException("Invalid ID");
                transactions = inventory.GetTransactionHistory(pid);
            }
            else
            {
                transactions = inventory.GetTransactionHistory(null);
            }

            if (transactions.Count == 0)
            {
                Console.WriteLine("No transactions found.");
                return;
            }

            Console.WriteLine();
            Console.WriteLine("{0} Transactions", transactions.Count);
            Console.WriteLine();
            foreach (var trans in transactions.OrderByDescending(t => t.Id))
            {
                Console.WriteLine(trans);
                Console.WriteLine();
            }
        }

        private void ViewInventoryValue()
        {
            Console.Clear();
            Console.WriteLine("INVENTORY VALUATION REPORT");
            Console.WriteLine();

            var products = inventory.GetAllProducts();
            if (products.Count == 0)
            {
                Console.WriteLine("No products in inventory.");
                return;
            }

            Console.WriteLine("{0,-5} {1,-20} {2,-8} {3,-12} {4,-12}", "ID", "Name", "Qty", "Unit Price", "Total Value");
            Console.WriteLine(new string('=', 65));

            foreach (var prod in products.OrderByDescending(p => p.GetInventoryValue()))
            {
                Console.WriteLine("{0,-5} {1,-20} {2,-8} {3,-12:C2} {4,-12:C2}",
                    prod.Id, prod.Name, prod.Quantity, prod.Price, prod.GetInventoryValue());
            }

            Console.WriteLine(new string('=', 65));
            Console.WriteLine("{0,-45} {1:C2}", "TOTAL INVENTORY VALUE:", inventory.GetTotalInventoryValue());
        }

        private void ShowError(string message)
        {
            Console.WriteLine();
            Console.WriteLine("ERROR: {0}", message);
        }
    }

    

    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                ConsoleUI ui = new ConsoleUI();
                ui.Run();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Fatal error: {0}", ex.Message);
            }
        }
    }
}
