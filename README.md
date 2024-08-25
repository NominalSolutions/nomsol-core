# Nominal Solutions Core API Extensions

Nominal Solutions Core API Extensions is a .NET library that provides essential tools and configurations to streamline the development of ASP.NET Core APIs. This library includes a customizable Basic Authentication handler, comprehensive Swagger configuration, and API versioning support. With built-in support for Serilog logging and seamless integration into existing projects, this library is designed to help you build robust and secure APIs with minimal setup.

## Features

- **Basic Authentication**: Secure your APIs using the `BasicAuthenticationHandler` that verifies credentials against your configuration settings.
- **Swagger Integration**: Automatically generate and customize API documentation with version-specific Swagger setups using the `ConfigureSwaggerOptions` class.
- **API Versioning**: Easily manage multiple versions of your API with minimal configuration using ASP.NET Core’s API versioning tools.
- **Business Service Registration**: Simplify service registration with attribute-based automatic DI configuration for business services.
- **Extensive Logging**: Leverage Serilog for comprehensive logging across your application, ensuring that all key actions and configurations are logged.

## Installation

To use this library in your project, install package from nuget:

   ```bash
   dotnet add package nomsol.core.api --version 0.0.1-beta
```
## Usage
### 1. Basic Authentication
To add basic authentication to your API, include the BasicAuthenticationHandler in your Startup.cs or equivalent file:

```csharp
services.AddAuthentication("BasicAuthentication")
        .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>("BasicAuthentication", null);
```
### 2. Swagger Configuration
To configure Swagger with detailed versioning and custom UI options, use the ConfigureSwaggerOptions class:

```csharp
services.AddTransient<IConfigureOptions<SwaggerGenOptions>>(serviceProvider =>
    new ConfigureSwaggerOptions(
        serviceProvider.GetRequiredService<IApiVersionDescriptionProvider>(),
        hostingEnvironment,
        applicationName,
        releaseVersion));
```
### 3. API Versioning
Enable API versioning in your project with a single method call:

```csharp
services.AddMinimalApiVersioning(environment, configuration);
```
### 4. Business Service Registration
Automatically register services with custom attributes using the AddBusinessServices method:

```csharp
services.AddBusinessServices();
```
### 5. Custom Swagger UI
Customize the Swagger UI by adding your custom CSS or JavaScript files:

```csharp
app.AddCustomSwagger(configuration, new List<string> { "v1", "v2" });
```

## Contributing
We welcome contributions to enhance this library. Please fork the repository, make your changes, and submit a pull request. Ensure that your code follows the existing style and includes appropriate tests.
