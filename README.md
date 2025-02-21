# Migrations command

1. Passenger Service
dotnet ef migrations add AddTablePassenger --output-dir Persistence\Migrations --project .\Passenger.Infrastructure --startup-project .\Passenger.API
dotnet ef database update --project .\Passenger.Infrastructure --startup-project .\Passenger.API

2. Admin Service
dotnet ef migrations add AddAdminPassenger --output-dir Persistence\Migrations --project .\Admin.Infrastructure --startup-project .\Admin.API
dotnet ef database update --project .\Admin.Infrastructure --startup-project .\Admin.API

3. Booking service
dotnet ef migrations add AddTicketTbl --output-dir Persistence\Migrations --project .\Booking.Infrastructure --startup-project .\Booking.API
dotnet ef database update --project .\Booking.Infrastructure --startup-project .\Booking.API