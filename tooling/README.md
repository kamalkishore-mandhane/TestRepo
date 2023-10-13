# SharedTooling
This repository is for shared build scripts, common and template files that are not cmake specific.
It is included via a [gitmodule](https://git-scm.com/book/en/v2/Git-Tools-Submodules) to our WzRefactor projects.

Run the following command within your target repository:
```
git submodule add https://github.com/Alludo-WinZip/SharedTooling.git tooling
```
This will create a .gitmodule file containing:
```
[submodule "SharedTooling"]
	path = tooling
	url = https://github.com/Alludo-WinZip/SharedTooling.git
```

You then need to run to commands to populate the ./tooling folder:
```bash
git submodule init
git submodule update
```

Or if when initially cloning a repo with gitmodules you can use the --recurse-submodules parameter:
```
git clone --recurse-submodules https://github.com/Alludo-WinZip/<your-repository>.git
```

## Documentation Support
Doxygen and Sphinx along with some other subsidiary tools are needed to generate documentation.

### Workflow Setup
The Doxygen/Sphinx toolchain makes use of several addons and plugins.
doxygen-awesome-css is installed using git submodule.
```bash
git submodule add https://github.com/jothepro/doxygen-awesome-css.git
```

### Building Documentation Locally
Install PlantUML locally on your system using Chocolatey:
```ps1
choco install plantuml
```

### Future Enhancements
* 
* Add current version of NuGet package