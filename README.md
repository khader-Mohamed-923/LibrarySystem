# 📚 Library Management System

A professional Library Management System Web API built with **.NET 10** and **Clean Architecture**. This project is designed with a strict focus on backend best practices, SOLID principles, and comprehensive automated testing (Unit, Integration, and Architecture Testing).

---

## 🗺️ Database Schema (ERD)

Below is the entity-relationship diagram representing the core domain of the library system. 

> *Note: Rendered automatically via GitHub Mermaid.*

```mermaid
erDiagram
    BOOK {
        int Id PK
        string Title
        string Author
        string ISBN
        int TotalCopies
        int AvailableCopies
    }

    MEMBER {
        int Id PK
        string FullName
        string Email
        datetime MembershipExpiryDate
        decimal OutstandingFine
    }

    LOAN {
        int Id PK
        int BookId FK
        int MemberId FK
        datetime BorrowedAt
        datetime DueDate
        datetime ReturnedAt
        decimal FineAmount
    }

    BOOK ||--o{ LOAN : "can have multiple"
    MEMBER ||--o{ LOAN : "creates multiple"
