.PHONY: up test

# plain `dotnet run` never launches a browser (that's an IDE/dotnet-watch behavior, not dotnet run) —
# dotnet watch honors launchSettings.json's launchBrowser/launchUrl and gives hot reload as a bonus.
up:
	dotnet watch run --project src/CommBiz.Api --launch-profile http --non-interactive

test:
	dotnet test
