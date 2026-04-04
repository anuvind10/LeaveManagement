# Stage 1 - Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

# Copy solution and project files first for layer caching
COPY *.slnx .
COPY LeaveManagement.API/LeaveManagement.API.csproj LeaveManagement.API/
COPY LeaveManagement.Application/LeaveManagement.Application.csproj LeaveManagement.Application/
COPY LeaveManagement.Domain/LeaveManagement.Domain.csproj LeaveManagement.Domain/
COPY LeaveManagement.Infrastructure/LeaveManagement.Infrastructure.csproj LeaveManagement.Infrastructure/
COPY LeaveManagement.Tests/LeaveManagement.Tests.csproj LeaveManagement.Tests/

# Restore dependencies
RUN dotnet restore

# Copy everything else
COPY . .

# Publish the API project
RUN dotnet publish LeaveManagement.API/LeaveManagement.API.csproj -c Release -o /app/publish

# Stage 2 - Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "LeaveManagement.API.dll"]