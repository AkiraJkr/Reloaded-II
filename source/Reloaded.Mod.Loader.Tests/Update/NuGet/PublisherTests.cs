﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Reloaded.Mod.Loader.IO;
using Reloaded.Mod.Loader.IO.Config;
using Reloaded.Mod.Loader.Update.Packaging;
using Sewer56.DeltaPatchGenerator.Lib.Utility;
using Sewer56.Update.Packaging.Enums;
using Sewer56.Update.Packaging.Extractors;
using Xunit;

namespace Reloaded.Mod.Loader.Tests.Update.NuGet;

public class PublisherTests
{
    private static string TestArchiveFile = "HeroesControllerPostProcess.zip";
    private static string TestArchiveFileOld = "HeroesControllerPostProcessOld.zip";

    [Fact]
    public async Task CanAutoGenerateDelta()
    {
        // Arrange
        using var oldVersionFolder = new TemporaryFolderAllocation();
        using var curVersionFolder = new TemporaryFolderAllocation();
        var zipUnarchiver = new ZipPackageExtractor();
        
        await zipUnarchiver.ExtractPackageAsync(TestArchiveFile, curVersionFolder.FolderPath);
        await zipUnarchiver.ExtractPackageAsync(TestArchiveFileOld, oldVersionFolder.FolderPath);

        // Act
        using var outputFolder = new TemporaryFolderAllocation();
        var modTuple = ConfigReader<ModConfig>.ReadConfigurations(curVersionFolder.FolderPath, ModConfig.ConfigFileName)[0];
        var metadata = await Publisher.PublishAsync(new Publisher.PublishArgs()
        {
            OutputFolder = outputFolder.FolderPath,
            OlderVersionFolders = new List<string>() { oldVersionFolder.FolderPath },
            ModTuple = modTuple
        });
        
        // Assert
        Assert.Equal(2, metadata.Releases.Count);
        Assert.Contains(metadata.Releases, item => item.ReleaseType == PackageType.Delta);
    }
}