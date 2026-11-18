# Elastic search demo app

This is a sandbox project implements a .NET API and a minimal infrastructe set up for a simple Products Microsservice with CRUD and indexing funcionalities.

Technologies:

- Elastic Search: indexing and tracking entities from a relational database;
- Kubernetes: local cluster set up with Minikube;
- Docker Compose for local development.

Architecture:

- DDD:
  - The API is designed as a Products Bounded Context in a broader application.
  - A single Ubiquitous Language is kept along the implementation
  - Domain invariants are implemented in the domain layer with a rich domain.
- Clean Architecture
- CQRS

## Improvement suggestions

- Replace EventBus (with dql etc.)
- Replace Domain Exceptions for result pattern
- Use a mapping library instead of mapping every object
- Pagination on search query

## Next steps

Application:

- Refactor Exception
- Include Unit Of Work
- Include Outbox pattern for event message
- Ensuring entities changes and the outbox message are persisted as a single UOW
- Refactor the EventProcessor into it's own background service;
- Creation of a Infra Layer with all the infra dependencies
- Refactor DI into its own file
- Create controllers
- Resolve build warning

K8s:

set up database initialization on minikube
