FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["MongoDB.AspNet.Identity.sln", "."]
COPY ["MongoDB.AspNet.Identity.csproj", "."]
COPY ["samples/SampleWebApp/SampleWebApp.csproj", "samples/SampleWebApp/"]

# Restore dependencies
RUN dotnet restore "samples/SampleWebApp/SampleWebApp.csproj"

# Copy source files
COPY . .

# Build the application
WORKDIR "/src/samples/SampleWebApp"
RUN dotnet build "SampleWebApp.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "SampleWebApp.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SampleWebApp.dll"]
