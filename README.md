# Heroes-.NET6-GraphQL
.NET 6 Full CRUD with Entity Framework Core and web API.

I have use the following technologies and packages:
- C#.
- .NET 6.
- Entity Framework Core.
- Microsoft SQL Server.
- Serilog.
- RESTful API.
- GraphQL (navigate in localhost/portNumber/ui/playground to test it)

In addition i have implement the following:
- Generic Repository Pattern.
- ASP.NET Core Identity (full membership system)
    - Registration with email confirmation
    - Login
    - Change / Forget / Reset Password
    - Email confirmation
    - 2FA with email
- Pagination.
- Web Api sorting based on property (using dynamic LINQ).
- Web Api filtering.
- API throttling.
- Logging both txt file and database table.
- Enable Cors.
- JWT Authentication (JSON Web Token).
- Authorization Roles.
- Logging (log to txt file in project directory and write in database).
- Image upload in project directory and retrieve image from directory.
- Cache Profiles.
- Send Sms with Twilio.
- Send Email with Mailkit.
- Web api to generate QR image in 3 different extensions (png/jpeg/html).
- Web api to find hidden text in QR image.
- Generate serial key from hardware id and online validation (only in GetAllHeroes api)
- Two-Factor Authentication with e-mail, validating 2FA token in separate controller.
- Custom global error handling in middleware.
- Unit Testing (XUnit & Moq) for HeroController.
- API HealthChecks (controller & dashboard)
