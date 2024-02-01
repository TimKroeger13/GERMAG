<h2>Database scaffolding:</h2>

````
Scaffold-DbContext "Host=192.168.1.25:5433;Database=postgres;Username=postgres;Password=password" Npgsql.EntityFrameworkCore.PostgreSQL -NoOnConfiguring -OutputDir ../Shared/DataModel/Database -Force -Context DataContext -Namespace GERMAG.DataModel.Database -StartupProject GERMAG.Server -Project GERMAG.Shared
````