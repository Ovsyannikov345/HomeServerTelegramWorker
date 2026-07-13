FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER app
WORKDIR /app
EXPOSE 8080

# Copies the output from the GitHub Actions runner
COPY ./publish .

ENTRYPOINT ["dotnet", "HomeLabCore.Api.dll"]
