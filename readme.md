# Elastic search demo app

This is a sandbox project implements a .NET API and a minimal infrastructe set up for a simple Products Microsservice with CRUD and indexing funcionalities.

Technologies:

- Elastic Search: indexing and tracking entities from a relational database;
- Kubernetes: local cluster set up with Minikube;
- Docker Compose for local development.

Architecture:

- DDD: The API is designed as a Products Bounded Context in a broader application.

## Improvement suggestions

- Replace EventBus (with dql etc.)
- Replace Domain Exceptions for result pattern 

## Next steps

Application:
- Creation of an application layer with our usecases that are on the minimal API mappings;
- Refactor Exception
- Refactor the EventProcessor into it's own background service;
- Creation of a Infra Layer with all the infra dependencies
- Refactor DI into its own file
- Create controllers

K8s:

set up database initialization on minikube
