# Set the base image as the .NET 7.0 SDK (this includes the runtime)
FROM mcr.microsoft.com/dotnet/sdk:7.0 as build-env

# Copy everything and publish the release (publish implicitly restores and builds)
WORKDIR /app
COPY . ./
RUN dotnet publish DotNet.GitHubAction.csproj -c Release -o out --no-self-contained

# Label the container
LABEL maintainer="Kamil Hussain <kamil467@gmail.com>"
LABEL repository="https://github.com/kamil467/Release-Summary-GitHub-Action"
LABEL homepage="https://github.com/kamil467/Release-Summary-GitHub-Action"

# Label as GitHub action
LABEL com.github.actions.name="github-release-summary"
# Limit to 160 characters
LABEL com.github.actions.description=".NET 7 based image for printing release summary in github action workflow."
# See branding:
# https://docs.github.com/actions/creating-actions/metadata-syntax-for-github-actions#branding
LABEL com.github.actions.icon="activity"
LABEL com.github.actions.color="orange"

# Relayer the .NET SDK, anew with the build output - final image only contain dotnet runtime
FROM mcr.microsoft.com/dotnet/runtime:7.0
COPY --from=build-env /app/out .
ENTRYPOINT [ "dotnet", "/DotNet.GitHubAction.dll" ]
