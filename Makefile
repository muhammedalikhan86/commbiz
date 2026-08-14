.PHONY: up test

up:
	dotnet run --project src/CommBiz.Api

test:
	dotnet test
