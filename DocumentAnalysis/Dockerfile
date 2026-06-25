FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish DocumentAnalysis/DocumentAnalysis.Controller/DocumentAnalysis.Controller.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
RUN apt-get update \
    && apt-get install -y --no-install-recommends libgdiplus libc6-dev tesseract-ocr tesseract-ocr-eng tesseract-ocr-ron \
    && ln -sf /usr/lib/libgdiplus.so /usr/lib/gdiplus.dll \
    && ln -sf /usr/lib/libgdiplus.so /usr/lib/libgdiplus.dll \
    && rm -rf /var/lib/apt/lists/*
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "DocumentAnalysis.Controller.dll"]
