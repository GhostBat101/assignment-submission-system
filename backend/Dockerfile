# File: Dockerfile (backend/Dockerfile)
# Purpose: Multi-stage Docker container build for the ASP.NET Core 8 Web API.

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY AssignmentSubmission.sln ./
COPY AssignmentSubmission.Api/*.csproj ./AssignmentSubmission.Api/
COPY AssignmentSubmission.Core/*.csproj ./AssignmentSubmission.Core/
COPY AssignmentSubmission.Infrastructure/*.csproj ./AssignmentSubmission.Infrastructure/
COPY AssignmentSubmission.Services/*.csproj ./AssignmentSubmission.Services/
COPY AssignmentSubmission.Tests/*.csproj ./AssignmentSubmission.Tests/

# Restore dependencies
RUN dotnet restore

# Copy entire source code and build
COPY . .
WORKDIR /src/AssignmentSubmission.Api
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:5000
EXPOSE 5000

ENTRYPOINT ["dotnet", "AssignmentSubmission.Api.dll"]
