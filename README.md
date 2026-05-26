<div align="center">
  <h1>Barangay Pharma System</h1>
</div>

<div align="center">
  <h3>Ensuring Good Health and Well-Being (SDG 3) through Digital Pharmaceutical Care</h3>
  <p>A modern, secure, and production-ready Web-Based Pharmacy Management System designed to manage patient records, medicine inventory, prescriptions, stock dispensing, and patient refill requests.</p>

  <br/>

![C#](https://img.shields.io/badge/c%23-%23239120.svg?style=flat&logo=c-sharp&logoColor=white)
![.NET 8](https://img.shields.io/badge/.net%208-%23512BD4.svg?style=flat&logo=.net&logoColor=white)
![HTML5](https://img.shields.io/badge/html5-%23E34F26.svg?style=flat&logo=html5&logoColor=white)
![CSS3](https://img.shields.io/badge/css3-%231572B6.svg?style=flat&logo=css3&logoColor=white)
![JavaScript](https://img.shields.io/badge/javascript-%23323330.svg?style=flat&logo=javascript&logoColor=%23F7DF1E)
![SQL Server](https://img.shields.io/badge/sql%20server-%23CC292B.svg?style=flat&logo=microsoft-sql-server&logoColor=white)

</div>

---

## 📋 Overview

The **Barangay Pharma System** is a robust web application built for local barangay health centers and Rural Health Units (RHUs). It serves as a centralized platform for healthcare staff to maintain patient records, manage pharmaceutical inventory, issue prescriptions, and dispense medicine. It also provides patients with a self-service portal to review active prescriptions, track dispensing history, and submit refill requests securely.

---

## 🌍 Sustainable Development Goal 3 (SDG 3) Alignment

The Barangay Pharma System directly supports UN Sustainable Development Goal 3: Good Health and Well-being. By digitizing medicine inventory, prescription management, and refill workflows for barangay health centers and rural communities, the system eliminates the administrative barriers that delay access to essential medicines. Health workers can monitor stock levels in real time, receive alerts before medicines expire, and maintain accurate dispensing records ensuring that no patient is turned away due to stockouts or outdated supplies. Patients gain a self-service portal to track their own prescriptions and request refills without repeated clinic visits. Through these capabilities, the Barangay Pharma System strengthens community health infrastructure, promotes timely access to life-saving maintenance medications, and supports a healthier, more resilient barangay population.

---

## 👥 Submitted By:

- **Allen P. Del Valle**
- **Michael Justin B. Surbnano**
- **Asthan Eilexer J. Patanao**

---

## 🔄 User Flows

### 🛡️ Admin Flow
1. **Login:** Admin logs in using the seeded credentials `admin@bps.com`.
2. **Dashboard Overview:** Views aggregate KPI counts (Total Patients, Medicines, Prescriptions, Dispensing Records), pending/resolved stock alerts, and recent audit activity.
3. **User & Account Management:** Navigates to **User Management** to create accounts for Staff or Patients.
   - *Automated Profile Generation:* Assigning the "Patient" role prompts the admin for clinical demographic details and automatically generates a unique `PatientCode` (`PAT-YYYY-NNNNN`).
4. **Inventory Setup:** Navigates to **Medicines** to register new medicines with custom categories, minimum stock thresholds, expiry dates, dosage details, and optional images, or manages the **Supplier Directory**.
5. **System Oversight:** Analyzes trends in the **Reports** portal or tracks security events in the read-only **Audit Logs**.

### 🩺 Staff Flow
1. **Login:** Staff logs in using `staff@bps.com`.
2. **Dashboard Overview:** Monitors today's dispensing statistics, pending refills, and active low-stock or expired medicine alerts.
3. **Patient Registry:** Navigates to **Patients** to create new patient profiles. A generated `PatientCode` is issued, which allows patients to securely self-register their accounts online.
4. **Clinical Prescribing:** Locates a patient, views their history, and creates a new prescription. The system restricts medicine selection to non-deleted and non-expired stock.
5. **Dispensing Workflow:** Selects a prescription and dispenses the drug.
   - Deducts stock, records the transaction, and updates alerts if inventory levels dip below minimum thresholds.
   - Prevents dispensing operations if the selected medicine is expired.
6. **Refill Review:** Evaluates pending patient refill requests, with the ability to **Approve** (automatically deducts stock and registers a dispensing record) or **Reject** (providing rejection reasons).

### 👤 Patient Flow
1. **Self-Registration (Optional):** Patient self-registers at `/Account/Register` using their staff-issued `PatientCode` to link their online credentials to their clinical file.
2. **Login:** Patient logs in using `patient@bps.com`.
3. **Personal Dashboard:** Greets the patient with their profile photo, showing active prescription details, total dispensing history, and pending refills.
4. **Profile Photo Management:** Updates their profile avatar under **My Profile** (validated for format and a 2MB file limit, automatically clearing previous images).
5. **Medication Tracking:** Reviews all prescriptions and filters them by state (Active, Completed, Expired, Refilled).
6. **Refill Requesting:** Requests a refill for an active prescription.
   - *Validation Rules:* The system blocks submission if the 20-day cooldown period since the last dispensing has not passed, or if the medicine is expired.
7. **Dispensing Logs:** Reviews a read-only, date-filtered list of all past collections.

---

## 🔑 Default Login Credentials

On first run, the database is seeded automatically with three roles and default user accounts:

| Role | Email Address | Password | Extra Info |
| :--- | :--- | :--- | :--- |
| **Admin** | `admin@bps.com` | `Admin@1234` | Full access (Dashboard, Users, Medicines, Suppliers, Audit Logs, Reports) |
| **Staff** | `staff@bps.com` | `Staff@1234` | Clinical operations (Dashboard, Patients, Prescriptions, Dispensing, Refills) |
| **Patient** | `patient@bps.com` | `Patient@1234` | Self-Service (Dashboard, Profile Photo upload, Prescriptions, Dispensing History, Refills) |

---

## 🚀 Setup & Installation Instructions

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Microsoft SQL Server](https://www.microsoft.com/en-us/sql-server/) (Express or LocalDB)
- Git

### Local Setup

1. **Clone the Repository:**
   ```bash
   git clone https://github.com/tsugumii21/Final-Project-in-IT-Elective-2-Barangay-Pharma-System-.git
   cd Final-Project-in-IT-Elective-2-Barangay-Pharma-System-
   ```

2. **Configure Connection String:**
   Open `BarangayPharmaSystem/appsettings.json` and verify the `DefaultConnection` string points to your local SQL Server instance:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=BarangayPharmaDB;Trusted_Connection=True;TrustServerCertificate=True"
   }
   ```

3. **Apply Database Migrations:**
   Ensure SQL Server is running, then run EF migrations to create the database:
   ```bash
   dotnet ef database update
   ```

4. **Run the Application:**
   ```bash
   dotnet run
   ```
   Open your browser and navigate to the local address displayed in the console (typically `http://localhost:5269` or `https://localhost:5001`).

---

## 📦 Features Implemented

### 🛡️ Admin Dashboard
- **User Management**: Create, edit, and soft-delete user accounts (Admin, Staff, Patient). Automatically links to a clinical Patient profile when the `Patient` role is assigned.
- **Medicine Inventory**: CRUD operations for medicines, including photo uploads, stock level thresholds, and expiration date tracking.
- **Supplier Directory**: CRUD operations for medicine suppliers/vendors.
- **Audit Logging**: Read-only log tracing all create, update, and delete actions with timestamps, IP addresses, and user details.
- **Reports & Analytics**: Visual charts showing top dispensed medications, patient activity rankings, low-stock overviews, and date-filtered dispensing metrics.

### 🩺 Staff Dashboard
- **Patient Management**: Manage patient profiles, view detailed histories (prescriptions, dispensing dates, refills), and generate unique patient IDs (`PAT-YYYY-NNNNN`).
- **Prescription Creation**: Create and edit prescriptions for patients. Integrates checks to prevent selecting expired or deleted medicines.
- **Dispensing Records**: Deduct stock and record dispensing actions against prescriptions. Includes double-submission protection and expired medicine blocks.
- **Refill Request Review**: Approve or reject patient-submitted refill requests. Approvals automatically trigger stock deduction and log audit trails.
- **Automated Stock Alerts**: Real-time background checking that raises alerts for Low Stock, Out of Stock, or Near Expiry conditions.

### 👤 Patient Dashboard
- **Self-Service Dashboard**: Personalized greeting banner, profile photo display, and KPI summary counters.
- **Secure Photo Upload**: Patients can upload and update their profile photos (validated for `.jpg`/`.png` and maximum size of 2MB). Overwritten photo files are cleaned from the server disk.
- **My Prescriptions & Refill Cooldown**: Lists all prescriptions and their statuses. Restricts refill requests with a strict **20-day server-side cooldown** since the last dispensing date and blocks requests for expired medicines.
- **My Dispensing History**: Read-only date-filtered log of all medicine collections.

---
