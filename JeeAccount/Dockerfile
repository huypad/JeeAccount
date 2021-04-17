FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["JeeAccount/JeeAccount.csproj", "JeeAccount/"]
RUN dotnet restore "JeeAccount/JeeAccount.csproj"
COPY JeeAccount JeeAccount/
WORKDIR "/src/JeeAccount"
RUN dotnet build "JeeAccount.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "JeeAccount.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "JeeAccount.dll"]