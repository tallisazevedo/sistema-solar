# Shared

This project was generated using [Angular CLI](https://github.com/angular/angular-cli) version 22.1.0.

## Client de API gerado do OpenAPI

`src/lib/api/` e gerado por [`ng-openapi-gen`](https://github.com/cyclosproject/ng-openapi-gen)
a partir do documento OpenAPI da `SolarES.Api` — nao editar esses arquivos a mao.
Para regerar depois de uma mudanca na Api:

```bash
dotnet run --project ../../src/SolarES.Api   # sobe a Api em http://localhost:5299
npm run generate:api-client                  # dentro de web/, le /openapi/v1.json e regera
ng build shared                              # o admin/landing importam de dist/shared
```

## Code scaffolding

Angular CLI includes powerful code scaffolding tools. To generate a new component, run:

```bash
ng generate component component-name
```

For a complete list of available schematics (such as `components`, `directives`, or `pipes`), run:

```bash
ng generate --help
```

## Building

To build the library, run:

```bash
ng build shared
```

This command will compile your project, and the build artifacts will be placed in the `dist/` directory.

### Publishing the Library

Once the project is built, you can publish your library by following these steps:

1. Navigate to the `dist` directory:

   ```bash
   cd dist/shared
   ```

2. Run the `npm publish` command to publish your library to the npm registry:
   ```bash
   npm publish
   ```

## Running unit tests

To execute unit tests with the [Vitest](https://vitest.dev/) test runner, use the following command:

```bash
ng test
```

## Running end-to-end tests

For end-to-end (e2e) testing, run:

```bash
ng e2e
```

Angular CLI does not come with an end-to-end testing framework by default. You can choose one that suits your needs.

## Additional Resources

For more information on using the Angular CLI, including detailed command references, visit the [Angular CLI Overview and Command Reference](https://angular.dev/tools/cli) page.
