# 🏥 Barangay Pharma System

A modern, secure, and user-friendly Web-Based Pharmacy Management System tailored for barangay health centers. The system handles patient records, medicine inventory, prescriptions, stock dispensing, and patient-initiated refill requests, complete with real-time audit logging and analytics.

---

## 🌍 Sustainable Development Goal 3 (SDG 3) Alignment

The **Barangay Pharma System** directly supports **UN Sustainable Development Goal 3: Good Health and Well-being**. By automating and streamlining medicine tracking, prescription workflows, and refill requests within **rural barangay communities in Calamba City, Laguna**, this application removes administrative barriers to essential healthcare. It ensures that local health centers can prevent critical medicine stockouts, monitor drug expiration dates to avoid dispensing unsafe medication, and provide patients with an active self-service portal. This digital transformation directly improves health outcomes, ensures timely access to life-saving maintenance medications, and fosters a healthier, more resilient barangay community.

---

## 🛠️ Technology Stack

- **Core Framework**: ASP.NET Core MVC (.NET 8)
- **Database**: SQL Server 2022/2025 via Entity Framework Core
- **Front-End Styling**: Vanilla CSS + Bootstrap 5 (Medical Blue Theme: `#1A6FA3`)
- **Typography**: *DM Sans* (for headings) & *Nunito* (for body text and labels)
- **UI Enhancement**: FontAwesome Icons, Chart.js (for analytics), and DataTables.js (for interactive tables)

---

## 🚀 Getting Started & Local Setup

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Microsoft SQL Server](https://www.microsoft.com/en-us/sql-server/) (Express or LocalDB)
- Git

### Installation Steps

1. **Clone the Repository:**
   ```bash
   git clone https://github.com/tsugumii21/Final-Project-in-IT-Elective-2-Barangay-Pharma-System-.git
   cd Final-Project-in-IT-Elective-2-Barangay-Pharma-System-
   ```

2. **Configure the Connection String:**
   Open `BarangayPharmaSystem/appsettings.json` and adjust the `DefaultConnection` string under `ConnectionStrings` to point to your local SQL Server instance:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=YOUR_SERVER_NAME;Database=BarangayPharmaDB;Trusted_Connection=True;TrustServerCertificate=True"
   }
   ```
   *(Note: The default connection string is configured for `localhost\SQLEXPRESS`.)*

3. **Apply Database Migrations:**
   Ensure SQL Server is running, then apply the migrations to create the database schema:
   ```bash
   dotnet ef database update
   ```

4. **Run the Application:**
   ```bash
   dotnet run
   ```
   Open your browser and navigate to the local address displayed in your terminal (typically `http://localhost:5269` or `https://localhost:5001`). The database seeder will automatically seed baseline roles, users, medicines, and sample records on startup.

---

## 🔑 Default Login Credentials

On application startup, three roles are created and seeded with default accounts:

| Role | Email Address | Password | Extra Info |
| :--- | :--- | :--- | :--- |
| **Admin** | `admin@bps.com` | `Admin@1234` | Full access (Dashboard, Users, Medicines, Suppliers, Audit Logs, Reports) |
| **Staff** | `staff@bps.com` | `Staff@1234` | Clinical operations (Dashboard, Patients, Prescriptions, Dispensing, Refills) |
| **Patient** | `patient@bps.com` | `Patient@1234` | Self-Service (Dashboard, Profile Photo upload, Prescriptions, Dispensing History, Refill requests) |

---

## 📦 Key Features Implemented

### 🛡️ Admin Module
- **User Management**: Create, edit, and soft-delete user accounts (Admin, Staff, Patient). Automatically links to a clinical Patient profile when the `Patient` role is assigned.
- **Medicine Inventory**: CRUD operations for medicines, including photo uploads, stock level thresholds, and expiration date tracking.
- **Supplier Directory**: CRUD operations for medicine suppliers/vendors.
- **Audit Logging**: Read-only log tracing all create, update, and delete actions with timestamps, IP addresses, and user details.
- **Reports & Analytics**: Visual charts showing top dispensed medications, patient activity rankings, low-stock overviews, and date-filtered dispensing metrics.

### 🩺 Staff Module
- **Patient Management**: Manage patient profiles, view detailed histories (prescriptions, dispensing dates, refills), and generate unique patient IDs (`PAT-YYYY-NNNNN`).
- **Prescription Creation**: Create and edit prescriptions for patients. Integrates checks to prevent selecting expired or deleted medicines.
- **Dispensing Records**: Deduct stock and record dispensing actions against prescriptions. Includes double-submission protection and expired medicine blocks.
- **Refill Request Review**: Approve or reject patient-submitted refill requests. Approvals automatically trigger stock deduction and log audit trails.
- **Automated Stock Alerts**: Real-time background checking that raises alerts for Low Stock, Out of Stock, or Near Expiry conditions.

### 👤 Patient Module
- **Self-Service Dashboard**: Personalized greeting banner, profile photo display, and KPI summary counters.
- **Secure Photo Upload**: Patients can upload and update their profile photos (validated for `.jpg`/`.png` and maximum size of 2MB). Overwritten photo files are cleaned from the server disk.
- **My Prescriptions & Refill Cooldown**: Lists all prescriptions and their statuses. Restricts refill requests with a strict **20-day server-side cooldown** since the last dispensing date and blocks requests for expired medicines.
- **My Dispensing History**: Read-only date-filtered log of all medicine collections.

---

## 👥 Developers
*   **Built for IT Elective 2 Final Project**
*   *Co-developed with Google Antigravity (Agentic AI)*
