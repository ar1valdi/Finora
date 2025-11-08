# Finora

Banking crud application that simulates microservice architecture and handling transactions in it. There are three applications, mailing, front and backend, which all communicate through rabbitmq. Created in order to learn architectural design patterns in real use, hence security is fully omitted.

Following design patterns implemented:
- transacional outbox
- cqrs
- event bus
- fail fast
- circuit breaker
- unit of work
- handshake
- timeouts

Project is ready to be built after cloning, appsettings and .env contain example secret files that work in local environment. Change those in any form of deployment.
