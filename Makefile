SOLUTION=BmsBridge.slnx
PROJECT=BmsBridge
TEST_PROJECT=BmsBridge.Tests

.PHONY: build test run capture clean restore publish

restore:
	dotnet restore $(SOLUTION)

build:
	dotnet build $(SOLUTION) --no-restore

test:
	dotnet test $(TEST_PROJECT) --no-build --verbosity normal

run:
	dotnet run --project $(PROJECT)

capture:
	dotnet run --project $(PROJECT) --test-operator

clean:
	dotnet clean $(SOLUTION)
	rm -rf $(PROJECT)/bin $(PROJECT)/obj
	rm -rf $(TEST_PROJECT)/bin $(TEST_PROJECT)/obj

publish:
	dotnet publish $(PROJECT) -c Release -o publish
