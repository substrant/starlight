# Testing

## Environment Variables

The following environment variables are used by the test program:
- `IS_CI` - This is set to `1` when running on a CI server. This is used to disable some tests that are not compatible when running CI.
- `AUTH_TOKEN` - If this is set, the launcher will be tested with the specified authentication token. This is used to test the launcher. If not provided, the test will skip.

## Testing With Visual Studio

To test Starlight, you will need to build the solution, and then run the tests. You can do this by right clicking the solution in the Solution Explorer, and clicking Run Tests. The tests will be compiled into `src/Starlight.Tests/bin/`.

## Testing With The Command Line (Windows)

Starlight uses NUnit for testing. You can run the tests by running the following command:

```bash
nunit3-console src/Starlight.Tests/bin/Release/Starlight.Tests.dll
```

This will run the tests in `Release` mode. You can also run the tests in `Debug` mode by replacing `Release` with `Debug`. Make sure that you have built the solution before running the tests.

## Disclaimer

If you are planning to contribute to Starlight, you will need to ensure that all of the tests pass before merging your pull request, especially when refactoring code. If you are adding a new feature, you will need to add tests for it. If you are fixing a bug, you will need to add a test with a scenario that fails without your fix, and passes with your fix. Since CI doesn't support all of the tests, you will need to run the tests locally before submitting a pull request.