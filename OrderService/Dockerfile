# Use the runtime image for deployment
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Copy the already-published output to the container
COPY ./Publish/* ./

ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1
ENV LD_LIBRARY_PATH=/app/linux-arm64/native:$LD_LIBRARY_PATH
ENV ASPNETCORE_URLS=http://+:5001

# add this library
# RUN apt-get update && apt-get install -y librdkafka-dev
RUN apt-get update
RUN apt-get install -y librdkafka-dev

# Expose the port for this service
EXPOSE 5001

# Start the application
ENTRYPOINT ["dotnet", "OrderService.dll"]

