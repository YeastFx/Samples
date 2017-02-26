# DataWebApp
Minimalist data management web app with authentication based on Yeast.Multitenancy.


Tenants are identified by incoming port number.

Tenant identifier is displayed in page title and header.

Each tenant has its own database for user identity and application data storage.

Uses StructureMap as DI container.