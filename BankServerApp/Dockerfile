FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["BankServerApp/BankServerApp.csproj", "BankServerApp/"]
RUN dotnet restore "BankServerApp/BankServerApp.csproj"
COPY . .
WORKDIR "/src/BankServerApp"
RUN dotnet build "BankServerApp.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BankServerApp.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BankServerApp.dll"]
