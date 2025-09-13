
# FROM hub.kserv.ir/base-images/dotnet/sdk:8.0 AS build
# WORKDIR /app
# EXPOSE 80
# ENV ASPNETCORE_HTTP_PORTS=80

# # Copy everything
# COPY . ./
# # Restore as distinct layers
# RUN dotnet restore "Service.Rest/Service.Rest.csproj" --configfile "Service.Rest/nuget.config"
# # Build and publish a release
# RUN dotnet publish "Service.Rest/Service.Rest.csproj" -c Release -o out --configfile "Service.Rest/nuget.config"

# Build runtime image
FROM hub.kserv.ir/creditcore-stage/aspnet:8.0-test
EXPOSE 80
ENV ASPNETCORE_HTTP_PORTS=80
ENV CSProject_Name=Service.Rest
ENV DOTNET_TieredCompilation=true
WORKDIR /app
COPY ./out/rest .
ENTRYPOINT ["dotnet", "Service.Rest.dll"]



