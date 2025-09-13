
# FROM hub.kserv.ir/base-images/dotnet/sdk:8.0 AS build
# WORKDIR /app
# EXPOSE 5000
# ENV ASPNETCORE_HTTP_PORTS=5000

# # Copy everything
# COPY . ./
# # Restore as distinct layers
# RUN dotnet restore "Service.Worker/Service.Worker.csproj" --configfile "Service.Worker/nuget.config"
# # Build and publish a release
# RUN dotnet publish "Service.Worker/Service.Worker.csproj" -c Release -o out --configfile "Service.Worker/nuget.config"

# Build runtime image
FROM hub.kserv.ir/creditcore-stage/aspnet:8.0-test
EXPOSE 5000
ENV ASPNETCORE_HTTP_PORTS=5000
ENV CSProject_Name=Service.Worker
ENV DOTNET_TieredCompilation=true
WORKDIR /app
COPY ./out/worker .
ENTRYPOINT ["dotnet", "Service.Worker.dll"]



