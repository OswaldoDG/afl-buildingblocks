FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /App

# Copy everything
COPY . ./
# Restore as distinct layers
RUN dotnet restore "CloudReportsBuildingBlocksPOC/CloudReportsBuildingBlocksPOC.csproj"

# Build and publish a release
RUN dotnet publish "CloudReportsBuildingBlocksPOC/CloudReportsBuildingBlocksPOC.csproj" -c Release -o /App/out
    
# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /App
COPY --from=build /App/out .

RUN apt-get update && apt-get install -y curl
RUN apt-get install -y tzdata

ENTRYPOINT ["dotnet", "CloudReportsBuildingBlocksPOC.dll"]
