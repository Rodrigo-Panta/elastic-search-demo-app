# Elastic search

## Next steps

- Creation of a domain layer with every domain specific content. Eg.:
  - Aggregate Product, domain event ProductCreatedEvent, repository interfaces, etc;
- Creation of an application layer with our usecases that are on the minimal API mappings;
- Refactor the EventProcessor into it's own background service;
- Creation of a Infra Layer with all the infra dependencies
- Refactor DI into its own file
- Create controllers