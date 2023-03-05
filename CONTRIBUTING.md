## Setup

- Install [Mono](https://www.mono-project.com/download/stable/)
- Use [VSCode](https://code.visualstudio.com/download)
- Install the [C# extension](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csharp) (it should prompt you after opening the project in VSCode)
- Run `nuget restore EndnightMods.sln` to install dependencies

## Creating a new mod

Generate code with:

```sh
csi ./scripts/CreateNewProject.csx "game" "ProjectName" "Description of project."
```

## Build

```sh
msbuild /property:Configuration=Release
```

Uncomment, comment or change the post-build scripts in the `.cspoj` files as necessary (these are the scripts which automatically copy the built mods into your game after building).

## Branches

The `main` branch contains the latest stable changes (ie. the code that is built and uploaded to Thunderstore). I do my own development on the `develop` branch and every push to this branch creates dev builds (you can see them under releases).

In most cases you want to fork from and merge PRs to the `main` branch. However if you're curious you can look at what's happening in the `develop` branch.
