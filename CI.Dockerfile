FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base

USER root
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

USER app
WORKDIR /app
EXPOSE 8080

# Copies the output from the GitHub Actions runner
COPY ./publish .

ENTRYPOINT ["dotnet", "HomeLabCore.Api.dll"]
