FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["LashBooking.Web.MVC/LashBooking.Web.MVC.csproj", "LashBooking.Web.MVC/"]
COPY ["LashBooking.Domain/LashBooking.Domain.csproj", "LashBooking.Domain/"]
COPY ["LashBooking.Infrastructure/LashBooking.Infrastructure.csproj", "LashBooking.Infrastructure/"]

RUN dotnet restore "LashBooking.Web.MVC/LashBooking.Web.MVC.csproj"

COPY . .
WORKDIR "/src/LashBooking.Web.MVC"
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "LashBooking.Web.MVC.dll"]