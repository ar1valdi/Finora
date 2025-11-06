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
