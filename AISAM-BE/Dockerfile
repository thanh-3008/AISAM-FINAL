# Use the .NET SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project files and restore dependencies
COPY ["AISAM.API/AISAM.API.csproj", "AISAM.API/"]
COPY ["AISAM.Common/AISAM.Common.csproj", "AISAM.Common/"]
COPY ["AISAM.Data/AISAM.Data.csproj", "AISAM.Data/"]
COPY ["AISAM.Repositories/AISAM.Repositories.csproj", "AISAM.Repositories/"]
COPY ["AISAM.Services/AISAM.Services.csproj", "AISAM.Services/"]
RUN dotnet restore "AISAM.API/AISAM.API.csproj"

# Copy the rest of the code and build
COPY . .
WORKDIR "/src/AISAM.API"
RUN dotnet build "AISAM.API.csproj" -c Release -o /app/build
RUN dotnet publish "AISAM.API.csproj" -c Release -o /app/publish

# Use the .NET runtime image to run the app
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Expose port for Render
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "AISAM.API.dll"]
