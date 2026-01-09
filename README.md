# 🔧 Service Device Manager (SDM)

A comprehensive ASP.NET Core MVC application for managing device repairs, tracking technician performance, and monitoring service level agreements (SLAs).

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)
![C#](https://img.shields.io/badge/C%23-12-239120?logo=c-sharp)
![Entity Framework](https://img.shields.io/badge/Entity%20Framework-Core-512BD4)
![Bootstrap](https://img.shields.io/badge/Bootstrap-5.3-7952B3?logo=bootstrap)

---

## 📋 Table of Contents

- [Features](#features)
- [Tech Stack](#tech-stack)
- [Prerequisites](#prerequisites)
- [Installation](#installation)
- [Configuration](#configuration)
- [Database Setup](#database-setup)
- [Usage](#usage)
- [User Roles](#user-roles)
- [Screenshots](#screenshots)
- [Project Structure](#project-structure)
- [Contributing](#contributing)
- [License](#license)
- [Contact](#contact)

---

## ✨ Features

### Core Functionality
- **Device Management** - Create, edit, view, and track devices through their repair lifecycle
- **Technician Assignment** - Assign devices to technicians and track workload distribution
- **Work Status Tracking** - Monitor devices from assignment through completion and approval
- **Diagnosis System** - Record detailed diagnoses with condition descriptions and recommendations
- **Manager Approval Workflow** - Review and approve completed work before marking devices as done

### Advanced Features
- **Priority Levels** - Categorize devices as Low, Medium, High, or Critical
- **Due Dates & SLA Tracking** - Set deadlines and automatically track SLA compliance
  - Real-time status: On Time, At Risk, Overdue
  - Completion tracking: Met SLA, Missed SLA
  - Visual indicators with color-coded badges
  
- **Notifications System** - Keep everyone informed with real-time updates
  - In-app notifications with bell icon and unread count
  - Email notifications for all major events
  - Notifications for: assignments, completions, approvals, overdue devices
  
- **Device History Timeline** - Complete audit trail of all device events
  - Visual timeline with color-coded icons
  - Tracks: creation, assignments, status changes, diagnoses, approvals
  - Shows who performed each action and when
  
- **Technician Performance Reports** - Comprehensive analytics dashboard
  - Total devices completed per technician
  - SLA compliance rates
  - Average completion time
  - Current workload and priority distribution
  - Leaderboard with ranking system
  - Detailed individual performance breakdowns

### Additional Capabilities
- **Role-Based Access Control** - Four user roles with specific permissions
- **Search & Filtering** - Filter devices by type, status, priority, SLA status
- **Audit Logging** - Complete audit trail of all system actions
- **Admin Override** - Emergency admin access for critical situations
- **Responsive Design** - Works seamlessly on desktop, tablet, and mobile

---

## 🛠 Tech Stack

### Backend
- **Framework**: ASP.NET Core 8.0 MVC
- **Language**: C# 12
- **ORM**: Entity Framework Core
- **Authentication**: ASP.NET Core Identity
- **Email**: MailKit

### Frontend
- **UI Framework**: Bootstrap 5.3
- **Icons**: Bootstrap Icons
- **JavaScript**: Vanilla JS

### Database
- **DBMS**: SQL Server (can be configured for other databases)

### Additional Libraries
- DocumentFormat.OpenXml (for Excel exports)
- Custom services for audit logging, notifications, and history tracking

---

## 📦 Prerequisites

Before you begin, ensure you have the following installed:

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) (or SQL Server Express)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) or [VS Code](https://code.visualstudio.com/)
- [Git](https://git-scm.com/)

---

## 🚀 Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/hidytech-a11y/device-manager.git
   cd device-manager
   ```

2. **Restore NuGet packages**
   ```bash
   dotnet restore
   ```

3. **Install Entity Framework Core tools** (if not already installed)
   ```bash
   dotnet tool install --global dotnet-ef
   ```

4. **Install required packages**
   ```bash
   dotnet add package MailKit
   dotnet add package DocumentFormat.OpenXml
   ```

---

## ⚙️ Configuration

### 1. Update Connection String

Edit `appsettings.json` and update the connection string:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=DeviceManagerDB;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

### 2. Configure Email Settings

Add your email configuration to `appsettings.json`:

```json
{
  "Email": {
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": "587",
    "Username": "your-email@gmail.com",
    "Password": "your-app-password",
    "FromEmail": "noreply@devicemanager.com"
  }
}
```

**For Gmail users:**
1. Enable 2-Step Verification in your Google Account
2. Generate an App Password: [Google Account → Security → App Passwords](https://myaccount.google.com/apppasswords)
3. Use the 16-character app password in the configuration

---

## 🗄️ Database Setup

1. **Create the database**
   ```bash
   dotnet ef database update
   ```

2. **Seed initial data** (Optional)
   
   The application will automatically create default roles on first run:
   - Admin
   - Manager
   - Technician
   - Viewer

3. **Create the first admin user**
   
   After running the application, register a user and manually assign the Admin role via SQL:
   ```sql
   -- Find the user ID
   SELECT Id, Email FROM AspNetUsers WHERE Email = 'admin@example.com';
   
   -- Find the Admin role ID
   SELECT Id, Name FROM AspNetRoles WHERE Name = 'Admin';
   
   -- Assign the role
   INSERT INTO AspNetUserRoles (UserId, RoleId) 
   VALUES ('user-id-here', 'admin-role-id-here');
   ```

---

## 💻 Usage

### Running the Application

```bash
dotnet run
```

The application will be available at:
- HTTPS: `https://localhost:44351`
- HTTP: `http://localhost:5000`

### Default Workflow

1. **Login** with your admin account
2. **Create Technicians** via the Technicians menu
3. **Create Device Types** (if needed)
4. **Create Devices** and assign them to technicians
5. **Technicians** log in and view their tasks
6. **Technicians** add diagnoses and mark devices as done
7. **Managers** review and approve completed work
8. **View Reports** to analyze technician performance

---

## 👥 User Roles

### Admin
- Full system access
- User management
- Device CRUD operations
- Emergency override capabilities
- View all reports and analytics

### Manager
- View all devices and technicians
- Approve completed work
- Assign/reassign devices
- View performance reports
- No user management access

### Technician
- View assigned devices only
- Update work status
- Add/edit diagnoses
- Mark devices as complete
- View personal task list

### Viewer
- Read-only access
- View devices and technicians
- View reports
- No modification capabilities

---

## 📸 Screenshots

### Dashboard
![Dashboard](screenshots/dashboard.png)
*Main overview showing device statistics and quick actions*

### Device List
![Device List](screenshots/devices.png)
*Comprehensive device management with filters and search*

### My Tasks (Technician View)
![My Tasks](screenshots/mytasks.png)
*Technician's personal task list with diagnosis forms*

### Device Details & Timeline
![Device Details](screenshots/details.png)
*Complete device information with visual history timeline*

### Performance Reports
![Performance Reports](screenshots/reports.png)
*Technician performance analytics and leaderboard*

### Notifications
![Notifications](screenshots/notifications.png)
*Real-time notification system*

---

## 📁 Project Structure

```
DeviceManager/
├── Controllers/
│   ├── AccountController.cs
│   ├── AdminDashboardController.cs
│   ├── ApprovalsController.cs
│   ├── DevicesController.cs
│   ├── NotificationsController.cs
│   ├── ReportsController.cs
│   ├── TechniciansController.cs
│   └── UsersController.cs
├── Data/
│   └── DeviceContext.cs
├── Models/
│   ├── Device.cs
│   ├── DeviceHistory.cs
│   ├── DeviceType.cs
│   ├── Diagnosis.cs
│   ├── Notification.cs
│   ├── Technician.cs
│   └── ViewModels/
├── Services/
│   ├── IEmailService.cs
│   ├── EmailService.cs
│   ├── INotificationService.cs
│   ├── NotificationService.cs
│   ├── IDeviceHistoryService.cs
│   ├── DeviceHistoryService.cs
│   ├── IAuditService.cs
│   └── AuditService.cs
├── Views/
│   ├── Account/
│   ├── Devices/
│   ├── Notifications/
│   ├── Reports/
│   ├── Technicians/
│   ├── Shared/
│   │   └── _Layout.cshtml
│   └── Home/
├── wwwroot/
│   ├── css/
│   ├── js/
│   └── images/
├── Migrations/
├── appsettings.json
├── Program.cs
└── README.md
```

---

## 🤝 Contributing

Contributions are welcome! Please follow these steps:

1. **Fork the repository**
2. **Create a feature branch**
   ```bash
   git checkout -b feature/AmazingFeature
   ```
3. **Commit your changes**
   ```bash
   git commit -m 'Add some AmazingFeature'
   ```
4. **Push to the branch**
   ```bash
   git push origin feature/AmazingFeature
   ```
5. **Open a Pull Request**

### Development Guidelines
- Follow C# coding conventions
- Write meaningful commit messages
- Add comments for complex logic
- Update documentation for new features
- Test thoroughly before submitting PR

---

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

## 📧 Contact

**Idris** - Developer

- GitHub: [@yourusername](https://github.com/yourusername)
- Email: your.email@example.com

**Project Link**: [https://github.com/yourusername/device-manager](https://github.com/yourusername/device-manager)

---

## 🙏 Acknowledgments

- Built with [ASP.NET Core](https://dotnet.microsoft.com/apps/aspnet)
- UI powered by [Bootstrap](https://getbootstrap.com/)
- Icons from [Bootstrap Icons](https://icons.getbootstrap.com/)
- Email functionality via [MailKit](https://github.com/jstedfast/MailKit)

---

## 🎯 Roadmap

Future enhancements planned:
- [ ] Mobile app (iOS/Android)
- [ ] Real-time chat between technicians and managers
- [ ] File attachments for devices (photos, documents)
- [ ] Advanced analytics with charts and graphs
- [ ] Export reports to PDF/Excel
- [ ] Multi-language support
- [ ] Dark mode
- [ ] API for third-party integrations

---

## 🐛 Known Issues

- None at the moment. Please report any issues you encounter!

---

## 💡 Tips

- For best performance, ensure SQL Server is properly indexed
- Email notifications require valid SMTP credentials
- Use the Admin Override feature responsibly
- Regular database backups are recommended
- Monitor the audit logs for security tracking

---

<div align="center">
  <p>Made with ❤️ by Idris</p>
  <p>© 2026 Device Manager. All rights reserved.</p>
</div>
