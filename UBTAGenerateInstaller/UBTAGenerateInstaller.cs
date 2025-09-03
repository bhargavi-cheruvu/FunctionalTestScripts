using System;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;
using System.Text;
using System.ComponentModel;
using System.Reflection.Metadata.Ecma335;

namespace UBTAGenerateInstaller
{
    internal class UBTAGenerateInstaller
    {
        static void Main(string[] args)
        {
            //args = new string[1] { "C:\\Test\\UBTAInstaller\\" };
            #region Check input parameter.
            if (args is null || args.Length < 1)
            {
                Console.WriteLine("Arguments are wrong defined.");
                Environment.Exit(0);
            }
            else
            {
                Console.WriteLine("Arguments are set");
            }

            if (!Directory.Exists(args[0]) || args[0] is null)
            {
                Environment.Exit(0);
                Console.WriteLine($"Error for {args[0]}");
            }
            else
            {
                Console.WriteLine("Check Directory passed.");
            }
            #endregion

            try
            {
                #region Set the variables.
                XNamespace ns = "http://wixtoolset.org/schemas/v4/wxs";

                // Specify the directory where your .wxs files are located
                string InstallerProjectPath = args[0];
                string wxsDirectory = InstallerProjectPath + @"\obj\x86\Debug";
                string InstallerProjectName = new DirectoryInfo(InstallerProjectPath).Name;

                // Specify the output file path
                string outputFileConnections = InstallerProjectPath + @"\Connections_files.wxs";
                string outputFileScripts = InstallerProjectPath + @"\Scripts_files.wxs";

                // necessary for the local copy with the powershell script
                string locationOfFileCopy = InstallerProjectPath + @"\bin\Debug";

                string solutionDirectory = new DirectoryInfo(InstallerProjectPath)?.Parent?.FullName ?? "";
                string UBTAScriptsPath = solutionDirectory + @"\UniversalBoardTestApplication\UniversalBoardTestApp\bin\Debug\UniversalBoardTestConfig\Scripts\";
                string UBTAConnectionsPath = solutionDirectory + @"\UniversalBoardTestApplication\UniversalBoardTestApp\bin\Debug\UniversalBoardTestConfig\Connections\";

                // Delete the output file, if it exists
                if (File.Exists(outputFileConnections)) File.Delete(outputFileConnections);
                if (File.Exists(outputFileScripts)) File.Delete(outputFileScripts);

                // get all .wxs files from directory
                var wxsFiles = Directory.GetFiles(wxsDirectory, "*.wxs").Select(s => s).Where(path => !path.Contains("Product.Generated.wxs")).ToList();

                #endregion

                #region Check if Package.wxs exist and trigger creation.
                if (!File.Exists(InstallerProjectPath + @"\Package.wxs"))
                {
                    XDocument PackageFile = new XDocument(
                    new XElement(ns + "Wix",
                        new XElement(ns + "Package",
                            new XAttribute("Name", $"{InstallerProjectName}"),
                            new XAttribute("Manufacturer", "Thermo Fisher Scientific, Inc."),
                            new XAttribute("Version", "1.0.0.0"),
                            new XAttribute("UpgradeCode", Guid.NewGuid().ToString()),
                            new XElement(ns + "MajorUpgrade",
                                new XAttribute("AllowDowngrades", "no"),
                                new XAttribute("DowngradeErrorMessage", "A newer version of [ProductName] is already installed."),
                                new XAttribute("AllowSameVersionUpgrades", "no")
                            ),
                            new XElement(ns + "MediaTemplate",
                                new XAttribute("EmbedCab", "yes")
                            ),
                            new XElement(ns + "Feature",
                                new XAttribute("Id", "Main"),
                                new XElement(ns + "ComponentGroupRef", new XAttribute("Id", "Configuration_files")),
                                new XElement(ns + "ComponentGroupRef", new XAttribute("Id", "Connections_files")),
                                new XElement(ns + "ComponentGroupRef", new XAttribute("Id", "Connections.Content")),
                                new XElement(ns + "ComponentGroupRef", new XAttribute("Id", "Scripts_files")),
                                new XElement(ns + "ComponentGroupRef", new XAttribute("Id", "Scripts.Content"))
                                )
                            )
                        )
                    );

                    PackageFile.Save(InstallerProjectPath + @"\Package.wxs");
                }
                #endregion

                #region Check if Folders.wxs exsit and trigger creation.
                if (!File.Exists(InstallerProjectPath + @"\Folders.wxs"))
                {
                    XDocument FolderFile = new XDocument(
                    new XElement(ns + "Wix",
                        new XElement(ns + "Fragment",
                            new XElement(ns + "StandardDirectory",
                                new XAttribute("Id", "ProgramFilesFolder"),
                                new XElement(ns + "Directory",
                                    new XAttribute("Id", "INSTALLFOLDER"),
                                    new XAttribute("Name", "UniversalBoardTestApp"),
                                    new XElement(ns + "Directory",
                                        new XAttribute("Id", "ConfigFolder"),
                                        new XAttribute("Name", "UniversalBoardTestConfig"),
                                        new XElement(ns + "Directory",
                                            new XAttribute("Id", "Connections"),
                                            new XAttribute("Name", "Connections")
                                        ),
                                        new XElement(ns + "Directory",
                                            new XAttribute("Id", "Scripts"),
                                            new XAttribute("Name", "Scripts")
                                            )
                                        )
                                    )
                                )
                            )
                        )
                    );

                    FolderFile.Save(InstallerProjectPath + @"\Folders.wxs");
                }
                #endregion

                #region Check if Configuration_Files.wxs exsit and trigger creation.
                if (!File.Exists(InstallerProjectPath + @"\Configuration_files.wxs"))
                {
                    XDocument ConfigurationFile = new XDocument(
                    new XElement(ns + "Wix",
                        new XElement(ns + "Fragment",
                            new XElement(ns + "ComponentGroup",
                                new XAttribute("Id", "Configuration_files"),
                                new XAttribute("Directory", "ConfigFolder"),
                                new XElement(ns + "Component",
                                    new XElement(ns + "File",
                                        new XAttribute("Source", "UniversalBoardTestConfigRemote.xml"),
                                        new XAttribute("Name", "UniversalBoardTestConfig.xml")
                                        )
                                    )
                                )
                            )
                        )
                    );

                    ConfigurationFile.Save(InstallerProjectPath + @"\Configuration_files.wxs");
                }

                string usedUBTAConfigXML = "";
                XDocument? changeConfigFile = XDocument.Load(InstallerProjectPath + @"\Configuration_files.wxs");

                if (changeConfigFile.Descendants(ns + "Component").FirstOrDefault() is null)
                {
                    changeConfigFile.Descendants(ns + "ComponentGroup").FirstOrDefault()?
                        .Add(new XElement(ns + "Component",
                             new XElement(ns + "File",
                                new XAttribute("Source", "UniversalBoardTestConfigRemote.xml"),
                                new XAttribute("Name", "UniversalBoardTestConfig.xml")
                         )));
                }

                if (wxsFiles.Count > 0)
                    usedUBTAConfigXML = "$(SolutionDir)\\UniversalBoardTestConfig.xml";
                else
                    usedUBTAConfigXML = "$(SolutionDir)\\UniversalBoardTestConfigRemote.xml";


                if (File.Exists(usedUBTAConfigXML.Replace("$(SolutionDir)", solutionDirectory)))
                {
                    if (changeConfigFile.Descendants(ns + "File").FirstOrDefault()?.Attribute("Source")?.Value != usedUBTAConfigXML)
                    {
                        var fileElement = changeConfigFile.Descendants(ns + "File").FirstOrDefault();

                        if (fileElement != null)
                        {
                            var sourceAttribute = fileElement.Attribute("Source");

                            if (sourceAttribute != null)
                            {
                                var elementAttribute = fileElement.Attribute("Source");

                                if (elementAttribute != null) elementAttribute.Value = usedUBTAConfigXML;

                                if (fileElement.Attribute("Name") is null)
                                {
                                    fileElement.Add(new XAttribute("Name", "UniversalBoardTestConfig.xml"));
                                }

                                changeConfigFile.Save(InstallerProjectPath + @"\Configuration_files.wxs");
                            }
                        }
                    }
                }
                else
                {
                    changeConfigFile.Descendants(ns + "Component").Where(s => s.Element(ns + "File")?.Attribute("Name")?.Value.Contains("UniversalBoardTestConfig") == true)?.Remove();
                    changeConfigFile.Save(InstallerProjectPath + @"\Configuration_files.wxs");
                }
                #endregion

                #region Generate a list of all connections and scripts.
                List<string> RefConnections = new List<string>();
                List<string> RefScripts = new List<string>();
                List<string> EmptyList = new List<string>();

                XDocument InstallerProjectSrc = XDocument.Load(InstallerProjectPath + @$"\{InstallerProjectName}.wixproj");
                var includeAttributes = InstallerProjectSrc.Descendants("ProjectReference").Select(projectRef => projectRef?.Attribute("Include")?.Value);

                foreach (var refproject in includeAttributes)
                {
                    if (refproject is null) continue;
                    if (refproject.Contains("Connections")) RefConnections.Add(Path.GetFileName(refproject).Replace("csproj", "wxs"));
                    else if (refproject.Contains("Scripts")) RefScripts.Add(Path.GetFileName(refproject).Replace("csproj", "wxs"));
                    else EmptyList.Add(refproject); ;
                }

                // list should be empty
                if (EmptyList.Count != 0)
                {
                    Console.WriteLine("There are not handled project references");

                    foreach (var item in EmptyList)
                    {
                        Console.WriteLine($"{item}");
                    }
                }

                #endregion

                #region Check items of .wixproj file.

                #region Check if in file .wixproj the reference to WixToolset.Heat is present.
                bool heatReferenceExists = InstallerProjectSrc.Descendants("PackageReference").Any(pr => pr.Attribute("Include")?.Value == "WixToolset.Heat");

                if (!heatReferenceExists)
                {
                    var heatSdkVersion = InstallerProjectSrc.Element("Project")?.Attribute("Sdk")?.Value?.Split('/')[1] ?? "4.0.3";
                    var packageReferenceGroup = InstallerProjectSrc.Descendants("ItemGroup").FirstOrDefault(ig => ig.Elements("PackageReference").Any());

                    if (packageReferenceGroup == null)
                    {
                        packageReferenceGroup = new XElement("ItemGroup");
                        InstallerProjectSrc.Descendants("Project").First().Add(packageReferenceGroup);
                    }

                    packageReferenceGroup.Add(new XElement("PackageReference",
                        new XAttribute("Include", "WixToolset.Heat"),
                        new XAttribute("Version", heatSdkVersion)));
                }
                #endregion

                #region Check if in file .wixproj the ProjectDirectory name exist.
                string relativSolutionpath = @"$(SolutionDir)" + @$"{InstallerProjectName}";
                bool elementExists = InstallerProjectSrc.Descendants("ProjectDirectoryName").Any(element => element.Value == relativSolutionpath);

                XElement? propertyGroupWixProj = InstallerProjectSrc.Descendants("PropertyGroup")?.FirstOrDefault();

                if (!elementExists)
                {
                    if (propertyGroupWixProj is null)
                    {
                        propertyGroupWixProj = new XElement("PropertyGroup");
                        propertyGroupWixProj.Add(new XElement("ProjectDirectoryName", relativSolutionpath));
                        InstallerProjectSrc.Root?.Add(propertyGroupWixProj);
                    }
                    else
                    {
                        propertyGroupWixProj.Add(new XElement("ProjectDirectoryName", relativSolutionpath));
                    }
                }
                #endregion

                #region Check if in file .wixproj harvesting is enabled
                bool harvestingEnabled = InstallerProjectSrc.Descendants("EnableProjectHarvesting").Any(pr => pr.Value == "true");

                if (!harvestingEnabled)
                {
                    if (propertyGroupWixProj == null)
                    {
                        propertyGroupWixProj = new XElement("PropertyGroup");
                        InstallerProjectSrc.Descendants("Project").First().Add(propertyGroupWixProj);
                    }

                    propertyGroupWixProj.Add(new XElement("EnableProjectHarvesting", "true"));
                }
                #endregion

                #region Check if in file .wixproj postbuild event already exists:
                string postBuildEventValue = "powershell -ExecutionPolicy Bypass -File " + @"$(SolutionDir)UBTAGenerateInstaller\CopyFilesFromTxtToTemp.ps1 " + "-DestPath $(ProjectDirectoryName)";
                bool postBuildEventExists = propertyGroupWixProj?.Elements("PostBuildEvent")?.Any(element => element.Value == postBuildEventValue) ?? false;

                if (!postBuildEventExists)
                {
                    propertyGroupWixProj?.Add(new XElement("PostBuildEvent", postBuildEventValue));
                    propertyGroupWixProj?.Add(new XElement("RunPostBuildEvent", "OnBuildSuccess"));
                }
                #endregion

                #region Save all changes in the .wixproj
                InstallerProjectSrc.Save(InstallerProjectPath + @$"\{InstallerProjectName}.wixproj");
                #endregion

                #region Define the merged XML.
                XDocument mergedConnections = new XDocument(
                    new XElement(ns + "Wix",
                        new XElement(ns + "Fragment",
                            new XElement(ns + "ComponentGroup",
                                new XAttribute("Id", "Connections_files"),
                                new XAttribute("Directory", "Connections")
                            )
                        )
                    )
                );

                XDocument mergedScripts = new XDocument(
                    new XElement(ns + "Wix",
                        new XElement(ns + "Fragment",
                            new XElement(ns + "ComponentGroup",
                                new XAttribute("Id", "Scripts_files"),
                                new XAttribute("Directory", "Scripts")
                            )
                        )
                    )
                );
                #endregion

                #endregion

                #region loop over .wxs files to generate installer files. 
                if (wxsFiles.Count > 0)
                {
                    foreach (string wxsFile in wxsFiles)
                    {
                        XDocument wxsDoc = XDocument.Load(wxsFile);

                        #region Select the right merged XML.
                        XDocument selectedMergedFile;

                        if (RefConnections.Any(keyword => wxsFile.Contains(keyword)))
                        {
                            selectedMergedFile = mergedConnections;
                        }
                        else if (RefScripts.Any(keyword => wxsFile.Contains(keyword)))
                        {
                            selectedMergedFile = mergedScripts;
                        }
                        else
                        {
                            continue;
                        }
                        #endregion

                        #region Loop over all interesting directories with includes binaries or content.
                        foreach (XElement directoryRef in wxsDoc.Descendants(ns + "DirectoryRef"))
                        {
                            if (directoryRef is null) continue;
                            var dirAttributeID = directoryRef?.Attribute("Id")?.Value ?? "";

                            if (dirAttributeID.Contains("Binaries"))
                            {
                                selectedMergedFile.Root?.Element(ns + "Fragment")?.Element(ns + "ComponentGroup")?.Add(directoryRef?.Descendants(ns + "Component"));
                            }
                            // content needs to copy hole directory ref
                            else if (dirAttributeID.Contains("Content") && directoryRef != null)
                            {
                                foreach (var cmp in directoryRef.Descendants(ns + "Component"))
                                {
                                    var nextReference = cmp?.Element(ns + "File")?.Attribute("Source")?.Value.Split('\\').LastOrDefault();
                                    if (nextReference == null) continue;

                                    if (selectedMergedFile.Descendants(ns + "File").Any(s => s.Attribute("Source")?.Value?.Contains(nextReference) == true))
                                    {
                                        continue;
                                    }
                                    else
                                    {
                                        XElement newContentFragment = new XElement(ns + "Fragment",
                                                                        new XElement(ns + "DirectoryRef",
                                                                        new XAttribute("Id", (selectedMergedFile == mergedConnections) ? "Connections" : "Scripts")
                                                                        )
                                                                       );
                                        newContentFragment?.Element(ns + "DirectoryRef")?.Add(cmp);
                                        selectedMergedFile.Root?.Add(newContentFragment);
                                    }

                                }
                            }
                        }
                        #endregion
                    }
                    Console.WriteLine("Merging .wxs files complete.");
                }
                else
                {
                    Console.WriteLine("No .wxs files are found, default is UniversalBoardTestConfigRemote Installer");
                }
                #endregion

                #region create a list of all resources, only for local copy.
                List<string> AllScriptsFiles = mergedScripts.Descendants(ns + "File").Select(s => s.Attribute("Source").Value).ToList();
                List<string> AllConnectionsFiles = mergedConnections.Descendants(ns + "File").Select(s => s.Attribute("Source").Value).ToList();
                #endregion

                #region check for package references.
                var checkProjectReferences = includeAttributes.Select(s => s?.Replace("..", solutionDirectory)).ToList().Where(file => File.Exists(file)).ToList();

                foreach (var projectFile in checkProjectReferences)
                {
                    if (projectFile is null) continue;

                    var projectName = projectFile.Split("\\").Where(s => s.Contains(".csproj")).ToList()[0].Split(".")[0];
                    XDocument wxsProject = XDocument.Load(projectFile);
                    XNamespace projNamespace = wxsProject?.Root?.Attribute("xmlns")?.Value ?? "http://wixtoolset.org/schemas/v4/wxs";

                    if (wxsProject is null) continue;

                    foreach (XElement outputPath in wxsProject.Descendants(projNamespace + "OutputPath"))
                    {
                        if (outputPath.Value.Contains("Release")) continue;

                        var newUpdatedPath = "";
                        if (outputPath.Value.Contains("..\\..\\"))
                        {
                            newUpdatedPath = solutionDirectory + outputPath.Value.Split(@"..")[2];
                        }
                        else if (outputPath.Value.Contains(")\\"))
                        {
                            newUpdatedPath = solutionDirectory + outputPath.Value.Split(")\\")[1];
                        }
                        else
                        {
                            newUpdatedPath = Path.GetDirectoryName(projectFile) + "\\" + outputPath.Value;
                        }

                        AllScriptsFiles = AllScriptsFiles
                            .Select(path =>
                            {
                                var splitPath = path.Split(")\\")[0].Split(".");
                                if (splitPath.Length < 3 || !splitPath[1].Contains(projectName) || path.Contains(newUpdatedPath))
                                    return path;

                                return newUpdatedPath + path.Split(")\\")[1];
                            })
                            .ToList();

                        AllConnectionsFiles = AllConnectionsFiles
                            .Select(path =>
                            {
                                var splitPath = path.Split(")\\")[0].Split(".");
                                if (splitPath.Length < 3 || !splitPath[1].Contains(projectName) || path.Contains(newUpdatedPath))
                                    return path;

                                return newUpdatedPath + path.Split(")\\")[1];
                            })
                            .ToList();
                    }

                    if (projectFile.Contains("Scripts", StringComparison.OrdinalIgnoreCase))
                    {
                        foreach (XElement packageRef in wxsProject.Descendants(projNamespace + "HintPath"))
                        {
                            string additionalDllForInstaller = packageRef.Value;
                            string[] additionalFileNameArr = additionalDllForInstaller.Split("\\").ToArray();

                            // skip if dependency already exsits
                            if (additionalFileNameArr[additionalFileNameArr.Length - 1].Contains("Logger.dll")) continue;
                            if (additionalFileNameArr[additionalFileNameArr.Length - 1].Contains("UniversalBoardTestApp.exe")) continue;
                            if (AllScriptsFiles.Any(path => Path.GetFileName(path).Equals(additionalFileNameArr[additionalFileNameArr.Length - 1], StringComparison.OrdinalIgnoreCase))) continue;

                            string additionalPackage = packageRef.Value.Replace("$(SolutionDir)", solutionDirectory);
                            Console.WriteLine($"Additional Package References (Scripts) = {additionalPackage}");

                            AllScriptsFiles.Add(additionalPackage);

                            XElement newContentFragment = new XElement(ns + "Fragment",
                                                                       new XElement(ns + "DirectoryRef",
                                                                            new XAttribute("Id", "Scripts"),
                                                                            new XElement(ns + "Component",
                                                                                new XAttribute("Id", "cmp" + HashString(additionalDllForInstaller)),
                                                                                new XAttribute("Guid", "*"),
                                                                                new XElement(ns + "File",
                                                                                    new XAttribute("Id", "fil" + HashString(additionalDllForInstaller)),
                                                                                    new XAttribute("Source", additionalDllForInstaller)
                                                                                    )
                                                                                )));

                            mergedScripts.Root?.Add(newContentFragment);
                        }
                    }
                    else if (projectFile.Contains("Connections", StringComparison.OrdinalIgnoreCase))
                    {
                        foreach (XElement packageRef in wxsProject.Descendants(projNamespace + "HintPath"))
                        {
                            string additionalDllForInstaller = packageRef.Value;
                            string[] additionalFileNameArr = additionalDllForInstaller.Split("\\").ToArray();

                            if (additionalFileNameArr[additionalFileNameArr.Length - 1].Contains("Logger.dll")) continue;
                            if (additionalFileNameArr[additionalFileNameArr.Length - 1].Contains("UniversalBoardTestApp.exe")) continue;
                            if (AllConnectionsFiles.Any(path => Path.GetFileName(path).Equals(additionalFileNameArr[additionalFileNameArr.Length - 1], StringComparison.OrdinalIgnoreCase))) continue;

                            string additionalPackage = packageRef.Value.Replace("$(SolutionDir)", solutionDirectory);
                            Console.WriteLine($"Additional Package References (Connections) = {additionalPackage}");

                            //string[] package = additionalPackage.Split("..\\..\\..\\");
                            //Console.WriteLine($"package = {package[0]}");
                            //Console.WriteLine($"package1 = {package[1]}");
                            //additionalPackage = package[1];
                            //AllConnectionsFiles.Add("c:\\" + additionalPackage);
                            AllConnectionsFiles.Add(additionalPackage);

                            XElement newContentFragment = new XElement(ns + "Fragment",
                                                                       new XElement(ns + "DirectoryRef",
                                                                            new XAttribute("Id", "Connections"),
                                                                            new XElement(ns + "Component",
                                                                                new XAttribute("Id", "cmp" + HashString(additionalDllForInstaller)),
                                                                                new XAttribute("Guid", "*"),
                                                                                new XElement(ns + "File",
                                                                                    new XAttribute("Id", "fil" + HashString(additionalDllForInstaller)),
                                                                                    new XAttribute("Source", additionalDllForInstaller)
                                                                                    )
                                                                                )));

                            mergedConnections.Root?.Add(newContentFragment);
                        }
                    }
                }
                #endregion

                #region generate the component group of all dependencies of merged scripts.
                List<string> scriptComponentIds = new List<string>();

                XElement scriptsComponentRefs = new XElement(ns + "Fragment",
                        new XElement(ns + "ComponentGroup",
                            new XAttribute("Id", "Scripts.Content")
                            )
                        );

                if (checkProjectReferences.Any(path => path?.Contains("\\Scripts\\") == false))
                {
                    foreach (var component in mergedScripts.Descendants(ns + "DirectoryRef").Descendants(ns + "Component"))
                    {
                        string newCompId = Hash(component);
                        var cmpAttributeID = component.Attribute("Id");

                        if (cmpAttributeID != null)
                        {
                            cmpAttributeID.Value = newCompId;
                            scriptComponentIds.Add(newCompId);
                        }
                    }

                    scriptsComponentRefs?.Element(ns + "ComponentGroup")?.Add(scriptComponentIds.Select(id => new XElement(ns + "ComponentRef", new XAttribute("Id", id))));
                }

                mergedScripts.Root?.Add(scriptsComponentRefs);
                #endregion

                #region generate the component group of all dependencies of merged connections.
                List<string> connectionsComponentIds = new List<string>();

                XElement connectionsComponentRefs = new XElement(ns + "Fragment",
                        new XElement(ns + "ComponentGroup",
                            new XAttribute("Id", "Connections.Content")
                            )
                        );

                if (checkProjectReferences.Any(path => path?.Contains("\\Connections\\") == false))
                {
                    foreach (var component in mergedConnections.Descendants(ns + "DirectoryRef").Descendants(ns + "Component"))
                    {
                        string newCompId = Hash(component);
                        var cmpAttributeID = component.Attribute("ID");
                        if (cmpAttributeID != null)
                        {
                            cmpAttributeID.Value = newCompId;
                            connectionsComponentIds.Add(newCompId);
                        }
                    }

                    connectionsComponentRefs?.Element(ns + "ComponentGroup")?.Add(connectionsComponentIds.Select(id => new XElement(ns + "ComponentRef", new XAttribute("Id", id))));
                }

                mergedConnections.Root?.Add(connectionsComponentRefs);
                #endregion

                #region Save all changes to the files.
                mergedScripts.Save(outputFileScripts);
                mergedConnections.Save(outputFileConnections);

                if (!Directory.Exists(locationOfFileCopy))
                {
                    Directory.CreateDirectory(locationOfFileCopy);
                }

                File.WriteAllLines(locationOfFileCopy + @"\AllScriptsFiles.txt", AllScriptsFiles);
                File.WriteAllLines(locationOfFileCopy + @"\AllConnectionsFiles.txt", AllConnectionsFiles);
                //File.WriteAllText(locationOfFileCopy + @"\Test.txt", solutionDirectory);
                #endregion
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        private static string HashString(string input) => SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(input)).Aggregate(new StringBuilder(), (sb, b) => sb.Append(b.ToString("X2"))).ToString();

        private static string Hash(XElement value)
        {
            XNamespace ns = "http://wixtoolset.org/schemas/v4/wxs";

            if (string.IsNullOrEmpty(value.Element(ns + "File")?.Attribute("Source")?.Value))
            {
                return "cmp" + value.Element(ns + "File")?.Attribute("Source")?.Value;
            }

            var elementAttributeSrc = value?.Element(ns + "File")?.Attribute("Source");

            if (elementAttributeSrc is null) return "HashCreationError";

            if (elementAttributeSrc.Value.Contains("Dir)\\"))
            {
                return "cmp" + SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(elementAttributeSrc.Value.Split("Dir)\\")[1])).Aggregate(new StringBuilder(), (sb, b) => sb.Append(b.ToString("X2"))).ToString();
            }
            else
            {
                return "cmp" + SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(elementAttributeSrc.Value)).Aggregate(new StringBuilder(), (sb, b) => sb.Append(b.ToString("X2"))).ToString();
            }
        }
    }
}