FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src
COPY ["src/pbt.Web.Host/pbt.Web.Host.csproj", "src/pbt.Web.Host/"]
COPY ["src/pbt.Web.Core/pbt.Web.Core.csproj", "src/pbt.Web.Core/"]
COPY ["src/pbt.Application/pbt.Application.csproj", "src/pbt.Application/"]
COPY ["src/pbt.Core/pbt.Core.csproj", "src/pbt.Core/"]
COPY ["src/pbt.EntityFrameworkCore/pbt.EntityFrameworkCore.csproj", "src/pbt.EntityFrameworkCore/"]
WORKDIR "/src/src/pbt.Web.Host"
RUN dotnet restore 

WORKDIR /src
COPY ["src/pbt.Web.Host", "src/pbt.Web.Host"]
COPY ["src/pbt.Web.Core", "src/pbt.Web.Core"]
COPY ["src/pbt.Application", "src/pbt.Application"]
COPY ["src/pbt.Core", "src/pbt.Core"]
COPY ["src/pbt.EntityFrameworkCore", "src/pbt.EntityFrameworkCore"]
WORKDIR "/src/src/pbt.Web.Host"
RUN dotnet publish -c Release -o /publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0
EXPOSE 80
WORKDIR /app
COPY --from=build /publish .
ENTRYPOINT ["dotnet", "pbt.Web.Host.dll"]
