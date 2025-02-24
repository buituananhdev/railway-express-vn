# Migrations command

1. User Management Service
dotnet ef migrations add AddTablePassenger --output-dir Persistence\Migrations --project .\src\Services\UserManagement\UserManagement.Infrastructure --startup-project .\src\Services\UserManagement\UserManagement.API
dotnet ef migrations remove --project .\src\Services\UserManagement\UserManagement.Infrastructure --startup-project .\src\Services\UserManagement\UserManagement.API

1. Admin Service
dotnet ef migrations add AddAdminPassenger --output-dir Persistence\Migrations .\src\Services\Admin\Admin.Infrastructure --startup-project .\src\Services\Admin\Admin.API
dotnet ef database update --project .\src\Services\Admin\Admin.Infrastructure --startup-project .\src\Services\Admin\Admin.API

1. Booking service
dotnet ef migrations add AddTicketTbl --output-dir Persistence\Migrations .\src\Services\Booking\Booking.Infrastructure --startup-project .\src\Services\Booking\Booking.API
dotnet ef database update .\src\Services\Booking\Booking.Infrastructure --startup-project .\src\Services\Booking\Booking.API

# Update database
dotnet ef database update --project .\src\Services\UserManagement\UserManagement.Infrastructure --startup-project .\src\Services\UserManagement\UserManagement.API
dotnet ef database update --project .\src\Services\Admin\Admin.Infrastructure --startup-project .\src\Services\Admin\Admin.API
dotnet ef database update --project .\src\Services\Booking\Booking.Infrastructure --startup-project .\src\Services\Booking\Booking.API
