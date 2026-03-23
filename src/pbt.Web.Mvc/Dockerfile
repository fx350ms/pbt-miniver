FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

RUN dotnet tool install -g Microsoft.Web.LibraryManager.Cli
RUN ln -s ~/.dotnet/tools/libman /usr/local/bin/libman

WORKDIR /src
COPY ["src/pbt.Web.Mvc/pbt.Web.Mvc.csproj", "src/pbt.Web.Mvc/"]
COPY ["src/pbt.Web.Core/pbt.Web.Core.csproj", "src/pbt.Web.Core/"]
COPY ["src/pbt.Application/pbt.Application.csproj", "src/pbt.Application/"]
COPY ["src/pbt.Core/pbt.Core.csproj", "src/pbt.Core/"]
COPY ["src/pbt.EntityFrameworkCore/pbt.EntityFrameworkCore.csproj", "src/pbt.EntityFrameworkCore/"]
WORKDIR "/src/src/pbt.Web.Mvc"
RUN dotnet restore 

WORKDIR /src
COPY ["src/pbt.Web.Mvc", "src/pbt.Web.Mvc"]
COPY ["src/pbt.Web.Core", "src/pbt.Web.Core"]
COPY ["src/pbt.Application", "src/pbt.Application"]
COPY ["src/pbt.Core", "src/pbt.Core"]
COPY ["src/pbt.EntityFrameworkCore", "src/pbt.EntityFrameworkCore"]
WORKDIR "/src/src/pbt.Web.Mvc"
RUN libman restore
RUN dotnet publish -c Release -o /publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0
EXPOSE 80
WORKDIR /app
COPY --from=build /publish .
ENTRYPOINT ["dotnet", "pbt.Web.Mvc.dll"]
