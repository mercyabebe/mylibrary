 # MyLibrary Desktop Application - Functional Overview and Description

## Overview

MyLibrary is a Windows desktop application developed in C# with WinForms that enables small libraries to manage their book inventory and member borrowing records efficiently. The application connects to a SQLite database using ADO.NET and offers a user-friendly interface for managing books, borrowers, and book issuance/returns.

---

## Functionalities

### 1. Login Form
- **Purpose:** Authenticate users before accessing the system.
- **Features:** 
  - Input fields for username and password.
  - Authentication against the `Users` table in the database.
  - Success leads to the Main Window; failure shows an error message.

### 2. Main Window
- **UI:** Tabbed interface with two tabs:
  - **Books Management**
  - **Borrowers Management**

---

### 3. Books Management
- **Display:** A `DataGridView` listing all books with columns: BookID, Title, Author, Year, AvailableCopies.
- **Operations:**
  - **Add Book:** Opens a form to input new book details.
  - **Edit Book:** Opens a form with selected book data for editing.
  - **Delete Book:** Deletes the selected book after confirmation.
  - **Filtering:** Filter books by author name or year range.
- **Validation:**
  - Title and Author cannot be empty.
  - Year must be between 1500 and the current year.
  - AvailableCopies must be a non-negative integer.

---

### 4. Borrowers Management
- **Display:** A `DataGridView` listing all borrowers with columns: BorrowerID, Name, Email, Phone.
- **Operations:**
  - **Add Borrower:** Opens a form to enter new borrower details.
  - **Edit Borrower:** Opens a form to edit selected borrower.
  - **Delete Borrower:** Deletes selected borrower after confirmation.
  - **Issue Book:**
    - Select borrower and book.
    - Checks if book copies are available.
    - Inserts a record into `IssuedBooks`.
    - Decrements `AvailableCopies` of the book.
  - **Return Book:**
    - Opens a form listing all issued books not yet returned.
    - Allows marking a book as returned.
    - Increments `AvailableCopies` accordingly.
- **Validation:**
  - Name, Email, and Phone cannot be empty.
  - Email is validated with a regex for correct format.

---

### 5. Reports and Filtering 
- Filter books by author or year range in the Books Management tab.
- A report for overdue books (where DueDate is before the current date) can be added.

---

## Technical Implementation Details

- **Language & Framework:** C# with WinForms for UI.
- **Database:** SQLite (`MyLibrary.db`) for simplicity.
- **Data Access:** ADO.NET with parameterized queries for security.
- **Event Handling:** 
  - Button clicks trigger CRUD operations and form navigation.
  - Selection events allow editing/deleting records.
- **Exception Handling:**
  - Database operations wrapped in try-catch blocks.
  - User-friendly error messages displayed on exceptions.
- **Input Validation:**
  - Checks for empty fields.
  - Numeric values checked for range validity.
  - Email format validated using regular expressions.

---

## Summary

MyLibrary integrates UI design, event-driven programming, database connectivity, and validation logic to provide a complete library management solution suitable for small-scale use. It demonstrates core concepts of C# desktop development and database CRUD operations effectively.

---

## Usage Notes

- The initial user is 'admin' with password 'admin123'.
- The database file `MyLibrary.db` must be present alongside the executable.
- The app uses SQLite for lightweight, file-based storage without requiring a server.

---

Thank you for exploring MyLibrary!
