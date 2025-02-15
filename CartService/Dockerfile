# Use the .NET 8 runtime image for deployment
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Copy the already-published output to the container
COPY ./Publish/* ./

# add this library
RUN apt-get update && apt-get install -y librdkafka-dev
RUN apt-get update && apt-get install -y netcat-openbsd

# Expose the port for this service
EXPOSE 5002

# Start the application
ENTRYPOINT ["dotnet", "CartService.dll"]

