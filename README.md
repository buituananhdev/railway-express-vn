# Migrations command

1. User Management Service
dotnet ef migrations add AddTablePassenger --output-dir Persistence\Migrations --project .\UserManagement.Infrastructure --startup-project .\UserManagement.API
dotnet ef database update --project .\UserManagement.Infrastructure --startup-project .\UserManagement.API

1. Admin Service
dotnet ef migrations add AddAdminPassenger --output-dir Persistence\Migrations --project .\Admin.Infrastructure --startup-project .\Admin.API
dotnet ef database update --project .\Admin.Infrastructure --startup-project .\Admin.API

1. Booking service
dotnet ef migrations add AddTicketTbl --output-dir Persistence\Migrations --project .\Booking.Infrastructure --startup-project .\Booking.API
dotnet ef database update --project .\Booking.Infrastructure --startup-project .\Booking.API