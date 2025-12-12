# CoffeePeek Back-End

This repository contains the back-end source code for the CoffeePeek application, a microservices-based platform designed to connect coffee enthusiasts with coffee shops.

## About The Project

CoffeePeek is a comprehensive platform that allows users to discover coffee shops, view job vacancies, and share their experiences. The back-end is built using a microservices architecture to ensure scalability, resilience, and maintainability. Each service is responsible for a specific business domain, communicating with others through a well-defined API gateway.

## Features

The platform is composed of several services, each providing a distinct set of features:

*   **API Gateway**: A single entry point for all client requests, routing them to the appropriate downstream service. It is implemented using YARP Reverse Proxy.
*   **Authentication Service**: Handles user registration, login, and JWT-based authentication.
*   **User Service**: Manages user profiles and related data.
*   **Shops Service**: Provides functionality for listing coffee shops, viewing their details, and managing shop information.
*   **Job Vacancies Service**: Allows coffee shops to post job openings and users to browse and apply for them.
*   **Photo Service**: Manages photo uploads for users and shops.
*   **Moderation Service**: Responsible for content moderation to ensure a safe and positive user experience.

## Tech Stack

The project is built with a modern technology stack:

*   **[.NET 10](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)**: The primary framework for building the services.
*   **[ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/)**: Used for building web APIs.
*   **[YARP (Yet Another Reverse Proxy)](https://microsoft.github.io/reverse-proxy/)**: Powers the API Gateway.
*   **[Docker](https://www.docker.com/)**: The services are containerized for easy deployment and scaling.
*   **Microservices Architecture**: The overall design pattern for the system.
*   **Entity Framework Core**: Used as the Object-Relational Mapper (ORM) for data access.
*   **PostgreSQL**: The relational database for the services.
*   **RabbitMQ**: Used for asynchronous communication between services.
*   **xUnit/NUnit**: For unit and integration testing.
