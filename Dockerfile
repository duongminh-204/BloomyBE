# ================= BUILD STAGE =================

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

COPY . .

WORKDIR /src/BloomyBE

RUN dotnet restore

RUN dotnet publish -c Release -o /app/publish


# ================= RUNTIME STAGE =================

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final

WORKDIR /app

COPY --from=build /app/publish .

# Render dùng port động
ENV ASPNETCORE_URLS=http://+:10000

EXPOSE 10000

ENTRYPOINT ["dotnet", "BloomyBE.dll"]