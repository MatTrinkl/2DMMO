using System.Text.Json;
using Mmo.Shared.Prefab.Configurations;
using Mmo.Shared.Prefab.Serialization;

namespace Mmo.Shared.Tests.Entities;

public class PrefabLoaderTests
{
    [Fact]
    public void LoadPrefabConfig_WithValidFile_LoadsSuccessfully()
    {
        // Create a temporary JSON file
        string tempFile = Path.GetTempFileName();
        string jsonFile = Path.ChangeExtension(tempFile, ".json");
        File.Move(tempFile, jsonFile);

        try
        {
            string json = @"{
  ""prefabs"": [
    {
      ""id"": 1,
      ""name"": ""testPlayer"",
      ""category"": ""player"",
      ""description"": ""Test player""
    }
  ]
}";
            File.WriteAllText(jsonFile, json);

            PrefabsConfig config = PrefabLoader.LoadPrefabConfig(jsonFile);

            Assert.NotNull(config);
            Assert.Single(config.Prefabs);
            Assert.Equal((ushort)1, config.Prefabs[0].Id);
            Assert.Equal("testPlayer", config.Prefabs[0].Name);
        }
        finally
        {
            if (File.Exists(jsonFile))
                File.Delete(jsonFile);
        }
    }

    [Fact]
    public void LoadPrefabConfig_WithNonExistentFile_ThrowsFileNotFoundException()
    {
        Assert.Throws<FileNotFoundException>(() =>
            PrefabLoader.LoadPrefabConfig("nonexistent.json"));
    }

    [Fact]
    public void LoadPrefabConfig_WithNonJsonFile_ThrowsArgumentException()
    {
        string tempFile = Path.GetTempFileName();
        string txtFile = Path.ChangeExtension(tempFile, ".txt");
        File.Move(tempFile, txtFile);

        try
        {
            Assert.Throws<ArgumentException>(() =>
                PrefabLoader.LoadPrefabConfig(txtFile));
        }
        finally
        {
            if (File.Exists(txtFile))
                File.Delete(txtFile);
        }
    }

    [Fact]
    public void LoadPrefabConfig_WithInvalidJson_ThrowsException()
    {
        string tempFile = Path.GetTempFileName();
        string jsonFile = Path.ChangeExtension(tempFile, ".json");
        File.Move(tempFile, jsonFile);

        try
        {
            File.WriteAllText(jsonFile, "invalid json");

            Assert.Throws<JsonException>(() =>
                PrefabLoader.LoadPrefabConfig(jsonFile));
        }
        finally
        {
            if (File.Exists(jsonFile))
                File.Delete(jsonFile);
        }
    }
}
