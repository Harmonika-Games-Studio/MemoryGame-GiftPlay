# Harmonika Unity Packages (In Development)

Welcome to the **Harmonika Unity Packages** repository! This repository is currently **under development** and contains two essential Unity packages designed to enhance internal workflows for Harmonika projects. These packages are not yet finalized and are subject to frequent updates and changes. They aim to improve productivity by providing functionalities for event-based applications, lead capture, license validation, and automated builds.

---

## Packages Overview

### 1. [Harmonika Build Unity Package](./Harmonika-Build-Unity-Package)

**Status:** _In Development_  
**Description:**  
The Harmonika Build Unity Package introduces automated build processes to simplify the creation of various build types directly from the Unity Editor. This package includes a custom script for generating standalone executables, Inno Setup installers, and standard builds, reducing manual setup and enhancing the deployment workflow.

**Key Features:**
- **Single Executable (SFX) Build**: Creates a standalone `.exe` that encapsulates all necessary files.
- **Installer Creation with Inno Setup**: Automates the creation of customizable installer packages.
- **Default Builds**: Supports standard Unity build configurations.

> **Note:** As this package is still in development, features may be incomplete or subject to change.

For more details, see the package documentation: [Harmonika Build Unity Package](./Harmonika-Build-Unity-Package/README.md)

---

### 2. [Harmonika AppManager Unity Package](./Harmonika-AppManager-Unity-Package)

**Status:** _In Development_  
**Description:**  
The Harmonika AppManager Unity Package provides an overlay for managing settings related to events, lead capture, license validation, and general application configurations. It also includes sections for contact information and tutorials to guide users in effectively utilizing the application.

**Key Features:**
- **Reward Inventory Management**: Manages prize inventory for event-driven applications.
- **Lead Capture Configuration**: Facilitates lead management with integration into Harmonika's CRM.
- **License Validation**: Streamlines license compliance within applications.
- **General Application Settings**: Provides flexible configuration options for various use cases.
- **Contact Information and Tutorials**: Supplies user guides and contact information for support.

> **Note:** This package is also in development, and additional features or modifications are expected.

For more details, see the package documentation: [Harmonika AppManager Unity Package](./Harmonika-AppManager-Unity-Package/README.md)

---

## Getting Started

Since this repository is in active development, usage instructions may change frequently. However, you can start exploring the packages by following these steps:

### Step 1: Clone this repository

```bash
git clone https://github.com/Harmonika-Games-Studio/Harmonika-Unity-Packages.git
```
This will create a local copy of the repository on your machine. You can navigate to the directory to explore the folder structure, documentation, and package files.

### Step 2: Open Your Unity Project

Open Unity Hub and select the Unity project where you want to add the Harmonika packages, or create a new project if you’re starting from scratch.  
Once your project is open, proceed with the following steps to import the packages.

### Step 3: Importing the Packages

The Harmonika Unity Packages can be imported directly from your local clone:

1. Go to **Window > Package Manager** in Unity to open the Package Manager.
2. In the Package Manager, click on the **+** button in the top left corner.
3. Select **Add package from disk...** from the dropdown menu.
4. Navigate to the cloned repository folder on your machine and locate the `package.json` file for the package you want to import:
   - For the **Harmonika Build Unity Package**:  
     Navigate to `Harmonika-Unity-Packages/Harmonika-Build-Unity-Package/package.json`.
   - For the **Harmonika AppManager Unity Package**:  
     Navigate to `Harmonika-Unity-Packages/Harmonika-AppManager-Unity-Package/package.json`.
5. Select the `package.json` file and click **Open**. Unity will automatically import the package into your project.

Repeat these steps for each package you want to import.

### Step 4: Initial Setup and Configuration

After importing the packages, you may need to perform some initial setup:

- **Harmonika Build Unity Package**:
  - Go to **Harmonika > Build Settings** in the Unity menu to access the custom build options.
  - Configure the settings for Single Executable (SFX) builds, Inno Setup installer generation, and default builds as needed for your project.

- **Harmonika AppManager Unity Package**:
  - Go to **Harmonika > AppManager Settings** in the Unity menu to open the settings overlay.
  - Configure your project’s lead capture, reward inventory, license validation, and other general application settings from this overlay.
  - Use the **Contact & Tutorials** section to provide relevant user guides and support information for your application.

> **Note**: Since this repository is in active development, some settings or features may be in experimental stages.

### Step 5: Testing and Verifying Package Integration

Once the packages are configured, it’s important to test their functionality:

- **Run a Test Build** (for the Harmonika Build Unity Package):
  - Use the build options to create a sample build and verify that the Single Executable (SFX) and installer generation are functioning as expected.
  - Check that the resulting files are generated correctly and that the build runs without issues.

- **Test AppManager Features** (for the Harmonika AppManager Unity Package):
  - Verify that settings such as lead capture, reward inventory, and license validation work within the context of your application.
  - Test the **Contact & Tutorials** section to ensure that the information and links are accessible to users.

### Step 6: Keeping Up with Updates

Since these packages are still in development, you may need to periodically update them:

1. Pull the latest changes from the GitHub repository:
   ```bash
   git pull origin main
   ```
2. Repeat the package import steps to apply the latest updates to your Unity project.

---

### Troubleshooting

If you encounter any issues during setup or usage:

- **Check the Unity Console**: Any errors related to missing dependencies or incorrect configurations will appear in the Unity Console.
- **Refer to Documentation**: Each package includes a README file with specific details about setup and known issues.
- **Contact Harmonika Support**: If the issue persists, reach out to the Harmonika development team for assistance.

---

By following these steps, you’ll be able to integrate and configure the Harmonika Unity Packages to enhance your project’s build workflow and application settings. Thank you for your help in testing and refining these packages as we continue development!
