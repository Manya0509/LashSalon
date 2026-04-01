FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

RUN apt-get update && apt-get install -y libfontconfig1 && rm -rf /var/lib/apt/lists/*

COPY publish/ .
ENTRYPOINT ["dotnet", "LashBooking.Web.MVC.dll"]
