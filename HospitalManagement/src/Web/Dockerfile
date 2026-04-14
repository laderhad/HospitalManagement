FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and projects
COPY HospitalManagement.slnx ./
COPY Directory.Build.props ./
COPY Directory.Packages.props ./
COPY global.json ./
COPY src/Web/ src/Web/
COPY src/Infrastructure/ src/Infrastructure/
COPY src/Shared/ src/Shared/
COPY src/Domain/ src/Domain/
COPY src/Application/ src/Application/

WORKDIR /src/src/Web

RUN dotnet restore "Web.csproj"
RUN dotnet publish "Web.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish ./

ENV ASPNETCORE_URLS="http://+:80"
EXPOSE 80

ENTRYPOINT ["dotnet", "Web.dll"]
