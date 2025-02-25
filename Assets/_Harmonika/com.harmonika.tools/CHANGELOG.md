## [0.2.9] - 25 February 2025
 - Added function to enable/disable HiddenButton
 - Added enum LeadID to Generics
## [0.2.8] - 25 February 2025
 - Fix DataSync to save field dataHora during SendLeads
 - Changed to only init sync when clicking on the button
 - Fix Loading screen
 - Removed a message not used on GetStatusFromBubble
## [0.2.7] - 17 January 2025
 - Add iGame interface
 - Fix 'Parseable.data' name to '.date'
 - Add 'tempo' and 'pontos' variables to DataSync.cs
 - Add floating toast tool
 - Fix visual bug on cronometer and bug on DataSync
 - Add UI responsivity to appmanager
 - Add new key to bubble json on datasync
 - Fix PeriodicConnect method
## [0.2.6] - 12 December 2024
 - Fix DatabaseTester not working on package because of assembly definition
 - Fix wrong class reference needed on config object in AppManager
 - Fixed bug when not registering leads
 - Add new cronometer component
 - Improve assembly definition and AutoLeadTestMenu
## [0.2.5] - 10 December 2024
 - Create LeadBox_Prefab, an automatized UI component that can be used to captate Leads
 - Created interface 'iGame' to control all games on AppManager
 - Create 'AutoLeads', an automatic script that creates all the captation lead UI with a ScrollView component
 - Added NaughtyAttributes to project (Dependence not worked, so the package files where added directly on project)
 - Integrate Bubble database (with switch to firebase)
 - Remade package structure
 - Add 'DatabaseTester' sample
## [0.2.4] - 08 November 2024
 - Fixed 'GetRandomPrizeInList' method logic on Storage
 - Fixed 'ApplyScriptable' method on AppManager that nedded to be public
 - Created 'PeriodicConnect' method in DataSync
 - Created a custon invoke (Harmonika.Tools.InvokeUtils.Invoke()) that receivas a callback instead a string
 - Created many new extendion methods in 'Generics.cs' script
 - Implements improvements in the SyncMenu.cs interface to facilitate synchronizations
 - Fixed 'Out of Memory' bug when too many itens was being added on Storage
## [0.2.3] - 05 November 2024
 - Remove build folder, harmonika builder scripts will be part of another package 'com.harmonika.builder'
## [0.2.2] - 05 November 2024
 - Fixed sync logic and SyncMenu responsivity
## [0.2.1] - 04 November 2024
 - Project is ready to be downloaded directly from git
## [0.0.1] - 01 November 2024
 - Initial Version